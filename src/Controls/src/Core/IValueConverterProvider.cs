using System;
using System.Reflection;

namespace Microsoft.Maui.Controls.Xaml
{
	interface IValueConverterProvider
	{
		object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider);
	}
}