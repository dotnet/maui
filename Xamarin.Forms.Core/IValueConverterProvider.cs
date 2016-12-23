using System;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	interface IValueConverterProvider
	{
		object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider);
	}
}