using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.FurnitureItems;

public static class FurnitureMappings
{
    public static FurnitureDto ToDto(this Furniture furniture) => new(
        furniture.Id,
        furniture.Name,
        furniture.Status,
        CoreMappings.Ref(furniture.CreatedBy, furniture.CreatedByUserId),
        furniture.Purchase.ToPurchaseDto(),
        furniture.Sale?.ToSaleDto(),
        furniture.Disposal?.ToDisposalDto());

    public static PurchaseDto ToPurchaseDto(this FurniturePurchase purchase) => new(
        purchase.Date,
        purchase.Amount,
        CoreMappings.Ref(purchase.Counterparty, purchase.CounterpartyUserId),
        purchase.SplitType,
        purchase.Shares.Select(share => share.ToDto()).ToList());

    public static SaleDto ToSaleDto(this FurnitureSale sale) => new(
        sale.Date,
        sale.Amount,
        CoreMappings.Ref(sale.Counterparty, sale.CounterpartyUserId),
        sale.SplitType,
        sale.Shares.Select(share => share.ToDto()).ToList());

    public static DisposalDto ToDisposalDto(this FurnitureDisposal disposal) => new(
        disposal.Date,
        disposal.Amount,
        CoreMappings.Ref(disposal.Counterparty, disposal.CounterpartyUserId),
        disposal.SplitType,
        disposal.Shares.Select(share => share.ToDto()).ToList());
}
