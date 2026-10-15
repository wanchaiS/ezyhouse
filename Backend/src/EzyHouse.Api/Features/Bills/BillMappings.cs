using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Bills;

public static class BillMappings
{
    public static BillTypeDto ToDto(this BillType billType) => new(
        billType.Id,
        billType.Name,
        billType.Frequency,
        billType.StartDate,
        billType.EndDate,
        billType.Members.Select(member => member.ToRef()).ToList(),
        billType.NextDueDate);
}
