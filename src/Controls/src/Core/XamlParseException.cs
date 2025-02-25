#nullable disable
using System;
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XamlParseException.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.XamlParseException']/Docs/*" />
	[Serializable]
	public class XamlParseException : Exception
	{
		readonly string _unformattedMessage;

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XamlParseException.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public XamlParseException()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XamlParseException.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public XamlParseException(string message)
		   : base(message)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XamlParseException.xml" path="//Member[@MemberName='.ctor'][4]/Docs/*" />
		public XamlParseException(string message, Exception innerException)
		   : base(message, innerException)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XamlParseException.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
#if !NETSTANDARD
		[ObsoleteAttribute("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
		protected XamlParseException(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		internal XamlParseException(string message, IServiceProvider serviceProvider, Exception innerException = null)
			: this(message, GetLineInfo(serviceProvider), innerException)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XamlParseException.xml" path="//Member[@MemberName='.ctor'][5]/Docs/*" />
		public XamlParseException(string message, IXmlLineInfo xmlInfo, Exception innerException = null)
			: base(FormatMessage(message, xmlInfo), innerException)
		{
			_unformattedMessage = message;
			XmlInfo = xmlInfo;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XamlParseException.xml" path="//Member[@MemberName='XmlInfo']/Docs/*" />
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
