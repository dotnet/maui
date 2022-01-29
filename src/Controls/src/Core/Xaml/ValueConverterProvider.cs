using System;
using System.Reflection;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

[assembly: Dependency(typeof(ValueConverterProvider))]
namespace Microsoft.Maui.Controls.Xaml
{
	class ValueConverterProvider : IValueConverterProvider
	{
		public object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider)
		{
			var ret = value.ConvertTo(toType, minfoRetriever, serviceProvider, out Exception exception);
			if (exception != null)
			{
				var lineInfo = (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
				throw new XamlParseException(exception.Message, serviceProvider, exception);
			}
			return ret;
		}
	}
}
