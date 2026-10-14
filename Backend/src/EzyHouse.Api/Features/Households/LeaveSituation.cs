using EzyHouse.Domain.Entities;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Households;

/// <summary>
/// Everything the leave checks look at, loaded in one go so each check is a plain in-memory rule.
/// </summary>
public sealed record LeaveSituation(
    int UserId,
    bool IsPrimary,
    int MemberCount,
    DateOnly Today,
    IReadOnlyList<Debt> Debts,
    IReadOnlyList<RentPeriod> UnpaidRentPeriods,
    IReadOnlyList<RentSchedule> RentSchedules,
    IReadOnlyList<Bill> UnpaidBills);
