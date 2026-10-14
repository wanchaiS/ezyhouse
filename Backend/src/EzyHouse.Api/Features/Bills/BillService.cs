using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Bills;

public sealed class BillService(
    IBillRepository bills,
    ICurrentUserContext currentUser,
    IRecurringChargeGenerator generator,
    SplitCalculator splitCalculator,
    IClock clock) : IBillService
{
    public async Task<IReadOnlyList<BillTypeDto>> ListTypesAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        List<BillType> types = await bills.ListTypesAsync(household.Id);
        return types.Select(type => type.ToDto(clock.Today)).ToList();
    }

    public async Task<BillTypeDto> CreateTypeAsync(BillTypeInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        var billType = new BillType { HouseholdId = household.Id };
        Apply(billType, request, household);
        bills.AddType(billType);
        await bills.SaveChangesAsync();
        return billType.ToDto(clock.Today);
    }

    public async Task<BillTypeDto> UpdateTypeAsync(int id, BillTypeInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        BillType billType = await bills.FindTypeAsync(household.Id, id)
            ?? throw new NotFoundException("Bill type not found.");
        Apply(billType, request, household);
        await bills.SaveChangesAsync();
        return billType.ToDto(clock.Today);
    }

    public async Task DeleteTypeAsync(int id)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        BillType billType = await bills.FindTypeAsync(household.Id, id)
            ?? throw new NotFoundException("Bill type not found.");

        // Unpaid placeholders go with the type; submitted bills are history and stay, keeping their
        // name while the database clears the type id.
        bills.RemoveBills(await bills.ListPendingBillsOfTypeAsync(household.Id, id));
        bills.RemoveType(billType);
        await bills.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<BillDto>> ListAsync(BillStatus? status)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        await generator.GenerateBillPlaceholdersAsync(household, clock.Today);
        List<Bill> list = await bills.ListBillsAsync(household.Id, status);
        return list.Select(bill => bill.ToDto()).ToList();
    }

    public async Task<BillDto> SubmitAsync(int id, SubmitBillInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        Bill bill = await bills.FindBillAsync(household.Id, id)
            ?? throw new NotFoundException("Bill not found.");
        if (bill.Status != BillStatus.Pending)
            throw new BusinessRuleException("This bill is already submitted; its amount can't be changed.");

        HashSet<int> memberIds = household.Members.Select(member => member.Id).ToHashSet();
        decimal amount = request.Amount!.Value;
        IReadOnlyList<ShareInput> shareInputs = request.SplitType switch
        {
            SplitType.Equal => EqualSplitMembers(bill, memberIds),
            _ => request.Shares is { Count: > 0 } shares
                ? shares
                : throw new ValidationFailedException("An exact split needs the shares."),
        };

        IReadOnlyList<Share> split = splitCalculator.Split(amount, request.SplitType!.Value, shareInputs, memberIds);
        bill.Submit(amount, split, currentUser.UserId, clock.Today);
        await bills.SaveChangesAsync();
        return await ReloadAsync(household.Id, bill.Id);
    }

    public async Task<BillDto> SetSharePaidAsync(int billId, int userId, bool paid)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        if (currentUser.UserId != userId && currentUser.UserId != household.PrimaryUserId)
            throw new ForbiddenException("Only that person or the primary housemate can tick this share.");

        Bill bill = await bills.FindBillAsync(household.Id, billId)
            ?? throw new NotFoundException("Bill not found.");
        bill.SetSharePaid(userId, paid, clock.Today);
        await bills.SaveChangesAsync();
        return bill.ToDto();
    }

    // Ordered by user id so the leftover cent always lands on the same person.
    private static List<ShareInput> EqualSplitMembers(Bill bill, IReadOnlySet<int> memberIds)
    {
        if (bill.BillType is null)
            throw new ValidationFailedException("This bill's type was deleted, so it can only be split exactly.");

        List<ShareInput> members = bill.BillType.Members
            .Where(member => memberIds.Contains(member.Id))
            .OrderBy(member => member.Id)
            .Select(member => new ShareInput(member.Id, null))
            .ToList();
        return members.Count > 0
            ? members
            : throw new ValidationFailedException("This bill type has no current members to split between.");
    }

    // A fresh read so the new shares come back with their users loaded.
    private async Task<BillDto> ReloadAsync(int householdId, int billId) =>
        (await bills.FindBillAsync(householdId, billId))!.ToDto();

    private static void Apply(BillType billType, BillTypeInput request, Household household)
    {
        if (request.EndDate is { } end && end < request.StartDate!.Value)
            throw new ValidationFailedException("The end date can't be before the start date.");

        List<int> requested = request.MemberIds!.Distinct().ToList();
        List<User> members = household.Members.Where(member => requested.Contains(member.Id)).ToList();
        if (members.Count != requested.Count)
            throw new ValidationFailedException("Every bill type member must be a current household member.");

        billType.Name = request.Name!.Trim();
        billType.Frequency = request.Frequency!.Value;
        billType.StartDate = request.StartDate!.Value;
        billType.EndDate = request.EndDate;
        billType.Members.Clear();
        billType.Members.AddRange(members);
    }
}
