namespace EzyHouse.Api.Features.Bills;

public interface IBillService
{
    Task<IReadOnlyList<BillTypeDto>> ListTypesAsync();

    Task<BillTypeDto> CreateTypeAsync(BillTypeInput request);

    Task<BillTypeDto> UpdateTypeAsync(int id, BillTypeInput request);

    Task DeleteTypeAsync(int id);
}
