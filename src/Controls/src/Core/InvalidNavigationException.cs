#nullable disable
using System;
using System.Runtime.Serialization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="Type[@FullName='Microsoft.Maui.Controls.InvalidNavigationException']/Docs/*" />
	[Serializable]
	public class InvalidNavigationException : Exception
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public InvalidNavigationException()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public InvalidNavigationException(string message)
			: base(message)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="//Member[@MemberName='.ctor'][4]/Docs/*" />
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