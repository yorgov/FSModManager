namespace GameDetection
{
    [Flags]
    public enum InstallMethod
    {
        None = 0,
        Steam = 1 << 0,
        DirectDownload = 1 << 1
    }

    public enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }
}