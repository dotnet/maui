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

			var entry = new Entry();
			entry.SetBinding(
				Entry.TextProperty,
				new Binding(nameof(BindingSource.SourceText), converter: new DoNothingValueConverter()));
			entry.BindingContext = new BindingSource();

			Assert.True(
				string.IsNullOrEmpty(entry.Text),
				"Binding.DoNothing should leave the target value unchanged.");
		}

		sealed class BindingSource
		{
			public string SourceText => "Source value";
		}

		sealed class DoNothingValueConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
				Binding.DoNothing;

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
				Binding.DoNothing;
		}
	}
}
