using System.Security.Claims;
using LinkAnalytics.Application.DTOs.Profiles;
using LinkAnalytics.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkAnalytics.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public sealed class ProfilesController(IProfileService profileService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfileResponse>> Create(CreateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var profile = await profileService.CreateAsync(request with { UserId = userId }, cancellationToken);
        if (profile is null)
            return Conflict(new { message = "Profile could not be created. Check the data and make sure the slug is available." });

        return CreatedAtAction(nameof(GetBySlug), new { slug = profile.Slug }, profile);
    }

    [HttpPut("{profileId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int profileId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var updated = await profileService.UpdateAsync(profileId, userId, request, cancellationToken);
        if (!updated)
            return BadRequest(new { message = "Profile was not found, is not owned by you, or the supplied data is invalid." });

        return NoContent();
    }

    [HttpDelete("{profileId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int profileId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var deleted = await profileService.DeleteAsync(profileId, userId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{slug}")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileResponse>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var profile = await profileService.GetBySlugAsync(slug, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }
}