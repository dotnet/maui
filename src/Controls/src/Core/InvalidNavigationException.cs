#nullable disable
using System;
using System.Runtime.Serialization;

namespace Microsoft.Maui.Controls
{
	/// <summary>Exception thrown when an invalid navigation operation is attempted.</summary>
	[Serializable]
	public class InvalidNavigationException : Exception
	{
		/// <summary>Creates a new <see cref="InvalidNavigationException"/>.</summary>
		public InvalidNavigationException()
		{
		}

		/// <summary>Creates a new <see cref="InvalidNavigationException"/> with a message.</summary>
		/// <param name="message">The exception message.</param>
		public InvalidNavigationException(string message)
			: base(message)
		{
		}

		/// <summary>Creates a new <see cref="InvalidNavigationException"/> with a message and inner exception.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The inner exception.</param>
		public InvalidNavigationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if !NETSTANDARD
		[ObsoleteAttribute("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
		protected InvalidNavigationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}