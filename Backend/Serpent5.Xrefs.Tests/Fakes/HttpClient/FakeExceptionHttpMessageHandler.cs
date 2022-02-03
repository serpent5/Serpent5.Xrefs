namespace Serpent5.Xrefs.Tests.Fakes.HttpClient;

public class FakeExceptionHttpMessageHandler : HttpMessageHandler
{
    private readonly Exception ex;

    private FakeExceptionHttpMessageHandler(Exception ex)
        => this.ex = ex;

    public static FakeExceptionHttpMessageHandler Create(Exception ex)
        => new(ex);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => throw ex;
}
