using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class Issue10792 : BaseTestFixture
	{
		const string IssueNumber = "10792";

		[Fact]
		public void MultiBindingOnBindingContextEvaluatesInheritedSourceProperties()
		{
			if (!string.Equals(
				Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var parent = new ContentView
			{
				BindingContext = new BindingValues()
			};
			var label = new Label();
			label.SetBinding(Label.BindingContextProperty, new MultiBinding
			{
				Bindings = new Collection<BindingBase>
				{
					new Binding(nameof(BindingValues.Property1), mode: BindingMode.OneTime),
					new Binding(nameof(BindingValues.Property2), mode: BindingMode.OneTime),
				},
				Converter = new ConcatenationConverter(),
				FallbackValue = "FallbackValue",
				TargetNullValue = "TargetNullValue",
			});

			parent.Content = label;

			Assert.True(
				string.Equals("First value + Second value", label.BindingContext as string, StringComparison.Ordinal),
				"BindingContext MultiBinding should evaluate both source properties.");
		}

		sealed class BindingValues
		{
			public string Property1 => "First value";

			public string Property2 => "Second value";
		}

		sealed class ConcatenationConverter : IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				if (values.Length == 2 &&
					values[0] is string first &&
					values[1] is string second)
				{
					return $"{first} + {second}";
				}

				return BindableProperty.UnsetValue;
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
			{
				throw new NotSupportedException();
			}
		}
	}
}
