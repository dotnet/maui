using System;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty(nameof(Member))]
	[ProvideCompiled("Xamarin.Forms.Build.Tasks.StaticExtension")]
	public class StaticExtension : IMarkupExtension
	{
		public string Member { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			IXmlLineInfoProvider lineInfoProvider;
			IXmlLineInfo lineInfo;

			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			var typeResolver = serviceProvider.GetService(typeof (IXamlTypeResolver)) as IXamlTypeResolver;
			if (typeResolver == null)
				throw new ArgumentException("No IXamlTypeResolver in IServiceProvider");

			if (string.IsNullOrEmpty(Member) || !Member.Contains("."))
			{
				lineInfoProvider = serviceProvider.GetService(typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
				lineInfo = (lineInfoProvider != null) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
				throw new XamlParseException("Syntax for x:Static is [Member=][prefix:]typeName.staticMemberName", lineInfo);
			}

			var dotIdx = Member.LastIndexOf('.');
			var typename = Member.Substring(0, dotIdx);
			var membername = Member.Substring(dotIdx + 1);

			var type = typeResolver.Resolve(typename, serviceProvider);

			var pinfo = type.GetRuntimeProperties().FirstOrDefault(pi => pi.Name == membername && pi.GetMethod.IsStatic);
			if (pinfo != null)
				return pinfo.GetMethod.Invoke(null, new object[] { });

			var finfo = type.GetRuntimeFields().FirstOrDefault(fi => fi.Name == membername && fi.IsStatic);
			if (finfo != null)
				return finfo.GetValue(null);

			lineInfoProvider = serviceProvider.GetService(typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
			lineInfo = (lineInfoProvider != null) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
			throw new XamlParseException($"No static member found for {Member}", lineInfo);
		}
	}
}