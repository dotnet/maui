using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// https://github.com/dotnet/maui/issues/8874
	public class PickerSelectedIndexChangedLoopTests : BaseTestFixture
	{
		const int MaxEvents = 10;

		class Template : StackLayout
		{
			public Template() => Children.Add(new ContentPresenter());
		}

		class ViewModel
		{
			public IList<string> TerritoryNames { get; } = new List<string> { "India", "Germany" };
		}

		[Theory]
		[InlineData(BindingMode.OneTime)]
		[InlineData(BindingMode.OneWay)]
		public void ChangingControlTemplateFromSelectedIndexChangedDoesNotLoop(BindingMode mode)
		{
			var templates = new[] { new ControlTemplate(typeof(Template)), new ControlTemplate(typeof(Template)) };
			var page = new ContentPage();
			var picker = new Picker();
			picker.SetBinding(Picker.ItemsSourceProperty, nameof(ViewModel.TerritoryNames), mode);
			page.Content = picker;
			picker.SelectedIndex = 0;
			page.ControlTemplate = templates[0];

			int templateIndex = 0;
			int eventCount = 0;
			picker.SelectedIndexChanged += (s, e) =>
			{
				Assert.True(++eventCount <= MaxEvents, "SelectedIndexChanged is looping");
				int index = picker.SelectedIndex;
				if (index != templateIndex && index >= 0)
					page.ControlTemplate = templates[index];
				templateIndex = index;
			};
			page.BindingContext = new ViewModel();

			picker.SelectedIndex = 1;

			Assert.Equal(1, picker.SelectedIndex);
			Assert.Equal("Germany", picker.SelectedItem);
			Assert.Same(templates[1], page.ControlTemplate);
		}
	}
}
