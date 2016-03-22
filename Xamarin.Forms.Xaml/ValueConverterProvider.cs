using System;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	internal class ValueConverterProvider : IValueConverterProvider
	{
		public object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider)
		{
			return value.ConvertTo(toType, minfoRetriever, serviceProvider);
		}
	}
}