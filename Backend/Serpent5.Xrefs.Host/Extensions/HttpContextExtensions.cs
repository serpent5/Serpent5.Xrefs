using System.Security.Cryptography;

namespace Serpent5.Xrefs.Host.Extensions;

public static class HttpContextExtensions
{
    private const string CspNonceItemName = "Serpent5.CspNonce";

    public static string GetCspNonce(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string cspNonceValue;

        if (httpContext.Items.TryGetValue(CspNonceItemName, out var cspNonceValueAsObject))
            cspNonceValue = (string)cspNonceValueAsObject!;
        else
        {
            cspNonceValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            httpContext.Items[CspNonceItemName] = cspNonceValue;
        }

        return cspNonceValue;
    }
}
