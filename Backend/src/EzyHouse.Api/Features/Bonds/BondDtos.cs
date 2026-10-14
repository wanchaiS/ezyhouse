using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;

namespace EzyHouse.Api.Features.Bonds;

public sealed record ContributionDto(UserRefDto User, decimal Amount, DateOnly Date);

public sealed record BondDto(
    int Id,
    decimal Total,
    DateOnly PaidDate,
    IReadOnlyList<ContributionDto> Contributions,
    decimal Outstanding);

public sealed record ContributionInput(
    [Required] int? UserId,
    [Required, Money] decimal? Amount,
    [Required] DateOnly? Date);

public sealed record BondInput(
    [Required, Money] decimal? Total,
    [Required] DateOnly? PaidDate,
    [Required] List<ContributionInput>? Contributions);
