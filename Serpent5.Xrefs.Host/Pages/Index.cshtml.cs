using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Serpent5.Xrefs.Host.Pages;

#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CA1034 // Nested types should not be visible

public class IndexModel : PageModel
{
    [BindProperty]
    public InputModel? Input { get; set; }

    public OutputModel? Output { get; set; }

    public async Task OnPost([FromServices] XrefClient xrefClient, CancellationToken cancellationToken)
    {
        if (Input is null || !ModelState.IsValid)
            return;

        IReadOnlyCollection<XrefSuggestion> xrefSuggestions;

        try
        {
            xrefSuggestions = await xrefClient.SuggestAsync(Input.Text, cancellationToken);
        }
        catch (XrefClientException)
        {
            // TODO: Handle Errors.
            xrefSuggestions = Array.Empty<XrefSuggestion>();
        }

        Output = new(
            xrefSuggestions
                .Select(x => x.Uid
                    .Replace("*", "%2A", StringComparison.OrdinalIgnoreCase)
                    .Replace("`", "%60", StringComparison.OrdinalIgnoreCase))
                .ToList());
    }

    public record InputModel([Required] string Text);

    public record OutputModel(IReadOnlyCollection<string> Suggestions);
}
#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore CA1034 // Nested types should not be visible
