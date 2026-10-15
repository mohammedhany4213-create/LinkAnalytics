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
    ILinkService linkService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType<LinkResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LinkResponse>> Create(
        CreateLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var link = await linkService.CreateAsync(
            request with { UserId = userId },
            cancellationToken);

        if (link is null)
            return BadRequest(new { message = "Link data is invalid or the profile does not belong to the current user." });

        return Created($"/api/links/{link.LinkId}/redirect", link);
    }

    [HttpGet("{linkId:int}/redirect")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectToDestination(
        int linkId,
        CancellationToken cancellationToken)
    {
        var countryCode = Request.Headers["CF-IPCountry"].ToString();

        var destination = await clickTrackingService.TrackAndGetDestinationAsync(
            linkId,
            countryCode,
            cancellationToken);

        return destination is null ? NotFound() : Redirect(destination);
    }
}
