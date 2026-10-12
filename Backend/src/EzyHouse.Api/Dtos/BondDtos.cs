using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Infrastructure;

namespace EzyHouse.Api.Dtos;

public sealed record ContributionDto(UserRefDto User, decimal Amount, DateOnly Date);

public sealed record BondDto(
    int Id,
    decimal Total,
    DateOnly PaidDate,
    IReadOnlyList<ContributionDto> Contributions,
    decimal Outstanding);

public sealed record ContributionInput(
    [property: Required] int? UserId,
    [property: Required, Money] decimal? Amount,
    [property: Required] DateOnly? Date);

public sealed record BondInput(
    [property: Required, Money] decimal? Total,
    [property: Required] DateOnly? PaidDate,
    [property: Required] List<ContributionInput>? Contributions);
