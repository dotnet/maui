using System;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class Issue11740
	{
		const string IssueNumber = "11740";

		[Fact]
		public void DoNothingConverterLeavesTargetUnchanged()
		{
			if (!string.Equals(
				Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var entry = new Entry
			{
				BindingContext = new BindingSource()
			};

			entry.SetBinding(
				Entry.TextProperty,
				new Binding(nameof(BindingSource.Value), converter: new DoNothingConverter()));

			Assert.True(
				string.IsNullOrEmpty(entry.Text),
				"Binding.DoNothing should leave the target unchanged.");
		}

		sealed class BindingSource
		{
			public string Value { get; } = "Source value";
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
