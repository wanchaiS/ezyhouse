using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;

namespace EzyHouse.Api.Features.Balances;

public sealed record MemberBalanceDto(UserRefDto User, decimal Net);

public sealed record DebtDto(UserRefDto From, UserRefDto To, decimal Amount);

public sealed record BalancesDto(
    IReadOnlyList<MemberBalanceDto> Members,
    IReadOnlyList<DebtDto> Debts);

public sealed record SettlementDto(
    int Id,
    UserRefDto From,
    UserRefDto To,
    decimal Amount,
    DateOnly Date,
    UserRefDto CreatedBy);

public sealed record SettlementInput(
    [Required] int? FromUserId,
    [Required] int? ToUserId,
    [Required, Money] decimal? Amount);
