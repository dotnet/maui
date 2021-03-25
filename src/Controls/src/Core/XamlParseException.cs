using System;
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
#if !NETSTANDARD1_0
	[Serializable]
#endif
	public class XamlParseException : Exception
	{
		readonly string _unformattedMessage;

		public XamlParseException()
		{
		}

		public XamlParseException(string message)
		   : base(message)
		{
		}

		public XamlParseException(string message, Exception innerException)
		   : base(message, innerException)
		{
		}

		protected XamlParseException(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		internal XamlParseException(string message, IServiceProvider serviceProvider, Exception innerException = null)
			: this(message, GetLineInfo(serviceProvider), innerException)
		{
		}

		public XamlParseException(string message, IXmlLineInfo xmlInfo, Exception innerException = null)
			: base(FormatMessage(message, xmlInfo), innerException)
		{
			_unformattedMessage = message;
			XmlInfo = xmlInfo;
		}

		public IXmlLineInfo XmlInfo { get; private set; }
		internal string UnformattedMessage => _unformattedMessage ?? Message;

		static string FormatMessage(string message, IXmlLineInfo xmlinfo)
		{
			if (xmlinfo == null || !xmlinfo.HasLineInfo())
				return message;
			return string.Format("Position {0}:{1}. {2}", xmlinfo.LineNumber, xmlinfo.LinePosition, message);
		}

		static IXmlLineInfo GetLineInfo(IServiceProvider serviceProvider)
			=> (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
	}
}
