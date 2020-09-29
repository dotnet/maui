using System;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.StyleSheets
{
	class StyleSheetServiceProvider : IServiceProvider
	{
		IProvideValueTarget vtProvider;
		IConverterOptions convOptions => new ConverterOptions();

		public StyleSheetServiceProvider(object targetObject, object targetProperty)
		{
			vtProvider = new ValueTargetProvider
			{
				TargetObject = targetObject,
				TargetProperty = targetProperty
			};
		}

		public object GetService(Type serviceType)
		{
			if (serviceType == typeof(IProvideValueTarget))
				return vtProvider;
			if (serviceType == typeof(IConverterOptions))
				return convOptions;
			return null;
		}

		class ValueTargetProvider : IProvideValueTarget
		{
			public object TargetObject { get; set; }
			public object TargetProperty { get; set; }
		}

		class ConverterOptions : IConverterOptions
		{
			public bool IgnoreCase => true;
		}
	}
}