namespace PicHider.Core;

public enum StegoError
{
    InvalidMask,
    FileNotFound,
    ImageLoadFailed,
    CapacityExceeded,
    NoHiddenData,
    WrongPassword,
    OutputFailed,
}

public sealed class StegoException : Exception
{
    public StegoError Error { get; }

    public StegoException(StegoError error, string message, Exception? inner = null)
        : base(message, inner)
    {
        Error = error;
    }
}
