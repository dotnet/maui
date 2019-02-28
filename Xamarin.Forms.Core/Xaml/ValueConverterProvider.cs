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
			var ret = value.ConvertTo(toType, minfoRetriever, serviceProvider, out Exception exception);
			if (exception != null)
				throw exception;
			return ret;
		}
	}
}
