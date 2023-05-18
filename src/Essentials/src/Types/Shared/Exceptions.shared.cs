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

	/// <summary>
	/// Exception that occurs when executed from a reference assembly. This usually means that the NuGet was not installed into the app project.
	/// </summary>
	class NotImplementedInReferenceAssemblyException : NotImplementedException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NotImplementedInReferenceAssemblyException"/> class.
		/// </summary>
		public NotImplementedInReferenceAssemblyException()
			: base("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.")
		{
		}
	}

	/// <summary>
	/// Exception that occurs when calling an API that requires a specific permission.
	/// </summary>
	public class PermissionException : UnauthorizedAccessException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PermissionException"/> class with the specified message.
		/// </summary>
		/// <param name="message">A message that describes this exception in more detail.</param>
		public PermissionException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Exception that occurs when an attempt is made to use a feature on a platform that does not support it.
	/// </summary>
	public class FeatureNotSupportedException : NotSupportedException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FeatureNotSupportedException"/> class.
		/// </summary>
		public FeatureNotSupportedException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FeatureNotSupportedException"/> class with the specified message.
		/// </summary>
		/// <param name="message">A message that describes this exception in more detail.</param>
		public FeatureNotSupportedException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FeatureNotSupportedException"/> class with the specified message and inner exception.
		/// </summary>
		/// <param name="message">A message that describes this exception in more detail.</param>
		/// <param name="innerException">An inner exception that has relation to this exception.</param>
		public FeatureNotSupportedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	///  Exception that occurs when an attempt is made to use a feature on a platform that does not have this feature enabled.
	/// </summary>
	public class FeatureNotEnabledException : InvalidOperationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FeatureNotEnabledException"/> class.
		/// </summary>
		public FeatureNotEnabledException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FeatureNotEnabledException"/> class with the specified message.
		/// </summary>
		/// <param name="message">A message that describes this exception in more detail.</param>
		public FeatureNotEnabledException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FeatureNotEnabledException"/> class with the specified message and inner exception.
		/// </summary>
		/// <param name="message">A message that describes this exception in more detail.</param>
		/// <param name="innerException">An inner exception that has relation to this exception.</param>
		public FeatureNotEnabledException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
