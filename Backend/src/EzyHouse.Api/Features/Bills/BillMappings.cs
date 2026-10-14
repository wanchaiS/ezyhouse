using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Bills;

public static class BillMappings
{
    public static BillTypeDto ToDto(this BillType billType, DateOnly today) => new(
        billType.Id,
        billType.Name,
        billType.Frequency,
        billType.StartDate,
        billType.EndDate,
        billType.Members.Select(member => member.ToRef()).ToList(),
        billType.GetNextDueDate(today));

    public static BillDto ToDto(this Bill bill) => new(
        bill.Id,
        new BillTypeRefDto(bill.BillTypeId, bill.BillTypeName),
        bill.DueDate,
        bill.Status,
        bill.Amount,
        bill.PayeeUserId is int payeeId ? CoreMappings.Ref(bill.Payee, payeeId) : null,
        bill.SubmittedDate,
        bill.Shares.Select(share => share.ToDto()).ToList());
}
