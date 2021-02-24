using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class RadioButtonTests : BaseTestFixture
	{
		[Test]
		public void RadioButtonAddedToGroupGetsGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			layout.Children.Add(radioButton);

			Assert.That(radioButton.GroupName, Is.EqualTo(groupName));
		}

		[Test]
		public void NestedRadioButtonAddedToGroupGetsGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);


			var grid = new Grid();
			grid.Children.Add(radioButton);
			layout.Children.Add(grid);

			Assert.That(radioButton.GroupName, Is.EqualTo(groupName));
		}

		[Test]
		public void RadioButtonAddedToGroupKeepsGroupName()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var oldName = "bar";
			var radioButton = new RadioButton() { GroupName = oldName };

			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);
			layout.Children.Add(radioButton);

			Assert.That(radioButton.GroupName, Is.EqualTo(oldName));
		}

		[Test]
		public void LayoutGroupNameAppliesToExistingRadioButtons()
		{
			var layout = new StackLayout();
			var groupName = "foo";
			var radioButton = new RadioButton();

			layout.Children.Add(radioButton);
			layout.SetValue(RadioButtonGroup.GroupNameProperty, groupName);

			Assert.That(radioButton.GroupName, Is.EqualTo(groupName));
		}

		[Test]
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

			Assert.That(radioButton1.GroupName, Is.EqualTo(updatedGroupName));
			Assert.That(radioButton2.GroupName, Is.EqualTo("other"));
		}

		[Test]
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

			Assert.IsTrue(radioButton1.IsChecked);
			Assert.IsFalse(radioButton2.IsChecked);
			Assert.IsFalse(radioButton3.IsChecked);
			Assert.IsFalse(radioButton4.IsChecked);

			radioButton3.IsChecked = true;

			Assert.IsFalse(radioButton1.IsChecked);
			Assert.IsFalse(radioButton2.IsChecked);
			Assert.IsTrue(radioButton3.IsChecked);
			Assert.IsFalse(radioButton4.IsChecked);
		}

		[Test]
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

			Assert.IsTrue(radioButton1.IsChecked);
			Assert.IsFalse(radioButton2.IsChecked);
			Assert.IsFalse(radioButton3.IsChecked);

			radioButton3.IsChecked = true;

			Assert.IsFalse(radioButton1.IsChecked);
			Assert.IsFalse(radioButton2.IsChecked);
			Assert.IsTrue(radioButton3.IsChecked);
		}

		[Test]
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

			Assert.IsTrue(radioButton1.IsChecked);
			Assert.IsFalse(radioButton2.IsChecked);
			Assert.IsTrue(radioButton3.IsChecked);
		}

		[Test]
		public void RemovingSelectedButtonFromGroupClearsSelection()
		{
			var radioButton1 = new RadioButton() { GroupName = "foo" };
			var radioButton2 = new RadioButton() { GroupName = "foo" };
			var radioButton3 = new RadioButton() { GroupName = "foo" };

			radioButton1.IsChecked = true;
			radioButton2.IsChecked = true;

			Assert.IsFalse(radioButton1.IsChecked);
			Assert.IsTrue(radioButton2.IsChecked);
			Assert.IsFalse(radioButton3.IsChecked);

			radioButton2.GroupName = "bar";

			Assert.IsFalse(radioButton1.IsChecked);
			Assert.IsTrue(radioButton2.IsChecked);
			Assert.IsFalse(radioButton3.IsChecked);
		}

		[Test]
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

			Assert.AreEqual(selected, 1);

			Assert.AreEqual(radioButton1.GroupName, "foo");
			radioButton1.GroupName = "bar";

			selected = layout.GetValue(RadioButtonGroup.SelectedValueProperty);
			Assert.Null(selected);
		}

		[Test]
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

			Assert.AreEqual(1, layout.GetValue(RadioButtonGroup.SelectedValueProperty));

			radioButton1.Value = "updated";

			Assert.AreEqual("updated", layout.GetValue(RadioButtonGroup.SelectedValueProperty));
		}
	}
}
