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
		public void ValuePropertyCoercedToItselfIfSetToNull()
		{
			var radioButton = new RadioButton();
			
			Assert.Equal(radioButton, radioButton.Value);

			radioButton.Value = null;

			Assert.Equal(radioButton, radioButton.Value);
		}
	}
}
