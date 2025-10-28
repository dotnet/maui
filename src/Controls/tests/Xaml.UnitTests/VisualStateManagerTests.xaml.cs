using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class VisualStateManagerTests : ContentPage
	{
		public VisualStateManagerTests()
		{
			InitializeComponent();
		}

		public VisualStateManagerTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
			public void SetUp()
			{
				Application.Current = new MockApplication();
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
			public void TearDown()
			{
				Application.Current = null;
			}

			[InlineData(false)]
			[InlineData(true)]
			public void VisualStatesFromStyleXaml(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry0 = layout.Entry0;

				// Verify that Entry0 has no VisualStateGroups
				Assert.False(entry0.HasVisualStateGroups());
				Assert.Equal(entry0.TextColor, null);
				Assert.Equal(entry0.PlaceholderColor, null);

				var entry1 = layout.Entry1;

				// Verify that the correct groups are set up for Entry1
				var groups = VisualStateManager.GetVisualStateGroups(entry1);
				Assert.Equal(3, groups.Count);
				Assert.Equal("CommonStates", groups[0].Name);
				Assert.Contains("Normal", groups[0].States.Select(state => state.Name).ToList());
				Assert.Contains("Disabled", groups[0].States.Select(state => state.Name).ToList());

				Assert.Equal(null, entry1.TextColor);
				Assert.Equal(null, entry1.PlaceholderColor);

				// Change the state of Entry1
				Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

				// And verify that the changes took
				Assert.Equal(Colors.Gray, entry1.TextColor);
				Assert.Equal(Colors.LightGray, entry1.PlaceholderColor);

				// Verify that Entry0 was unaffected
				Assert.Equal(null, entry0.TextColor);
				Assert.Equal(null, entry0.PlaceholderColor);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void UnapplyVisualState(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);
				var entry1 = layout.Entry1;

				Assert.Equal(null, entry1.TextColor);
				Assert.Equal(null, entry1.PlaceholderColor);

				// Change the state of Entry1
				var groups = VisualStateManager.GetVisualStateGroups(entry1);
				Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

				// And verify that the changes took
				Assert.Equal(Colors.Gray, entry1.TextColor);
				Assert.Equal(Colors.LightGray, entry1.PlaceholderColor);

				// Now change it to Normal
				Assert.True(VisualStateManager.GoToState(entry1, "Normal"));

				// And verify that the changes reverted
				Assert.Equal(null, entry1.TextColor);
				Assert.Equal(null, entry1.PlaceholderColor);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void VisualStateGroupsDirectlyOnElement(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry = layout.Entry2;

				var groups = VisualStateManager.GetVisualStateGroups(entry);

				Assert.NotNull(groups);
				Assert.Equal(2, groups.Count);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void EmptyGroupDirectlyOnElement(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry3 = layout.Entry3;

				var groups = VisualStateManager.GetVisualStateGroups(entry3);

				Assert.NotNull(groups);
				Assert.True(groups.Count == 1);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void VisualStateGroupsFromStylesAreDistinct(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

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

			[InlineData(false)]
			[InlineData(true)]
			public void SettersAreAddedToCorrectState(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry = layout.Entry4;

				var groups = VisualStateManager.GetVisualStateGroups(entry);

				Assert.Equal(1, groups.Count);

				var common = groups[0];

				var normal = common.States.Single(state => state.Name == "Normal");
				var disabled = common.States.Single(state => state.Name == "Disabled");

				Assert.Equal(0, normal.Setters.Count);
				Assert.Equal(2, disabled.Setters.Count);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void VisualElementGoesToCorrectStateWhenAvailable(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var button = layout.Button1;

				Assert.Equal(Colors.Lime, button.BackgroundColor);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void TargetedVisualElementGoesToCorrectState(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var label1 = layout.TargetLabel1;

				VisualStateManager.GoToState(layout, "Red");

				Assert.Equal("Red", label1.Text);

				VisualStateManager.GoToState(layout, "Blue");

				Assert.Equal("Blue", label1.Text);

			}
		}
	}
}