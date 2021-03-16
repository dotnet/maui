using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	internal class CheckBoxCoreGalleryPage : CoreGalleryPage<CheckBox>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var isCheckedContainer = new ValueViewContainer<CheckBox>(Test.CheckBox.IsChecked, new CheckBox() { IsChecked = true, HorizontalOptions = LayoutOptions.Start }, "IsChecked", value => value.ToString());
			Add(isCheckedContainer);

			var checkedColorContainer = new ValueViewContainer<CheckBox>(Test.CheckBox.CheckedColor, new CheckBox() { IsChecked = true, Color = Color.Orange, HorizontalOptions = LayoutOptions.Start }, "Color", value => value.ToString());
			Add(checkedColorContainer);

			var groupList = new VisualStateGroupList();
			var group = new VisualStateGroup();
			var checkedVisualState = new VisualState
			{
				Name = "IsChecked"
			};
			checkedVisualState.Setters.Add(new Setter
			{
				Property = CheckBox.ColorProperty,
				Value = Color.Orange
			});

			group.States.Add(checkedVisualState);

			var normalVisualState = new VisualState
			{
				Name = "Normal"
			};
			normalVisualState.Setters.Add(new Setter
			{
				Property = CheckBox.ColorProperty,
				Value = Color.Red
			});
			group.States.Add(normalVisualState);
			groupList.Add(group);


			var checkBoxStateManaged = new CheckBox() { Color = Color.Red, HorizontalOptions = LayoutOptions.Start };
			VisualStateManager.SetVisualStateGroups(checkBoxStateManaged, groupList);


			var unCheckedColorContainer = new ValueViewContainer<CheckBox>(Test.CheckBox.UncheckedColor, checkBoxStateManaged, "Color", value => value.ToString());
			Add(unCheckedColorContainer);
		}
	}
}