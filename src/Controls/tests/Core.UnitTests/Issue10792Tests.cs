using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class Issue10792 : BaseTestFixture
	{
		[Fact]
		public void MultiBindingOnBindingContextEvaluatesInheritedSource()
		{
			var parent = new StackLayout
			{
				BindingContext = new TestViewModel()
			};
			var label = new Label();
			label.SetBinding(BindableObject.BindingContextProperty, new MultiBinding
			{
				Bindings = new Collection<BindingBase>
				{
					new Binding(nameof(TestViewModel.Property1), mode: BindingMode.OneTime),
					new Binding(nameof(TestViewModel.Property2), mode: BindingMode.OneTime)
				},
				Converter = new JoinConverter()
			});

			parent.Children.Add(label);

			Assert.True(
				Equals("First|Second", label.BindingContext),
				"MultiBinding on BindingContext should evaluate both operands.");
		}

		sealed class TestViewModel
		{
			public string Property1 => "First";

			public string Property2 => "Second";
		}

		sealed class JoinConverter : IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
				string.Join("|", values);

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
				throw new NotSupportedException();
		}
	}
}
