#nullable disable
using System;
using System.Reflection;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: Dependency(typeof(ValueConverterProvider))]
#pragma warning restore CS0618 // Type or member is obsolete
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
