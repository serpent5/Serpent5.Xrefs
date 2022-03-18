using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Serpent5.Xrefs;

using static MediaTypeNames;

public class XrefClient
{
    private readonly HttpClient httpClient;
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public XrefClient(HttpClient httpClient)
        => this.httpClient = httpClient;

    public async Task<IReadOnlyCollection<XrefSuggestion>> SuggestAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Can't be null, empty, or whitespace", nameof(searchTerm));

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, GetAutocompleteUri(searchTerm));
        HttpResponseMessage? httpResponseMessage = null;

        try
        {
            httpResponseMessage = await httpClient.SendAsync(
                    httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            httpResponseMessage.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            httpResponseMessage?.Dispose();
            throw new XrefClientException(ex.Message, ex);
        }

        List<XrefSuggestion>? xrefSuggestions;

        using (httpResponseMessage)
        {
            var responseContentType = httpResponseMessage.Content?.Headers.ContentType?.MediaType;

            if (responseContentType != Application.Json)
                throw new XrefClientException($"Unexpected Content-Type \"{responseContentType}\".");

            var httpResponseMessageContent = await httpResponseMessage.Content!.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            // TODO: Wrap JsonException in XrefClientException.
            xrefSuggestions = await JsonSerializer.DeserializeAsync<List<XrefSuggestion>>(
                    httpResponseMessageContent, jsonSerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            // TODO: Ensure all expected properties were returned/mapped.
        }

        return xrefSuggestions!;
    }

    private static Uri GetAutocompleteUri(string searchTerm)
    {
        // TODO: This is overkill. We could probably get awat with an encode and "replace" for the dynamic value.
        var queryStringParameters = new Dictionary<string, string>
        {
            ["text"] = searchTerm
        };

        var queryString = string.Join(
            "&",
            queryStringParameters.Select(x => string.Join(
                "=",
                x.Key,
                UrlEncoder.Default.Encode(x.Value))));

        var uriBuilder = new UriBuilder("https://xref.docs.microsoft.com")
        {
            Path = "autocomplete",
            Query = queryString
        };

        return uriBuilder.Uri;
    }
}
