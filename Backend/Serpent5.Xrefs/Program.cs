#pragma warning disable  // Avoid uninstantiated internal classes

using Serpent5.Xrefs;

using var httpClient = new HttpClient();
var xrefClient = new XrefClient(httpClient);

foreach (var x in await xrefClient.SuggestAsync("AddControllers"))
    Console.WriteLine("- {0}", x);

#pragma warning restore  // Avoid uninstantiated internal classes
