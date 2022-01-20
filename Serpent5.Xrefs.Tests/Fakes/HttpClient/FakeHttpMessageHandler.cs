using System.Net;
using System.Text;

namespace Serpent5.Xrefs.Tests.Fakes.HttpClient;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage httpResponseMessage;

    private FakeHttpMessageHandler(HttpResponseMessage httpResponseMessage)
        => this.httpResponseMessage = httpResponseMessage;

    public HttpRequestMessage? HttpRequestMessage { get; private set; }

    public static FakeHttpMessageHandler CreateWithNoContentResponse()
        => CreateWithResponse(new HttpResponseMessage(HttpStatusCode.NoContent));

    public static FakeHttpMessageHandler CreateWithStatusCodeResponse(HttpStatusCode httpStatusCode)
        => CreateWithResponse(new HttpResponseMessage(httpStatusCode));

    public static FakeHttpMessageHandler CreateWithStringContentResponse(
        string stringContent, string contentType, Encoding? contentEncoding = null)
        => CreateWithResponse(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(stringContent, contentEncoding ?? Encoding.UTF8, contentType)
        });

    public static FakeHttpMessageHandler CreateWithResponse(HttpResponseMessage httpResponseMessage)
        => new(httpResponseMessage);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        HttpRequestMessage = httpRequestMessage;

        return Task.FromResult(httpResponseMessage);
    }
}
