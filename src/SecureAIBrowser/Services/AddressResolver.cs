namespace SecureAIBrowser.Services;

public static class AddressResolver
{
    private const string SearchEndpoint = "https://www.bing.com/search?q=";

    public static Uri Resolve(string input)
    {
        var value = input.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Enter a web address or search term.", nameof(input));
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute) && IsWebUri(absolute))
        {
            return absolute;
        }

        if (!value.Contains(' ') && Uri.TryCreate($"https://{value}", UriKind.Absolute, out var hostOnly) &&
            !string.IsNullOrWhiteSpace(hostOnly.Host) && hostOnly.Host.Contains('.'))
        {
            return hostOnly;
        }

        return new Uri(SearchEndpoint + Uri.EscapeDataString(value));
    }

    public static bool IsWebUri(Uri uri) =>
        string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
}
