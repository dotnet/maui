#nullable disable
using System.Collections;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides attached properties for managing groups of <see cref="RadioButton"/> controls.
	/// </summary>
	public static class RadioButtonGroup
	{
		static readonly BindableProperty RadioButtonGroupControllerProperty =
			BindableProperty.CreateAttached("RadioButtonGroupController", typeof(RadioButtonGroupController), typeof(Maui.ILayout), default(RadioButtonGroupController),
			defaultValueCreator: (b) => new RadioButtonGroupController(b as Maui.ILayout),
			propertyChanged: (b, o, n) => OnControllerChanged(b, (RadioButtonGroupController)o, (RadioButtonGroupController)n));

		static RadioButtonGroupController GetRadioButtonGroupController(BindableObject b)
		{
			return (RadioButtonGroupController)b.GetValue(RadioButtonGroupControllerProperty);
		}

		/// <summary>Bindable property for attached property <c>GroupName</c>.</summary>
		public static readonly BindableProperty GroupNameProperty =
			BindableProperty.Create("GroupName", typeof(string), typeof(Maui.ILayout), null,
			propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).GroupName = (string)n; });

		/// <summary>
		/// Gets the group name for the specified layout, used to logically group radio buttons.
		/// </summary>
		/// <param name="b">The bindable object to query.</param>
		/// <returns>The group name assigned to the layout.</returns>
		public static string GetGroupName(BindableObject b)
		{
			return (string)b.GetValue(GroupNameProperty);
		}

		/// <summary>
		/// Sets the group name for the specified layout, used to logically group radio buttons.
		/// </summary>
		/// <param name="bindable">The bindable object to update.</param>
		/// <param name="groupName">The group name to assign.</param>
		public static void SetGroupName(BindableObject bindable, string groupName)
		{
			bindable.SetValue(GroupNameProperty, groupName);
		}

		/// <summary>Bindable property for attached property <c>SelectedValue</c>.</summary>
		public static readonly BindableProperty SelectedValueProperty =
			BindableProperty.Create("SelectedValue", typeof(object), typeof(Maui.ILayout), null,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).SelectedValue = n; });

		/// <summary>
		/// Gets the currently selected value from the radio button group.
		/// </summary>
		/// <param name="bindableObject">The bindable object to query.</param>
		/// <returns>The value of the selected radio button in the group.</returns>
		public static object GetSelectedValue(BindableObject bindableObject)
		{
			return bindableObject.GetValue(SelectedValueProperty);
		}

		/// <summary>
		/// Sets the selected value for the radio button group.
		/// </summary>
		/// <param name="bindable">The bindable object to update.</param>
		/// <param name="selectedValue">The value to select.</param>
		public static void SetSelectedValue(BindableObject bindable, object selectedValue)
		{
			bindable.SetValue(SelectedValueProperty, selectedValue);
		}

		internal static void UpdateRadioButtonGroup(RadioButton radioButton)
		{
			UncheckOtherRadioButtonsInScope(radioButton);
			radioButton.OnGroupSelectionChanged(radioButton);
		}

		internal static void UncheckOtherRadioButtonsInScope(RadioButton radioButton)
		{
			if (!string.IsNullOrEmpty(radioButton.GroupName))
			{
				var root = GetVisualRoot(radioButton) ?? radioButton.Parent;
				if (root is not IElementController rootController)
				{
					return;
				}

				foreach (var child in rootController.LogicalChildren)
				{
					UncheckMatchingDescendants(child, radioButton.GroupName, radioButton);
				}
			}
			else
			{
				if (radioButton.Parent is not IElementController parentController)
				{
					return;
				}

				foreach (var child in parentController.LogicalChildren)
				{
					if (child is RadioButton rb && string.IsNullOrEmpty(rb.GroupName))
					{
						UncheckRadioButtonIfChecked(rb, radioButton);
					}
				}
			}
		}

		static void UncheckRadioButtonIfChecked(RadioButton child, RadioButton radioButton)
		{
			if (child != radioButton && child.IsChecked)
			{
				child.SetValue(RadioButton.IsCheckedProperty, false, specificity: SetterSpecificity.FromHandler);
			}
		}

		static void UncheckMatchingDescendants(Element element, string groupName, RadioButton radioButton)
		{
			if (element is RadioButton rb && rb.GroupName == groupName)
			{
				UncheckRadioButtonIfChecked(rb, radioButton);
			}

			if (element is IElementController controller)
			{
				foreach (var child in controller.LogicalChildren)
				{
					UncheckMatchingDescendants(child, groupName, radioButton);
				}
			}
		}

		static void OnControllerChanged(BindableObject bindableObject, RadioButtonGroupController oldController,
			RadioButtonGroupController newController)
		{
			if (newController == null)
			{
				return;
			}

			newController.GroupName = GetGroupName(bindableObject);
			newController.SelectedValue = GetSelectedValue(bindableObject);
		}

		internal static Element GetVisualRoot(Element element)
		{
			Element parent = element.Parent;
			while (parent != null && !(parent is Page))
			{
				parent = parent.Parent;
			}
			return parent;
		}
	}
}
