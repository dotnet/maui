using System;
using System.Reflection;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly:Dependency(typeof(ValueConverterProvider))]
namespace Xamarin.Forms.Xaml
{
	class ValueConverterProvider : IValueConverterProvider
	{
		public object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider)
		{
			return value.ConvertTo(toType, minfoRetriever, serviceProvider);
		}
	}
}
