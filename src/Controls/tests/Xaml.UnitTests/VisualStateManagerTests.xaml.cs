using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class VisualStateManagerTests : ContentPage
{
	public VisualStateManagerTests() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IClassFixture<ApplicationFixture>
	{
		public Tests(ApplicationFixture fixture) { }

		[Theory]
		[XamlInflatorData]
		internal void VisualStatesFromStyleXaml(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry0 = layout.Entry0;

			// Verify that Entry0 has no VisualStateGroups
			Assert.False(entry0.HasVisualStateGroups());
			Assert.Null(entry0.TextColor);
			Assert.Null(entry0.PlaceholderColor);

			var entry1 = layout.Entry1;

			// Verify that the correct groups are set up for Entry1
			var groups = VisualStateManager.GetVisualStateGroups(entry1);
			Assert.Equal(3, groups.Count);
			Assert.Equal("CommonStates", groups[0].Name);
			Assert.Contains("Normal", groups[0].States.Select(state => state.Name).ToList());
			Assert.Contains("Disabled", groups[0].States.Select(state => state.Name).ToList());

			Assert.Null(entry1.TextColor);
			Assert.Null(entry1.PlaceholderColor);

			// Change the state of Entry1
			Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

			// And verify that the changes took
			Assert.Equal(Colors.Gray, entry1.TextColor);
			Assert.Equal(Colors.LightGray, entry1.PlaceholderColor);

			// Verify that Entry0 was unaffected
			Assert.Null(entry0.TextColor);
			Assert.Null(entry0.PlaceholderColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void UnapplyVisualState(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);
			var entry1 = layout.Entry1;

			Assert.Null(entry1.TextColor);
			Assert.Null(entry1.PlaceholderColor);

			// Change the state of Entry1
			var groups = VisualStateManager.GetVisualStateGroups(entry1);
			Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

			// And verify that the changes took
			Assert.Equal(Colors.Gray, entry1.TextColor);
			Assert.Equal(Colors.LightGray, entry1.PlaceholderColor);

			// Now change it to Normal
			Assert.True(VisualStateManager.GoToState(entry1, "Normal"));

			// And verify that the changes reverted
			Assert.Null(entry1.TextColor);
			Assert.Null(entry1.PlaceholderColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void VisualStateGroupsDirectlyOnElement(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry = layout.Entry2;

			var groups = VisualStateManager.GetVisualStateGroups(entry);

			Assert.NotNull(groups);
			Assert.Equal(2, groups.Count);
		}

		[Theory]
		[XamlInflatorData]
		internal void EmptyGroupDirectlyOnElement(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry3 = layout.Entry3;

			var groups = VisualStateManager.GetVisualStateGroups(entry3);

			Assert.NotNull(groups);
			Assert.True(groups.Count == 1);
		}

		[Theory]
		[XamlInflatorData]
		internal void VisualStateGroupsFromStylesAreDistinct(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var label1 = layout.ErrorLabel1;
			var label2 = layout.ErrorLabel2;

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			var groups2 = VisualStateManager.GetVisualStateGroups(label2);

			Assert.NotSame(groups1, groups2);

			var currentState1 = groups1[0].CurrentState;
			var currentState2 = groups2[0].CurrentState;

			Assert.Equal("Normal", currentState1.Name);
			Assert.Equal("Normal", currentState2.Name);

			VisualStateManager.GoToState(label1, "Invalid");

			Assert.Equal("Invalid", groups1[0].CurrentState.Name);
			Assert.Equal("Normal", groups2[0].CurrentState.Name);
		}

		[Theory]
		[XamlInflatorData]
		internal void SettersAreAddedToCorrectState(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry = layout.Entry4;

			var groups = VisualStateManager.GetVisualStateGroups(entry);

			Assert.Single(groups);

			var common = groups[0];

			var normal = common.States.Single(state => state.Name == "Normal");
			var disabled = common.States.Single(state => state.Name == "Disabled");

			Assert.Empty(normal.Setters);
			Assert.Equal(2, disabled.Setters.Count);
		}

		[Theory]
		[XamlInflatorData]
		internal void VisualElementGoesToCorrectStateWhenAvailable(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var button = layout.Button1;

			Assert.Equal(Colors.Lime, button.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void TargetedVisualElementGoesToCorrectState(XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var label1 = layout.TargetLabel1;

			VisualStateManager.GoToState(layout, "Red");

			Assert.Equal("Red", label1.Text);

			VisualStateManager.GoToState(layout, "Blue");

			Assert.Equal("Blue", label1.Text);

		}
	}
}