namespace LinkAnalytics.Application.DTOs.Links;

public sealed record ReorderLinksRequest(IReadOnlyList<int> LinkIds);
