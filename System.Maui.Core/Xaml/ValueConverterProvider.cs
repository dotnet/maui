using System;
using System.Reflection;

using System.Maui;
using System.Maui.Xaml;

[assembly:Dependency(typeof(ValueConverterProvider))]
namespace System.Maui.Xaml
{
	class ValueConverterProvider : IValueConverterProvider
	{
		public object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider)
		{
			var ret = value.ConvertTo(toType, minfoRetriever, serviceProvider, out Exception exception);
			if (exception != null)
				throw exception;
			return ret;
		}
	}
}
