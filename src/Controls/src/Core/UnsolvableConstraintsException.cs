#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/UnsolvableConstraintsException.xml" path="Type[@FullName='Microsoft.Maui.Controls.UnsolvableConstraintsException']/Docs/*" />
	[Serializable]
	public class UnsolvableConstraintsException : Exception
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/UnsolvableConstraintsException.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public UnsolvableConstraintsException()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/UnsolvableConstraintsException.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public UnsolvableConstraintsException(string message)
			: base(message)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/UnsolvableConstraintsException.xml" path="//Member[@MemberName='.ctor'][4]/Docs/*" />
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