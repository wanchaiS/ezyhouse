using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.FurnitureItems;

public sealed class FurnitureService(
    IFurnitureRepository furniture,
    ICurrentUserContext currentUser,
    SplitCalculator splitCalculator) : IFurnitureService
{
    public async Task<IReadOnlyList<FurnitureDto>> ListAsync(FurnitureStatus? status)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        IReadOnlyList<Furniture> items = await furniture.ListAsync(household.Id, status);
        return items.Select(item => item.ToDto()).ToList();
    }

    public async Task<FurnitureDto> GetAsync(int id)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        return (await LoadAsync(household.Id, id)).ToDto();
    }

    public async Task<FurnitureDto> CreateAsync(FurnitureInput request)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        var item = new Furniture
        {
            HouseholdId = household.Id,
            Name = request.Name!.Trim(),
            CreatedByUserId = currentUser.UserId,
        };
        item.Transactions.Add(BuildPurchase(household, request.Purchase!));

        furniture.Add(item);
        await furniture.SaveChangesAsync();
        return await ReloadAsync(household.Id, item.Id);
    }

    public async Task<FurnitureDto> UpdateAsync(int id, FurnitureInput request)
    {
        (Household household, Furniture item) = await LoadManageableAsync(id);
        item.Edit(request.Name!.Trim(), BuildPurchase(household, request.Purchase!));
        await furniture.SaveChangesAsync();
        return await ReloadAsync(household.Id, item.Id);
    }

    public async Task DeleteAsync(int id)
    {
        (_, Furniture item) = await LoadManageableAsync(id);
        furniture.Remove(item);
        await furniture.SaveChangesAsync();
    }

    public async Task<FurnitureDto> SellAsync(int id, SaleInput request)
    {
        (Household household, Furniture item) = await LoadManageableAsync(id);
        var sale = new FurnitureSale();
        Fill(sale, household, request.Date!.Value, request.Amount!.Value, request.ReceivedByUserId!.Value,
            request.SplitType!.Value, request.Shares!, "The person receiving the money");
        item.RecordSale(sale);
        await furniture.SaveChangesAsync();
        return await ReloadAsync(household.Id, item.Id);
    }

    public async Task<FurnitureDto> DisposeAsync(int id, DisposalInput request)
    {
        (Household household, Furniture item) = await LoadManageableAsync(id);
        var disposal = new FurnitureDisposal();
        Fill(disposal, household, request.Date!.Value, request.Amount!.Value, request.PaidByUserId!.Value,
            request.SplitType!.Value, request.Shares!, "The person who paid");
        item.RecordDisposal(disposal);
        await furniture.SaveChangesAsync();
        return await ReloadAsync(household.Id, item.Id);
    }

    private FurniturePurchase BuildPurchase(Household household, PurchaseInput input)
    {
        var purchase = new FurniturePurchase();
        Fill(purchase, household, input.Date!.Value, input.Amount!.Value, input.PaidByUserId!.Value,
            input.SplitType!.Value, input.Shares!, "The person who paid");
        return purchase;
    }

    private void Fill(
        FurnitureTransaction transaction,
        Household household,
        DateOnly date,
        decimal amount,
        int counterpartyUserId,
        SplitType splitType,
        List<ShareInput> shares,
        string counterpartyLabel)
    {
        var memberIds = household.Members.Select(member => member.Id).ToHashSet();
        if (!memberIds.Contains(counterpartyUserId))
            throw new ValidationFailedException($"{counterpartyLabel} must be a current household member.");

        transaction.Date = date;
        transaction.Amount = amount;
        transaction.CounterpartyUserId = counterpartyUserId;
        transaction.SplitType = splitType;
        transaction.Shares = splitCalculator.Split(amount, splitType, shares, memberIds).ToList();
    }

    private async Task<(Household Household, Furniture Item)> LoadManageableAsync(int id)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        Furniture item = await LoadAsync(household.Id, id);
        int userId = currentUser.UserId;
        if (item.CreatedByUserId != userId && household.PrimaryUserId != userId)
            throw new ForbiddenException("Only the person who added this item or the primary housemate can change it.");

        return (household, item);
    }

    private async Task<Furniture> LoadAsync(int householdId, int id) =>
        await furniture.FindAsync(householdId, id)
        ?? throw new NotFoundException("That furniture item doesn't exist.");

    // New transactions only carry user ids, so read the item back to fill in the names the response shows.
    private async Task<FurnitureDto> ReloadAsync(int householdId, int id) =>
        (await LoadAsync(householdId, id)).ToDto();
}
