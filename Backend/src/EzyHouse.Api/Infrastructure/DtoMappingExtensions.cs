using EzyHouse.Api.Dtos;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Infrastructure;

public static class DtoMappingExtensions
{
    public static UserRefDto ToRef(this User user) => new(user.Id, user.Name);

    public static UserDto ToDto(this User user) => new(user.Id, user.Name, user.Email, user.HouseholdId);

    public static ShareDto ToDto(this Share share) => new(Ref(share.User, share.UserId), share.Amount);

    public static PaidShareDto ToDto(this PaidShare share) =>
        new(Ref(share.User, share.UserId), share.Amount, share.Paid, share.PaidDate);

    public static HouseholdDto ToDto(this Household household) => new(
        household.Id,
        household.Name,
        household.InviteCode,
        Ref(household.PrimaryUser, household.PrimaryUserId),
        household.Members.Select(member => member.ToRef()).ToList());

    public static ExpenseDto ToDto(this Expense expense) => new(
        expense.Id,
        expense.Description,
        expense.Amount,
        expense.Date,
        Ref(expense.PaidBy, expense.PaidByUserId),
        Ref(expense.CreatedBy, expense.CreatedByUserId),
        expense.SplitType,
        expense.Shares.Select(share => share.ToDto()).ToList());

    public static SettlementDto ToDto(this Settlement settlement) => new(
        settlement.Id,
        Ref(settlement.From, settlement.FromUserId),
        Ref(settlement.To, settlement.ToUserId),
        settlement.Amount,
        settlement.Date,
        Ref(settlement.CreatedBy, settlement.CreatedByUserId));

    public static RentScheduleDto ToDto(this RentSchedule schedule, DateOnly today) => new(
        schedule.Id,
        schedule.Amount,
        schedule.Frequency,
        schedule.StartDate,
        schedule.EndDate,
        schedule.SplitType,
        schedule.Shares.Select(share => share.ToDto()).ToList(),
        schedule.GetNextDueDate(today));

    public static RentPeriodDto ToDto(this RentPeriod period) => new(
        period.Id,
        period.ScheduleId,
        period.StartDate,
        period.EndDate,
        period.Amount,
        Ref(period.Payee, period.PayeeUserId),
        period.Shares.Select(share => share.ToDto()).ToList());

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
        bill.PayeeUserId is int payeeId ? Ref(bill.Payee, payeeId) : null,
        bill.SubmittedDate,
        bill.Shares.Select(share => share.ToDto()).ToList());

    public static FurnitureDto ToDto(this Furniture furniture) => new(
        furniture.Id,
        furniture.Name,
        furniture.Status,
        Ref(furniture.CreatedBy, furniture.CreatedByUserId),
        furniture.Purchase.ToPurchaseDto(),
        furniture.Sale?.ToSaleDto(),
        furniture.Disposal?.ToDisposalDto());

    public static PurchaseDto ToPurchaseDto(this FurniturePurchase purchase) => new(
        purchase.Date,
        purchase.Amount,
        Ref(purchase.Counterparty, purchase.CounterpartyUserId),
        purchase.SplitType,
        purchase.Shares.Select(share => share.ToDto()).ToList());

    public static SaleDto ToSaleDto(this FurnitureSale sale) => new(
        sale.Date,
        sale.Amount,
        Ref(sale.Counterparty, sale.CounterpartyUserId),
        sale.SplitType,
        sale.Shares.Select(share => share.ToDto()).ToList());

    public static DisposalDto ToDisposalDto(this FurnitureDisposal disposal) => new(
        disposal.Date,
        disposal.Amount,
        Ref(disposal.Counterparty, disposal.CounterpartyUserId),
        disposal.SplitType,
        disposal.Shares.Select(share => share.ToDto()).ToList());

    public static ContributionDto ToDto(this BondContribution contribution) =>
        new(Ref(contribution.User, contribution.UserId), contribution.Amount, contribution.Date);

    public static BondDto ToDto(this Bond bond) => new(
        bond.Id,
        bond.Total,
        bond.PaidDate,
        bond.Contributions.Select(contribution => contribution.ToDto()).ToList(),
        bond.Outstanding);

    public static BalancesDto ToBalancesDto(
        IReadOnlyDictionary<int, decimal> nets,
        IReadOnlyList<Debt> debts,
        IReadOnlyDictionary<int, User> users) => new(
        nets.Select(pair => new MemberBalanceDto(Ref(UserOrNull(users, pair.Key), pair.Key), pair.Value)).ToList(),
        debts.Select(debt => new DebtDto(
            Ref(UserOrNull(users, debt.FromUserId), debt.FromUserId),
            Ref(UserOrNull(users, debt.ToUserId), debt.ToUserId),
            debt.Amount)).ToList());

    private static User? UserOrNull(IReadOnlyDictionary<int, User> users, int id) =>
        users.TryGetValue(id, out User? user) ? user : null;

    private static UserRefDto Ref(User? user, int id) =>
        user?.ToRef() ?? throw new InvalidOperationException($"User {id} was not loaded.");
}
