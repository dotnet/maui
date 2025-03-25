#nullable disable
using System;
using System.Reflection;

namespace Microsoft.Maui.Controls.Xaml
{
	// Used by MAUI XAML Hot Reload.
	// Consult XET if updating!
	interface IValueConverterProvider
	{
		object Convert(object value, Type toType, Func<MemberInfo> minfoRetriever, IServiceProvider serviceProvider);
	}
}