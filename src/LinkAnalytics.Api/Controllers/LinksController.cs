using LinkAnalytics.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LinkAnalytics.Api.Controllers;

[ApiController]
[Route("api/links")]
public sealed class LinksController(IClickTrackingService clickTrackingService) : ControllerBase
{
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
