using System.Security.Claims;
using System.Text.RegularExpressions;
using LinkAnalytics.Application.DTOs.Profiles;
using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkAnalytics.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public sealed class ProfilesController(IProfileService profileService, ILinkService linkService) : ControllerBase
{
    private static readonly Regex SlugPattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant);

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

        if (!IsValidProfileData(request.Slug, request.Title, request.Bio))
            return BadRequest(new { message = "Profile slug, title, or bio is invalid." });

        var profile = await profileService.CreateAsync(request with { UserId = userId }, cancellationToken);
        if (profile is null)
            return Conflict(new { message = "A profile with this slug already exists." });

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

        if (profileId <= 0 || !IsValidProfileData(request.Slug, request.Title, request.Bio))
            return BadRequest(new { message = "Profile slug, title, or bio is invalid." });

        var updated = await profileService.UpdateAsync(profileId, userId, request, cancellationToken);
        return updated
            ? NoContent()
            : NotFound(new { message = "Profile was not found or does not belong to the current user." });
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

    [HttpPut("{profileId:int}/links/order")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderLinks(
        int profileId,
        ReorderLinksRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        if (profileId <= 0 || request.LinkIds is null || request.LinkIds.Count == 0 ||
            request.LinkIds.Any(id => id <= 0) ||
            request.LinkIds.Distinct().Count() != request.LinkIds.Count)
            return BadRequest(new { message = "Link IDs must be a non-empty list of distinct positive IDs." });

        var reordered = await linkService.ReorderAsync(profileId, userId, request, cancellationToken);
        return reordered
            ? NoContent()
            : NotFound(new { message = "Profile or one or more links were not found for the current user." });
    }

    [HttpGet("{slug}")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileResponse>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var profile = await profileService.GetBySlugAsync(slug, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    private static bool IsValidProfileData(string? slug, string? title, string? bio)
    {
        if (string.IsNullOrWhiteSpace(slug) || string.IsNullOrWhiteSpace(title))
            return false;

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var normalizedTitle = title.Trim();
        var normalizedBio = bio?.Trim() ?? string.Empty;

        return normalizedSlug.Length <= 50 &&
               SlugPattern.IsMatch(normalizedSlug) &&
               normalizedTitle.Length is > 0 and <= 100 &&
               normalizedBio.Length <= 500;
    }
}
