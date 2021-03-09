using System;

namespace Microsoft.Maui.Controls
{
#if !NETSTANDARD1_0
	[Serializable]
#endif
	public class UnsolvableConstraintsException : Exception
	{
		public UnsolvableConstraintsException()
		{
		}

		public UnsolvableConstraintsException(string message)
			: base(message)
		{
		}

		public UnsolvableConstraintsException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if !NETSTANDARD1_0
		protected UnsolvableConstraintsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}