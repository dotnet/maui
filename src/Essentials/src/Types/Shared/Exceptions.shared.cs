using System;

namespace Microsoft.Maui.Essentials
{
	static class ExceptionUtils
	{
#if NETSTANDARD || NET6_0
		internal static NotImplementedInReferenceAssemblyException NotSupportedOrImplementedException =>
			new NotImplementedInReferenceAssemblyException();
#else
		internal static FeatureNotSupportedException NotSupportedOrImplementedException =>
			new FeatureNotSupportedException($"This API is not supported on {DeviceInfo.Platform}");
#endif

	}

	public class NotImplementedInReferenceAssemblyException : NotImplementedException
	{
		public NotImplementedInReferenceAssemblyException()
			: base("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.")
		{
		}
	}

	public class PermissionException : UnauthorizedAccessException
	{
		public PermissionException(string message)
			: base(message)
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

	public class FeatureNotEnabledException : InvalidOperationException
	{
		public FeatureNotEnabledException()
		{
		}

		public FeatureNotEnabledException(string message)
			: base(message)
		{
		}

		public FeatureNotEnabledException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
