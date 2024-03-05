using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// This dummy class is required to compile records when targeting .NET Standard
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit
    {
    }
}
