using System.Security.Cryptography;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Households;

public sealed class HouseholdService(
    IHouseholdRepository households,
    ICurrentUserContext currentUser,
    IRecurringChargeGenerator charges,
    IClock clock) : IHouseholdService
{
    // Uppercase letters and digits without the look-alikes 0/O and 1/I, so a code read aloud
    // or copied by hand can't be mistyped.
    private const string InviteCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int InviteCodeLength = 8;

    private const string AlreadyInHousehold = "You're already in a household. Leave it first.";
    private const string InviteCodeNotFound = "Invite code not found.";

    public async Task<HouseholdDto> CreateAsync(CreateHouseholdRequest request)
    {
        User user = await currentUser.GetUserAsync();
        if (user.HouseholdId is not null)
            throw new BusinessRuleException(AlreadyInHousehold);

        var household = new Household
        {
            Name = request.Name!.Trim(),
            InviteCode = await NewInviteCodeAsync(),
            PrimaryUserId = user.Id,
        };
        households.Add(household);
        await households.SaveChangesAsync();

        // Saved separately: the household needs the user as primary and the user needs the
        // household's id, and inserting both at once would be a cycle.
        user.HouseholdId = household.Id;
        await households.SaveChangesAsync();

        return household.ToDto();
    }

    public async Task<HouseholdDto> GetAsync()
    {
        User user = await currentUser.GetUserAsync();
        Household? household = user.HouseholdId is int householdId
            ? await households.FindAsync(householdId)
            : null;

        return household?.ToDto() ?? throw new NotFoundException("You aren't in a household.");
    }

    public async Task<HouseholdDto> JoinAsync(JoinHouseholdRequest request)
    {
        User user = await currentUser.GetUserAsync();
        if (user.HouseholdId is not null)
            throw new BusinessRuleException(AlreadyInHousehold);

        Household? household = await households.FindByInviteCodeAsync(request.InviteCode!.Trim().ToUpperInvariant());

        // A household with no members is an archive left behind by its last member; its code no longer works.
        if (household is null || household.Members.Count == 0)
            throw new NotFoundException(InviteCodeNotFound);

        // Joining adds the user to no rent schedule or bill type; the primary does that.
        user.HouseholdId = household.Id;
        await households.SaveChangesAsync();

        return household.ToDto();
    }

    public async Task<HouseholdDto> TransferPrimaryAsync(TransferPrimaryRequest request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        User newPrimary = household.Members.SingleOrDefault(member => member.Id == request.UserId)
            ?? throw new ValidationFailedException("That person isn't a member of this household.");

        // Rent periods are only created when someone reads them, so create every period already due
        // before switching: those belong to the outgoing primary. Existing rent periods and bills keep
        // their payee, so unpaid shares stay owed to the old primary (API.md).
        await charges.GenerateRentPeriodsAsync(household, clock.Today);
        household.PrimaryUserId = newPrimary.Id;
        household.PrimaryUser = newPrimary;
        await households.SaveChangesAsync();

        return household.ToDto();
    }

    public async Task LeaveAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        int userId = currentUser.UserId;

        // Without this, rent for a period nobody has opened the Rent tab for yet would be missed.
        await charges.GenerateRentPeriodsAsync(household, clock.Today);

        LeaveSituation situation = await households.GetLeaveSituationAsync(household, userId, clock.Today);
        List<string> problems = new[]
        {
            OpenDebtsProblem(situation),
            UnpaidRentProblem(situation),
            ActiveRentScheduleProblem(situation),
            UnpaidBillsProblem(situation),
            PrimaryWithOthersProblem(situation),
        }.OfType<string>().ToList();

        if (problems.Count > 0)
            throw new BusinessRuleException(string.Join("\n", problems));

        foreach (BillType billType in await households.GetBillTypesWithMemberAsync(household.Id, userId))
            billType.Members.RemoveAll(member => member.Id == userId);

        // Furniture shares stay as history; if this was the last member, the household stays as an archive.
        User user = await currentUser.GetUserAsync();
        user.HouseholdId = null;
        await households.SaveChangesAsync();
    }

    // Debts, not the net balance: two debts that cancel out still leave the user owing someone
    // and owed by someone else, and nobody could settle them once the user has gone.
    private static string? OpenDebtsProblem(LeaveSituation situation) =>
        situation.Debts.Any(debt => debt.FromUserId == situation.UserId || debt.ToUserId == situation.UserId)
            ? "You still owe money, or are owed money, for groceries or furniture. Settle up first."
            : null;

    private static string? UnpaidRentProblem(LeaveSituation situation)
    {
        Func<PaidShare, bool> isUnpaid = delegate (PaidShare share) { return !share.Paid; };

        bool blocked = situation.UnpaidRentPeriods.Any(period =>
            period.Shares.Any(share => share.UserId == situation.UserId && isUnpaid(share)) ||
            (period.PayeeUserId == situation.UserId && period.Shares.Any(isUnpaid)));

        return blocked ? "A rent share owed by you or to you is unpaid." : null;
    }

    private static string? ActiveRentScheduleProblem(LeaveSituation situation) =>
        situation.RentSchedules.Any(schedule =>
            (schedule.EndDate is null || schedule.EndDate >= situation.Today) &&
            schedule.Shares.Any(share => share.UserId == situation.UserId))
            ? "You're in the split of a rent schedule that hasn't ended. Ask the primary to end it or take you off it."
            : null;

    private static string? UnpaidBillsProblem(LeaveSituation situation)
    {
        bool blocked = situation.UnpaidBills.Any(bill =>
            bill.Status == BillStatus.Submitted &&
            (bill.Shares.Any(share => share.UserId == situation.UserId && !share.Paid) ||
             (bill.PayeeUserId == situation.UserId && bill.Shares.Any(share => !share.Paid))));

        return blocked ? "A bill share owed by you or to you is unpaid." : null;
    }

    private static string? PrimaryWithOthersProblem(LeaveSituation situation) =>
        situation.IsPrimary && situation.MemberCount > 1
            ? "You're the primary and others are still in the household. Hand over primary first."
            : null;

    private async Task<string> NewInviteCodeAsync()
    {
        string code;
        do
        {
            code = new string(RandomNumberGenerator.GetItems<char>(InviteCodeAlphabet, InviteCodeLength));
        }
        while (await households.InviteCodeExistsAsync(code));

        return code;
    }
}
