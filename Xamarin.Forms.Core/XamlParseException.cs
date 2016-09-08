using System;
using System.Xml;

namespace Xamarin.Forms.Xaml
{
	public class XamlParseException : Exception
	{
		readonly string _unformattedMessage;

		public XamlParseException(string message, IXmlLineInfo xmlInfo, Exception innerException = null) : base(FormatMessage(message, xmlInfo), innerException)
		{
			_unformattedMessage = message;
			XmlInfo = xmlInfo;
		}

		public IXmlLineInfo XmlInfo { get; private set; }

		internal string UnformattedMessage
		{
			get { return _unformattedMessage ?? Message; }
		}

		static string FormatMessage(string message, IXmlLineInfo xmlinfo)
		{
			if (xmlinfo == null || !xmlinfo.HasLineInfo())
				return message;
			return string.Format("Position {0}:{1}. {2}", xmlinfo.LineNumber, xmlinfo.LinePosition, message);
		}
	}
}