namespace Serpent5.Xrefs;

public class XrefClientException : Exception
{
    public XrefClientException() { }

    public XrefClientException(string message)
        : base(message) { }

    public XrefClientException(string message, Exception innerException)
        : base(message, innerException) { }
}
