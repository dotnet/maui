using System;
using System.Xml;

namespace Xamarin.Forms.Xaml
{
	public class XamlParseException : Exception
	{
		readonly string _unformattedMessage;

		public XamlParseException(string message, IXmlLineInfo xmlInfo) : base(FormatMessage(message, xmlInfo))
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