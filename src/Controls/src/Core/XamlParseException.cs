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

		/// <summary>For internal use by the XAML engine.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="xmlInfo">Line information for the error location.</param>
		/// <param name="innerException">The inner exception.</param>
		public XamlParseException(string message, IXmlLineInfo xmlInfo, Exception innerException = null)
			: base(FormatMessage(message, xmlInfo), innerException)
		{
			_unformattedMessage = message;
			XmlInfo = xmlInfo;
		}

		/// <summary>Gets line information about the condition that caused the exception.</summary>
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
