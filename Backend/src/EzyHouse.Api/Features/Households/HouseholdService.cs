using System.Security.Cryptography;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.Scheduling;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Households;

public sealed class HouseholdService(
    IHouseholdRepository households,
    ICurrentUserContext currentUser,
    IScheduleRunner runner,
    IHouseholdBalances balances,
    SplitCalculator splitCalculator,
    IClock clock) : IHouseholdService
{
    // Uppercase letters and digits without the look-alikes 0/O and 1/I, so a code read aloud
    // or copied by hand can't be mistyped.
    private const string InviteCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int InviteCodeLength = 8;

    private const string AlreadyInHousehold = "You're already in a household. Leave it first.";
    private const string InviteCodeNotFound = "Invite code not found.";
    private const string OpenDebts = "You still owe money, or are owed money. Settle up first.";

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

        await HandOverAsync(household, newPrimary);
        await households.SaveChangesAsync();

        return household.ToDto();
    }

    public async Task LeaveAsync(LeaveInput request)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        int userId = currentUser.UserId;
        User? nextPrimary = ChooseNextPrimary(household, userId, request.NextPrimaryUserId);

        // Rent or a bill due today but not yet created by the hourly job still counts in the debts below.
        await runner.RunDueForHouseholdAsync(household.Id);

        var (_, debts) = await balances.CalculateAsync(household.Id, household.Members.Select(member => member.Id));

        // Debts, not the net balance: two debts that cancel out still leave the user owing someone
        // and owed by someone else, and nobody could settle them once the user has gone.
        Func<Debt, bool> involvesLeaver = delegate (Debt debt)
        {
            return debt.FromUserId == userId || debt.ToUserId == userId;
        };
        if (debts.Any(involvesLeaver))
            throw new BusinessRuleException(OpenDebts);

        if (nextPrimary is not null)
            await HandOverAsync(household, nextPrimary);

        await TakeOffSchedulesAsync(household, userId);

        // Expense shares stay as history; if this was the last member, the household stays as an archive.
        User user = await currentUser.GetUserAsync();
        user.HouseholdId = null;
        await households.SaveChangesAsync();
    }

    /// <summary>
    /// Null when nobody has to take over: the leaver isn't primary, or is the last member.
    /// A primary leaving others behind must name one of them (400).
    /// </summary>
    private static User? ChooseNextPrimary(Household household, int userId, int? nextPrimaryUserId)
    {
        if (household.PrimaryUserId != userId || household.Members.Count <= 1)
            return null;

        if (nextPrimaryUserId is null)
            throw new ValidationFailedException("You're the primary, so choose who becomes primary before you leave.");

        return household.Members.SingleOrDefault(member => member.Id == nextPrimaryUserId && member.Id != userId)
            ?? throw new ValidationFailedException("The next primary must be another member of this household.");
    }

    // Everything that fell due before the handover is created first, so it stays paid by the outgoing
    // primary; rent and bills created from now on are paid by the new one.
    private async Task HandOverAsync(Household household, User newPrimary)
    {
        await runner.RunDueForHouseholdAsync(household.Id);
        household.PrimaryUserId = newPrimary.Id;
        household.PrimaryUser = newPrimary;
    }

    // The leaver stops sharing future rent and bills. Everyone left shares the rent equally, since an
    // exact split no longer adds up; a schedule left with nobody ends today.
    private async Task TakeOffSchedulesAsync(Household household, int userId)
    {
        DateOnly today = clock.Today;
        HashSet<int> remainingMemberIds = household.Members
            .Where(member => member.Id != userId)
            .Select(member => member.Id)
            .ToHashSet();

        foreach (RentSchedule schedule in await households.GetRentSchedulesWithShareAsync(household.Id, userId))
        {
            List<ShareInput> others = schedule.Shares
                .Where(share => share.UserId != userId)
                .Select(share => new ShareInput(share.UserId, null))
                .ToList();
            if (others.Count == 0)
            {
                EndToday(schedule, today);
                schedule.Shares = [];
                continue;
            }

            schedule.SplitType = SplitType.Equal;
            schedule.Shares = splitCalculator
                .Split(schedule.Amount, SplitType.Equal, others, remainingMemberIds)
                .ToList();
        }

        foreach (BillType billType in await households.GetBillTypesWithMemberAsync(household.Id, userId))
        {
            billType.Members.RemoveAll(member => member.Id == userId);
            if (billType.Members.Count == 0)
                EndToday(billType, today);
        }
    }

    private static void EndToday<TOccurrence>(RecurringSchedule<TOccurrence> schedule, DateOnly today)
        where TOccurrence : Expense
    {
        if (schedule.EndDate is null || schedule.EndDate > today)
            schedule.EndDate = today;
        schedule.NextDueDate = null;
    }

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
