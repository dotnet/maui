using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Maui.Platform
{
	class HandlerNotFoundException : System.Exception
	{
		public HandlerNotFoundException()
		{
		}

		public HandlerNotFoundException(IElement element) :
			this($"Handler not found for view {element}.")
		{

		}

		public HandlerNotFoundException(string message) : base(message)
		{
		}

		public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HandlerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
