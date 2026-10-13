using LinkAnalytics.Application.DTOs.Profiles;
using LinkAnalytics.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LinkAnalytics.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public sealed class ProfilesController(IProfileService profileService) : ControllerBase
{
    [HttpGet("{slug}")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileResponse>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var profile = await profileService.GetBySlugAsync(slug, cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }
}
