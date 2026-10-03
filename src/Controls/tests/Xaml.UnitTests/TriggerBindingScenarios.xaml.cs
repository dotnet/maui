using Maui.Controls.Sample;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TriggerBindingScenarios : ContentPage
{
	public TriggerBindingScenarios() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void DataTriggerUpdatesButtonAndRestoresEmptyState(XamlInflator inflator)
		{
			var model = new TriggersViewModel();
			var page = new TriggerBindingScenarios(inflator) { BindingContext = model };

			Assert.Equal("Save", page.SaveButton.Text);
			Assert.False(page.SaveButton.IsEnabled);
			Assert.Equal(0.5, page.SaveButton.Opacity);

			page.DataEntry.Text = "Test";
			Assert.Equal("Test", model.DataEntryText);
			Assert.True(page.SaveButton.IsEnabled);
			Assert.Equal(1.0, page.SaveButton.Opacity);

			page.DataEntry.Text = string.Empty;
			Assert.Equal(string.Empty, model.DataEntryText);
			Assert.False(page.SaveButton.IsEnabled);
			Assert.Equal(0.5, page.SaveButton.Opacity);
		}

		[Theory]
		[XamlInflatorData]
		internal void EventTriggerValidatesInputAndCanRecover(XamlInflator inflator)
		{
			var page = new TriggerBindingScenarios(inflator);

			page.NumericEntry.Text = "123";
			Assert.Equal(Colors.Black, page.NumericEntry.TextColor);
			Assert.Equal(Colors.SkyBlue, page.NumericEntry.BackgroundColor);

			page.NumericEntry.Text = "abc";
			Assert.Equal(Colors.Red, page.NumericEntry.TextColor);
			Assert.Equal(Colors.Yellow, page.NumericEntry.BackgroundColor);

			page.NumericEntry.Text = string.Empty;
			Assert.Equal(Colors.Red, page.NumericEntry.TextColor);
			Assert.Equal(Colors.Yellow, page.NumericEntry.BackgroundColor);

			page.NumericEntry.Text = "456";
			Assert.Equal(Colors.Black, page.NumericEntry.TextColor);
			Assert.Equal(Colors.SkyBlue, page.NumericEntry.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void MultiTriggerRequiresBothFieldsAndRestoresSetters(XamlInflator inflator)
		{
			var model = new TriggersViewModel();
			var page = new TriggerBindingScenarios(inflator) { BindingContext = model };

			Assert.Equal("Submit", page.SubmitButton.Text);
			foreach (var (email, phone, enabled) in new[]
			{
				("", "", false),
				("user@test.com", "", false),
				("", "555-1234", false),
				("user@test.com", "555-1234", true),
				("", "555-1234", false),
				("user@test.com", "555-1234", true),
				("user@test.com", "", false),
			})
			{
				page.EmailEntry.Text = email;
				page.PhoneEntry.Text = phone;

				Assert.Equal(email, model.EmailEntryText);
				Assert.Equal(phone, model.PhoneEntryText);
				Assert.Equal(enabled, page.SubmitButton.IsEnabled);
				Assert.Equal(enabled ? 1.0 : 0.5, page.SubmitButton.Opacity);
			}
		}

		[Theory]
		[XamlInflatorData]
		internal void StateTriggerFollowsTwoWaySwitchBinding(XamlInflator inflator)
		{
			var model = new TriggersViewModel();
			var page = new TriggerBindingScenarios(inflator) { BindingContext = model };
			var group = Assert.Single(VisualStateManager.GetVisualStateGroups(page.StateGrid));

			Assert.Equal("Unchecked", group.CurrentState.Name);
			Assert.Equal(Colors.White, page.StateGrid.BackgroundColor);

			page.StateSwitch.IsToggled = true;
			Assert.True(model.IsToggled);
			Assert.Equal("Checked", group.CurrentState.Name);
			Assert.Equal(Colors.Black, page.StateGrid.BackgroundColor);

			page.StateSwitch.IsToggled = false;
			Assert.False(model.IsToggled);
			Assert.Equal("Unchecked", group.CurrentState.Name);
			Assert.Equal(Colors.White, page.StateGrid.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void CompareStateTriggerFollowsTwoWayCheckBoxBinding(XamlInflator inflator)
		{
			var model = new TriggersViewModel();
			var page = new TriggerBindingScenarios(inflator) { BindingContext = model };
			var group = Assert.Single(VisualStateManager.GetVisualStateGroups(page.CompareGrid));

			Assert.Equal("UncheckedState", group.CurrentState.Name);
			Assert.Equal(Colors.LightGray, page.CompareGrid.BackgroundColor);

			page.CompareCheckBox.IsChecked = true;
			Assert.True(model.IsChecked);
			Assert.Equal("CheckedState", group.CurrentState.Name);
			Assert.Equal(Colors.DarkGreen, page.CompareGrid.BackgroundColor);

			page.CompareCheckBox.IsChecked = false;
			Assert.False(model.IsChecked);
			Assert.Equal("UncheckedState", group.CurrentState.Name);
			Assert.Equal(Colors.LightGray, page.CompareGrid.BackgroundColor);
		}
	}
}
