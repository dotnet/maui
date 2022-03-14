using System;
using System.Runtime.Serialization;

namespace Microsoft.Maui.Controls
{
#if !NETSTANDARD1_0
	/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="Type[@FullName='Microsoft.Maui.Controls.InvalidNavigationException']/Docs" />
	[Serializable]
#endif
	public class InvalidNavigationException : Exception
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public InvalidNavigationException()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public InvalidNavigationException(string message)
			: base(message)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InvalidNavigationException.xml" path="//Member[@MemberName='.ctor'][4]/Docs" />
		public InvalidNavigationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected InvalidNavigationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}