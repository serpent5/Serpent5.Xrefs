using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Serpent5.Xrefs.Host.Extensions;

namespace Serpent5.Xrefs.Host.TagHelpers;

#pragma warning disable CA1062 // Validate arguments of public methods
[HtmlTargetElement("script")]
public class NonceTagHelper : TagHelper
{
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext ctx, TagHelperOutput tagHelperOutput)
        => tagHelperOutput.Attributes.Add("nonce", ViewContext.HttpContext.GetCspNonce());
}
#pragma warning restore CA1062 // Validate arguments of public methods
