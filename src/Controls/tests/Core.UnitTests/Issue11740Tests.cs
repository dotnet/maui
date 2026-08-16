using System;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class Issue11740
	{
		[Fact]
		public void DoNothingConverterLeavesTargetValueUnchanged()
		{
			var entry = new Entry
			{
				Text = string.Empty,
				BindingContext = new BindingSource(),
			};

			entry.SetBinding(
				Entry.TextProperty,
				new Binding(nameof(BindingSource.SourceText), converter: new DoNothingConverter()));

			Assert.True(entry.Text == string.Empty, "Binding.DoNothing must leave the target value unchanged.");
		}

		sealed class BindingSource
		{
			public string SourceText => "Source text";
		}

		sealed class DoNothingConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return Binding.DoNothing;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return Binding.DoNothing;
			}
		}
	}
}
