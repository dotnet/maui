#nullable disable
using System;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.StyleSheets
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

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return vtProvider;
			if (serviceType == typeof(IConverterOptions))
				return convOptions;
After:
			{
				return vtProvider;
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return vtProvider;
			if (serviceType == typeof(IConverterOptions))
				return convOptions;
After:
			{
				return vtProvider;
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return vtProvider;
			if (serviceType == typeof(IConverterOptions))
				return convOptions;
After:
			{
				return vtProvider;
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return vtProvider;
			if (serviceType == typeof(IConverterOptions))
				return convOptions;
After:
			{
				return vtProvider;
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return vtProvider;
			if (serviceType == typeof(IConverterOptions))
				return convOptions;
After:
			{
				return vtProvider;
*/
			{
				return vtProvider;
			}

			if (serviceType == typeof(IConverterOptions))
			{
				return convOptions;
			}

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