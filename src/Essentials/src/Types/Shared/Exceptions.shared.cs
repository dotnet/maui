using System;

namespace Microsoft.Maui.ApplicationModel
{
	static class ExceptionUtils
	{
#if (NETSTANDARD || !PLATFORM) || NET6_0_OR_GREATER
		internal static NotImplementedInReferenceAssemblyException NotSupportedOrImplementedException =>
			new NotImplementedInReferenceAssemblyException();
#else
		internal static FeatureNotSupportedException NotSupportedOrImplementedException =>
			new FeatureNotSupportedException($"This API is not supported on {DeviceInfo.Platform}");
#endif

	}

	/// <include file="../../../docs/Microsoft.Maui.Essentials/NotImplementedInReferenceAssemblyException.xml" path="Type[@FullName='Microsoft.Maui.Essentials.NotImplementedInReferenceAssemblyException']/Docs" />
	class NotImplementedInReferenceAssemblyException : NotImplementedException
	{
		/// <include file="../../../docs/Microsoft.Maui.Essentials/NotImplementedInReferenceAssemblyException.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public NotImplementedInReferenceAssemblyException()
			: base("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.")
		{
		}
	}

	/// <include file="../../../docs/Microsoft.Maui.Essentials/PermissionException.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PermissionException']/Docs" />
	public class PermissionException : UnauthorizedAccessException
	{
		/// <include file="../../../docs/Microsoft.Maui.Essentials/PermissionException.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public PermissionException(string message)
			: base(message)
		{
		}
	}

	/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotSupportedException.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FeatureNotSupportedException']/Docs" />
	public class FeatureNotSupportedException : NotSupportedException
	{
		/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotSupportedException.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public FeatureNotSupportedException()
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotSupportedException.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public FeatureNotSupportedException(string message)
			: base(message)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotSupportedException.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public FeatureNotSupportedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotEnabledException.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FeatureNotEnabledException']/Docs" />
	public class FeatureNotEnabledException : InvalidOperationException
	{
		/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotEnabledException.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public FeatureNotEnabledException()
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotEnabledException.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public FeatureNotEnabledException(string message)
			: base(message)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Essentials/FeatureNotEnabledException.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public FeatureNotEnabledException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
