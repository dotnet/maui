#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Exception thrown when layout constraints cannot be solved.</summary>
	[Serializable]
	public class UnsolvableConstraintsException : Exception
	{
		/// <summary>Creates a new <see cref="UnsolvableConstraintsException"/>.</summary>
		public UnsolvableConstraintsException()
		{
		}

		/// <summary>Creates a new <see cref="UnsolvableConstraintsException"/> with a message.</summary>
		/// <param name="message">The exception message.</param>
		public UnsolvableConstraintsException(string message)
			: base(message)
		{
		}

		/// <summary>Creates a new <see cref="UnsolvableConstraintsException"/> with a message and inner exception.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The inner exception.</param>
		public UnsolvableConstraintsException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if !NETSTANDARD
		[ObsoleteAttribute("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
		protected UnsolvableConstraintsException(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
	}
}