using System.Net;
using System.Net.Mime;
using Serpent5.Xrefs.Tests.Fakes.HttpClient;
using Xunit;

namespace Serpent5.Xrefs.Tests;

using static MediaTypeNames;

#pragma warning disable CA2000 // Dispose objects before losing scope
public class XrefClientTests
{
    private const string anyNonNullOrWhitespaceString = "A_STRING";
    private const string anyStringContent = anyNonNullOrWhitespaceString;
    private const string noResultsStringContent = "[]";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Throws_ArgumentException_When_Text_Is_Null_Whitespace_Or_Empty(string? invalidText)
    {
        using var fakeHttpMessageHandler = FakeHttpMessageHandler.CreateWithNoContentResponse();
        var xrefClient = new XrefClient(new HttpClient(fakeHttpMessageHandler));

        await Assert.ThrowsAsync<ArgumentException>(
            "searchTerm",
            async () => await xrefClient.SuggestAsync(invalidText!));
    }

    [Theory]
    [InlineData("A_STRING", "https://xref.docs.microsoft.com/autocomplete?text=A_STRING")]
    [InlineData("ANOTHER_STRING", "https://xref.docs.microsoft.com/autocomplete?text=ANOTHER_STRING")]
    [InlineData("A STRING WITH SPACES", "https://xref.docs.microsoft.com/autocomplete?text=A%20STRING%20WITH%20SPACES")]
#pragma warning disable CA1054 // URI-like parameters should not be strings
    public async Task Uses_HttpGet_And_Encoded_API_URI(string s, string expectedUri)
#pragma warning restore CA1054 // URI-like parameters should not be strings
    {
        var fakeHttpMessageHandler = FakeHttpMessageHandler.CreateWithStringContentResponse(
            noResultsStringContent, Application.Json);
        var xrefClient = new XrefClient(new HttpClient(fakeHttpMessageHandler));

        await xrefClient.SuggestAsync(s);

        Assert.Equal(HttpMethod.Get, fakeHttpMessageHandler.HttpRequestMessage?.Method);
        Assert.Equal(new Uri(expectedUri), fakeHttpMessageHandler.HttpRequestMessage?.RequestUri);
    }

    [Fact]
    public async Task Throws_XrefClientException_When_HttpClient_Throws_HttpRequestException()
    {
        var fakeHttpMessageHandler = FakeExceptionHttpMessageHandler.Create(new HttpRequestException());
        var xrefClient = new XrefClient(new HttpClient(fakeHttpMessageHandler));

        var xrefClientException = await Assert.ThrowsAsync<XrefClientException>(
            async () => await xrefClient.SuggestAsync(anyNonNullOrWhitespaceString));

        Assert.IsType<HttpRequestException>(xrefClientException.InnerException);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Throws_XrefClientException_When_Response_Status_Code_Is_Not_Success(HttpStatusCode httpStatusCode)
    {
        var fakeHttpMessageHandler = FakeHttpMessageHandler.CreateWithStatusCodeResponse(httpStatusCode);
        var xrefClient = new XrefClient(new HttpClient(fakeHttpMessageHandler));

        var xrefClientException = await Assert.ThrowsAsync<XrefClientException>(
            async () => await xrefClient.SuggestAsync(anyNonNullOrWhitespaceString));

        Assert.IsType<HttpRequestException>(xrefClientException.InnerException);
    }

    [Fact]
    public async Task Throws_XrefClientException_When_Response_Content_Type_Is_Not_Application_Json()
    {
        const string anyContentTypeNotApplicationJson = "text/plain";

        var fakeHttpMessageHandler = FakeHttpMessageHandler.CreateWithStringContentResponse(
            anyStringContent,
            anyContentTypeNotApplicationJson);
        var xrefClient = new XrefClient(new HttpClient(fakeHttpMessageHandler));

        await Assert.ThrowsAsync<XrefClientException>(
            async () => await xrefClient.SuggestAsync(anyNonNullOrWhitespaceString));
    }

    [Fact]
    public async Task Parses_No_Results_Json_As_Empty_Collection()
    {
        var fakeHttpMessageHandler = FakeHttpMessageHandler.CreateWithStringContentResponse(
            noResultsStringContent, Application.Json);
        var xrefClient = new XrefClient(new HttpClient(fakeHttpMessageHandler));

        Assert.Empty(await xrefClient.SuggestAsync(anyNonNullOrWhitespaceString));
    }

    [Fact]
    public async Task Parses_Single_Result_Json_As_Single_Item_Collection()
    {
        var fakeHttpMessageHandler = FakeHttpMessageHandler.CreateWithStringContentResponse(
            @"[{ ""uid"": ""A_STRING_UID"", ""score"": 0.01 }]", Application.Json);
        var xrefClient = new XrefClient(new HttpClient(fakeHttpMessageHandler));

        Assert.Equal(
            new XrefSuggestion[]
            {
                new("A_STRING_UID", 0.01M)
            },
            await xrefClient.SuggestAsync(anyNonNullOrWhitespaceString));
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scope
