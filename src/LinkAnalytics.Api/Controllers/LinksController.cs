using System.Security.Claims;
using LinkAnalytics.Application.DTOs.Links;
using LinkAnalytics.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkAnalytics.Api.Controllers;

[ApiController]
[Route("api/links")]
public sealed class LinksController(
    IClickTrackingService clickTrackingService,
    ILinkService linkService,
    ILinkAnalyticsService linkAnalyticsService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType<LinkResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinkResponse>> Create(
        CreateLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        if (!IsValidLinkData(request.Title, request.Url))
            return BadRequest(new { message = "Link title or URL is invalid. Only absolute HTTP and HTTPS URLs are supported." });

        var link = await linkService.CreateAsync(request with { UserId = userId }, cancellationToken);
        if (link is null)
            return NotFound(new { message = "The target profile was not found or does not belong to the current user." });

        return Created($"/api/links/{link.LinkId}/redirect", link);
    }

    [HttpPut("{linkId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int linkId,
        UpdateLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        if (linkId <= 0 || !IsValidLinkData(request.Title, request.Url))
            return BadRequest(new { message = "Link title or URL is invalid. Only absolute HTTP and HTTPS URLs are supported." });

        var updated = await linkService.UpdateAsync(linkId, userId, request, cancellationToken);
        return updated
            ? NoContent()
            : NotFound(new { message = "Link was not found or does not belong to the current user." });
    }

    [HttpDelete("{linkId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int linkId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var deleted = await linkService.DeleteAsync(linkId, userId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{linkId:int}/analytics")]
    [Authorize]
    [ProducesResponseType<LinkAnalyticsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinkAnalyticsResponse>> GetAnalytics(
        int linkId,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var analytics = await linkAnalyticsService.GetLinkAnalyticsAsync(linkId, userId, cancellationToken);
        return analytics is null ? NotFound() : Ok(analytics);
    }

    [HttpGet("{linkId:int}/redirect")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectToDestination(
        int linkId,
        CancellationToken cancellationToken)
    {
        var countryCode = Request.Headers["CF-IPCountry"].ToString();
        var destination = await clickTrackingService.TrackAndGetDestinationAsync(linkId, countryCode, cancellationToken);
        return destination is null ? NotFound() : Redirect(destination);
    }

    private static bool IsValidLinkData(string? title, string? url)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 100 ||
            string.IsNullOrWhiteSpace(url) || url.Length > 2048 ||
            !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}
