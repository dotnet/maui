using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using Grid = Microsoft.Maui.Controls.Compatibility.Grid;
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	public class RadioButtonTests : BaseTestFixture
	{
		[Fact]
		public void RadioButtonAddedToGroupGetsGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			layout.Children.Add(radioButton);

			Assert.Equal(radioButton.GroupName, groupName);
		}

		[Fact]
		public void NestedRadioButtonAddedToGroupGetsGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);


			var grid = new Grid();
			grid.Children.Add(radioButton);
			layout.Children.Add(grid);

			Assert.Equal(radioButton.GroupName, groupName);
		}

		[Fact]
		public void RadioButtonAddedToGroupKeepsGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var oldName = "bar";
			var radioButton = new RadioButton() { GroupName = oldName, Value = 1 };

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			layout.Children.Add(radioButton);

			Assert.Equal(radioButton.GroupName, oldName);
		}

		[Fact]
		public void LayoutGroupNameAppliesToExistingRadioButtons()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.Children.Add(radioButton);
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);

			Assert.Equal(radioButton.GroupName, groupName);
		}

		[Fact]
		public void UpdatedGroupNameAppliesToRadioButtonsWithOldGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var updatedGroupName = "bar";
			var otherGroupName = "other";
			var radioButton1 = new RadioButton();
			var radioButton2 = new RadioButton() { GroupName = otherGroupName };

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);

			layout.SetValue(RadioButtonGroup.GroupNameProperty, updatedGroupName);

			Assert.Equal(radioButton1.GroupName, updatedGroupName);
			Assert.Equal("other", radioButton2.GroupName);
		}

		[Fact]
		public void ThereCanBeOnlyOne()
		{
			var groupName = "foo";

			var radioButton1 = new RadioButton() { GroupName = groupName };
			var radioButton2 = new RadioButton() { GroupName = groupName };
			var radioButton3 = new RadioButton() { GroupName = groupName };
			var radioButton4 = new RadioButton() { GroupName = groupName };

			var layout = new Grid();

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.Children.Add(radioButton3);
			layout.Children.Add(radioButton4);

			radioButton1.IsChecked = true;

			Assert.True(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.False(radioButton3.IsChecked);
			Assert.False(radioButton4.IsChecked);

			radioButton3.IsChecked = true;

			Assert.False(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.True(radioButton3.IsChecked);
			Assert.False(radioButton4.IsChecked);
		}

		[Fact]
		public void ImpliedGroup()
		{
			var radioButton1 = new RadioButton();
			var radioButton2 = new RadioButton();
			var radioButton3 = new RadioButton();

			var layout = new Grid();

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.Children.Add(radioButton3);

			radioButton1.IsChecked = true;

			Assert.True(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.False(radioButton3.IsChecked);

			radioButton3.IsChecked = true;

			Assert.False(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.True(radioButton3.IsChecked);
		}

		[Fact]
		public void ImpliedGroupDoesNotIncludeExplicitGroups()
		{
			var radioButton1 = new RadioButton();
			var radioButton2 = new RadioButton();
			var radioButton3 = new RadioButton() { GroupName = "foo" };

			var layout = new Grid();

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.Children.Add(radioButton3);

			radioButton1.IsChecked = true;
			radioButton3.IsChecked = true;

			Assert.True(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.True(radioButton3.IsChecked);
		}

		[Fact]
		public void RemovingSelectedButtonFromGroupClearsSelection()
		{
			var radioButton1 = new RadioButton() { GroupName = "foo" };
			var radioButton2 = new RadioButton() { GroupName = "foo" };
			var radioButton3 = new RadioButton() { GroupName = "foo" };

			var layout = new Grid();
			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.Children.Add(radioButton3);

			radioButton1.IsChecked = true;
			radioButton2.IsChecked = true;

			Assert.False(radioButton1.IsChecked);
			Assert.True(radioButton2.IsChecked);
			Assert.False(radioButton3.IsChecked);

			radioButton2.GroupName = "bar";

			Assert.False(radioButton1.IsChecked);
			Assert.True(radioButton2.IsChecked);
			Assert.False(radioButton3.IsChecked);
		}

		[Fact]
		public void GroupControllerSelectionIsNullWhenSelectedButtonRemoved()
		{
			var layout = new Grid();
			layout.SetValue(RadioButtonGroup.GroupNameProperty, "foo");
			var selected = layout.GetValue(RadioButtonGroup.SelectedValueProperty);

			Assert.Null(selected);

			var radioButton1 = new RadioButton() { Value = 1 };
			var radioButton2 = new RadioButton() { Value = 2 };
			var radioButton3 = new RadioButton() { Value = 3 };

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.Children.Add(radioButton3);

			Assert.Null(selected);

			radioButton1.IsChecked = true;

			selected = layout.GetValue(RadioButtonGroup.SelectedValueProperty);

			Assert.Equal(1, selected);

			Assert.Equal("foo", radioButton1.GroupName);
			radioButton1.GroupName = "bar";

			selected = layout.GetValue(RadioButtonGroup.SelectedValueProperty);
			Assert.Null(selected);
		}

		[Fact]
		public void GroupSelectedValueUpdatesWhenSelectedButtonValueUpdates()
		{
			var layout = new Grid();
			layout.SetValue(RadioButtonGroup.GroupNameProperty, "foo");

			var radioButton1 = new RadioButton() { Value = 1, IsChecked = true };
			var radioButton2 = new RadioButton() { Value = 2 };
			var radioButton3 = new RadioButton() { Value = 3 };

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.Children.Add(radioButton3);

			Assert.Equal(1, layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			radioButton1.Value = "updated";

			Assert.Equal("updated", layout.GetValue(RadioButtonGroup.SelectedValueProperty));
		}

		[Fact]
		public async Task RadioButtonGroupLayoutShouldNotLeak()
		{
			WeakReference CreateReference()
			{
				var layout = new StackLayout();
				var rb1 = new RadioButton { Content = "RB1", Value = "RB1" };
				var rb2 = new RadioButton { Content = "RB2", Value = "RB2" };
				layout.Add(rb1);
				layout.Add(rb2);
				layout.SetValue(RadioButtonGroup.GroupNameProperty, "GroupA");
				layout.SetValue(RadioButtonGroup.SelectedValueProperty, "RB1");

				return new(layout);
			}

			WeakReference reference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(reference.IsAlive, "RadioButtonGroup should not be alive");
		}

		[Fact]
		public async Task RadioButtonShouldNotLeak()
		{
			WeakReference CreateReference()
			{
				var radioButton = new RadioButton { Content = "RB1", Value = "RB1", GroupName = "GroupA" };
				return new(radioButton);
			}
			WeakReference reference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(reference.IsAlive, "RadioButton should not be alive");

		}

		[Fact]
		public void GroupNullSelectionClearsAnySelection()
		{
			var layout = new Grid();
			layout.SetValue(RadioButtonGroup.GroupNameProperty, "foo");

			var radioButton1 = new RadioButton() { Value = 1, IsChecked = true };
			var radioButton2 = new RadioButton() { Value = 2 };
			var radioButton3 = new RadioButton() { Value = 3 };

			layout.Children.Add(radioButton1);
			layout.Children.Add(radioButton2);
			layout.Children.Add(radioButton3);

			Assert.Equal(1, layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			layout.SetValue(RadioButtonGroup.SelectedValueProperty, null);

			Assert.False(radioButton1.IsChecked);
		}

		[Fact]
		public void ValuePropertyCanBeSetToNull()
		{
			var radioButton = new RadioButton();

			Assert.Null(radioButton.Value);

			radioButton.Value = 1;

			Assert.Equal(1, radioButton.Value);

			radioButton.Value = null;

			Assert.Null(radioButton.Value);
		}

		[Fact]
		public void RadioButtonGroupWorksWithDynamicallyAddedDescendants()
		{
			// Simulates CollectionView scenario where RadioButtons are added as descendants
			// rather than direct children (they're inside ItemTemplate)
			var layout = new StackLayout();
			var groupName = "choices";

			// Set up RadioButtonGroup on parent layout
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			layout.SetValue(RadioButtonGroup.SelectedValueProperty, null);

			// Create a container that simulates CollectionView item container
			var itemContainer = new StackLayout();
			layout.Children.Add(itemContainer);

			// Create RadioButtons and add them to the nested container
			// This triggers DescendantAdded events (like CollectionView does)
			var radioButton1 = new RadioButton() { Value = "Choice 1" };
			var radioButton2 = new RadioButton() { Value = "Choice 2" };
			var radioButton3 = new RadioButton() { Value = "Choice 3" };

			itemContainer.Children.Add(radioButton1);
			itemContainer.Children.Add(radioButton2);
			itemContainer.Children.Add(radioButton3);

			// Verify RadioButtons received the group name from ancestor
			Assert.Equal(groupName, radioButton1.GroupName);
			Assert.Equal(groupName, radioButton2.GroupName);
			Assert.Equal(groupName, radioButton3.GroupName);

			// Verify SelectedValue is initially null
			Assert.Null(layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			// Check a RadioButton
			radioButton2.IsChecked = true;

			// Verify SelectedValue binding is updated
			Assert.Equal("Choice 2", layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			// Check another RadioButton
			radioButton3.IsChecked = true;

			// Verify SelectedValue binding updates again
			Assert.Equal("Choice 3", layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			// Verify only one RadioButton is checked
			Assert.False(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.True(radioButton3.IsChecked);
		}

		[Fact]
		public void RadioButtonGroupSelectedValueBindingWorksWithNestedDescendants()
		{
			// Tests that setting SelectedValue on the group selects the correct descendant RadioButton
			var layout = new StackLayout();
			var groupName = "choices";

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);

			// Nested container simulating CollectionView
			var itemContainer = new StackLayout();
			layout.Children.Add(itemContainer);

			var radioButton1 = new RadioButton() { Value = "Choice 1" };
			var radioButton2 = new RadioButton() { Value = "Choice 2" };
			var radioButton3 = new RadioButton() { Value = "Choice 3" };

			itemContainer.Children.Add(radioButton1);
			itemContainer.Children.Add(radioButton2);
			itemContainer.Children.Add(radioButton3);

			// Set SelectedValue from the group (simulates binding update)
			layout.SetValue(RadioButtonGroup.SelectedValueProperty, "Choice 2");

			// Verify the correct RadioButton is checked
			Assert.False(radioButton1.IsChecked);
			Assert.True(radioButton2.IsChecked);
			Assert.False(radioButton3.IsChecked);

			// Change SelectedValue
			layout.SetValue(RadioButtonGroup.SelectedValueProperty, "Choice 3");

			// Verify selection updates
			Assert.False(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.True(radioButton3.IsChecked);
		}

		[Fact]
		public void RadioButtonGroupWorksWithContentViewControlTemplate()
		{
			// ContentView with ControlTemplate containing RadioButton
			// The ControlTemplate is applied inline (before ContentView is added to parent layout)
			var groupName = "Test1";
			var layout = new VerticalStackLayout();
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);

			// Create ContentView with inline ControlTemplate (RadioButton inside Border)
			// This mimics how XAML inline ControlTemplate works - template is applied
			// before ContentView is added to the parent layout
			var radioButton1 = new RadioButton { Content = "Option 1", Value = "opt1", GroupName = groupName };
			var radioButton2 = new RadioButton { Content = "Option 2", Value = "opt2", GroupName = groupName };

			var border1 = new Border { Content = radioButton1 };
			var border2 = new Border { Content = radioButton2 };

			var contentView1 = new ContentView();
			var contentView2 = new ContentView();

			// Apply ControlTemplate by simulating: template root added as logical child BEFORE parent is set
			((IControlTemplated)contentView1).AddLogicalChild(border1);
			((IControlTemplated)contentView2).AddLogicalChild(border2);

			// Now add ContentViews to layout (parent set AFTER template already applied)
			layout.Add(contentView1);
			layout.Add(contentView2);

			// Initially, neither button should be checked (no IsChecked="True" was set)
			Assert.False(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.Null(layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			// Check radio button 1 - only rb1 should be checked and SelectedValue updated
			radioButton1.IsChecked = true;
			Assert.True(radioButton1.IsChecked);
			Assert.False(radioButton2.IsChecked);
			Assert.Equal("opt1", layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			// Check radio button 2 - radio button 1 should be unchecked and SelectedValue updated
			radioButton2.IsChecked = true;
			Assert.False(radioButton1.IsChecked);
			Assert.True(radioButton2.IsChecked);
			Assert.Equal("opt2", layout.GetValue(RadioButtonGroup.SelectedValueProperty));
		}

		[Fact]
		public void RadioButtonGroupAutoChecksMatchingButtonInContentViewWhenSelectedValuePreset()
		{
			// Verifies the positive auto-check path for the ContentView/ControlTemplate scenario
			// when SelectedValue IS explicitly set on the group, a RadioButton
			// added through a ContentView ControlTemplate with a matching Value must be auto-checked.
			var groupName = "Test2";
			var layout = new VerticalStackLayout();
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);

			// Pre-set SelectedValue BEFORE adding buttons (simulates binding from ViewModel)
			layout.SetValue(RadioButtonGroup.SelectedValueProperty, "opt2");

			var radioButton1 = new RadioButton { Content = "Option 1", Value = "opt1", GroupName = groupName };
			var radioButton2 = new RadioButton { Content = "Option 2", Value = "opt2", GroupName = groupName };

			var border1 = new Border { Content = radioButton1 };
			var border2 = new Border { Content = radioButton2 };

			var contentView1 = new ContentView();
			var contentView2 = new ContentView();

			// Apply ControlTemplate before adding to layout
			((IControlTemplated)contentView1).AddLogicalChild(border1);
			((IControlTemplated)contentView2).AddLogicalChild(border2);

			layout.Add(contentView1);
			layout.Add(contentView2);

			// RadioButton whose Value matches the pre-set SelectedValue must be auto-checked
			Assert.False(radioButton1.IsChecked);
			Assert.True(radioButton2.IsChecked);
			Assert.Equal("opt2", layout.GetValue(RadioButtonGroup.SelectedValueProperty));
		}
	}
}
