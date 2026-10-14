using EzyHouse.Domain.Enums;

namespace EzyHouse.Api.Features.Bills;

public interface IBillService
{
    Task<IReadOnlyList<BillTypeDto>> ListTypesAsync();

    Task<BillTypeDto> CreateTypeAsync(BillTypeInput request);

    Task<BillTypeDto> UpdateTypeAsync(int id, BillTypeInput request);

    Task DeleteTypeAsync(int id);

    Task<IReadOnlyList<BillDto>> ListAsync(BillStatus? status);

    Task<BillDto> SubmitAsync(int id, SubmitBillInput request);

    Task<BillDto> SetSharePaidAsync(int billId, int userId, bool paid);
}
