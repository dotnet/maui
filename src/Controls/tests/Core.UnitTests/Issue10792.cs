using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class Issue10792
	{
		[Fact]
		public void MultiBindingEvaluatesWhenAppliedToBindingContext()
		{
			if (!string.Equals(
				Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
				"10792",
				StringComparison.Ordinal))
			{
				return;
			}

			var source = new BindingSource();
			var parent = new StackLayout
			{
				BindingContext = source
			};
			var label = new Label();
			label.SetBinding(BindableObject.BindingContextProperty, new MultiBinding
			{
				Bindings =
				{
					new Binding(nameof(BindingSource.Property1), mode: BindingMode.OneTime),
					new Binding(nameof(BindingSource.Property2), mode: BindingMode.OneTime)
				},
				Converter = new JoinValuesConverter(),
				FallbackValue = "FallbackValue"
			});

			parent.Children.Add(label);

			Assert.Equal("Alpha + Beta", label.BindingContext as string);
		}

		sealed class BindingSource
		{
			public string Property1 => "Alpha";

			public string Property2 => "Beta";
		}

		sealed class JoinValuesConverter : IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				if (values.Length != 2 || values[0] is not string first || values[1] is not string second)
					return BindableProperty.UnsetValue;

				return $"{first} + {second}";
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
				throw new NotSupportedException();
		}
	}
}
