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

#if !NETSTANDARD
		[ObsoleteAttribute("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
		protected HandlerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
