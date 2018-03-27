using System;

namespace Microsoft.Caboodle
{
    public class NotImplementedInReferenceAssemblyException : NotImplementedException
    {
        public NotImplementedInReferenceAssemblyException()
            : base("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.")
        {
        }
    }

    public class PermissionException : UnauthorizedAccessException
    {
        public PermissionException(string permission)
            : base($"API requires the {permission} permission to be set.")
        {
        }
    }

    public class FeatureNotSupportedException : NotSupportedException
    {
        public FeatureNotSupportedException()
        {
        }

        public FeatureNotSupportedException(string message)
            : base(message)
        {
        }

        public FeatureNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
