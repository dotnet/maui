#nullable disable
using System;
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>Exception that is raised when the XAML parser encounters a XAML error.</summary>
	[Serializable]
	public class XamlParseException : Exception
	{
		readonly string _unformattedMessage;
		readonly IXamlLineInfo _xamlInfo;
		IXmlLineInfo _xmlInfo;

		/// <summary>For internal use by the XAML engine.</summary>
		public XamlParseException()
		{
		}

		/// <summary>For internal use by the XAML engine.</summary>
		/// <param name="message">The exception message.</param>
		public XamlParseException(string message)
		   : base(message)
		{
		}

		/// <summary>For internal use by the XAML engine.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The inner exception.</param>
		public XamlParseException(string message, Exception innerException)
		   : base(message, innerException)
		{
		}

		/// <summary>For internal use by the XAML engine.</summary>
		/// <param name="info">Serialization info.</param>
		/// <param name="context">Streaming context.</param>
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

		internal XamlParseException(string message, IXamlLineInfo xamlInfo, Exception innerException = null)
			: base(FormatMessage(message, xamlInfo), innerException)
		{
			_unformattedMessage = message;
			_xamlInfo = xamlInfo;
		}

		/// <summary>For internal use by the XAML engine.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="xmlInfo">Line information for the error location.</param>
		/// <param name="innerException">The inner exception.</param>
		public XamlParseException(string message, IXmlLineInfo xmlInfo, Exception innerException = null)
			: this(message, xmlInfo == null ? null : new XmlToXamlLineInfoAdapter(xmlInfo), innerException)
		{
			_xmlInfo = xmlInfo;
		}

		/// <summary>Gets line information about the condition that caused the exception.</summary>
		public IXmlLineInfo XmlInfo => _xmlInfo ??= _xamlInfo == null ? null : new XamlToXmlLineInfoAdapter(_xamlInfo);

		/// <summary>Gets XAML line information about the condition that caused the exception.</summary>
		public IXamlLineInfo XamlInfo => _xamlInfo;

		internal string UnformattedMessage => _unformattedMessage ?? Message;

		static string FormatMessage(string message, IXamlLineInfo xamlInfo)
		{
			if (xamlInfo == null || !xamlInfo.HasLineInfo())
				return message;
			return string.Format("Position {0}:{1}. {2}", xamlInfo.LineNumber, xamlInfo.LinePosition, message);
		}

		static IXamlLineInfo GetLineInfo(IServiceProvider serviceProvider)
			=> (serviceProvider.GetService(typeof(IXamlLineInfo)) is IXamlLineInfo lineInfo) ? lineInfo : new XamlLineInfo();

		sealed class XmlToXamlLineInfoAdapter : IXamlLineInfo
		{
			readonly IXmlLineInfo _lineInfo;

			public XmlToXamlLineInfoAdapter(IXmlLineInfo lineInfo) => _lineInfo = lineInfo;

			public bool HasLineInfo() => _lineInfo.HasLineInfo();
			public int LineNumber => _lineInfo.LineNumber;
			public int LinePosition => _lineInfo.LinePosition;
		}

		sealed class XamlToXmlLineInfoAdapter : IXmlLineInfo
		{
			readonly IXamlLineInfo _lineInfo;

			public XamlToXmlLineInfoAdapter(IXamlLineInfo lineInfo) => _lineInfo = lineInfo;

			public bool HasLineInfo() => _lineInfo.HasLineInfo();
			public int LineNumber => _lineInfo.LineNumber;
			public int LinePosition => _lineInfo.LinePosition;
		}
	}
}
