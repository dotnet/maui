using System;

namespace Xamarin.Forms
{
#if NETSTANDARD2_0
	[Serializable]
#endif
	public class InvalidNavigationException : Exception
	{
		public InvalidNavigationException()
		{
		}

		public InvalidNavigationException(string message)
			: base(message)
		{
		}

		public InvalidNavigationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETSTANDARD2_0
		protected InvalidNavigationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}