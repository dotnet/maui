using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Maui.Platform
{
	class ToPlatformException : System.Exception
	{
		public ToPlatformException()
		{
		}

		public ToPlatformException(string message) : base(message)
		{
		}

		public ToPlatformException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ToPlatformException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
