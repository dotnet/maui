using System.Collections;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RadioButtonGroup.xml" path="Type[@FullName='Microsoft.Maui.Controls.RadioButtonGroup']/Docs/*" />
	public static class RadioButtonGroup
	{
		internal const string GroupSelectionChangedMessage = "RadioButtonGroupSelectionChanged";
		internal const string GroupValueChangedMessage = "RadioButtonGroupValueChanged";

		static readonly BindableProperty RadioButtonGroupControllerProperty =
			BindableProperty.CreateAttached("RadioButtonGroupController", typeof(RadioButtonGroupController), typeof(Maui.ILayout), default(RadioButtonGroupController),
			defaultValueCreator: (b) => new RadioButtonGroupController(b as Maui.ILayout),
			propertyChanged: (b, o, n) => OnControllerChanged(b, (RadioButtonGroupController)o, (RadioButtonGroupController)n));

		static RadioButtonGroupController GetRadioButtonGroupController(BindableObject b)
		{
			return (RadioButtonGroupController)b.GetValue(RadioButtonGroupControllerProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButtonGroup.xml" path="//Member[@MemberName='GroupNameProperty']/Docs/*" />
		public static readonly BindableProperty GroupNameProperty =
			BindableProperty.Create("GroupName", typeof(string), typeof(Maui.ILayout), null,
			propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).GroupName = (string)n; });

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButtonGroup.xml" path="//Member[@MemberName='GetGroupName']/Docs/*" />
		public static string GetGroupName(BindableObject b)
		{
			return (string)b.GetValue(GroupNameProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButtonGroup.xml" path="//Member[@MemberName='SetGroupName']/Docs/*" />
		public static void SetGroupName(BindableObject bindable, string groupName)
		{
			bindable.SetValue(GroupNameProperty, groupName);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButtonGroup.xml" path="//Member[@MemberName='SelectedValueProperty']/Docs/*" />
		public static readonly BindableProperty SelectedValueProperty =
			BindableProperty.Create("SelectedValue", typeof(object), typeof(Maui.ILayout), null,
			defaultBindingMode: BindingMode.TwoWay,
			propertyChanged: (b, o, n) => { GetRadioButtonGroupController(b).SelectedValue = n; });

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButtonGroup.xml" path="//Member[@MemberName='GetSelectedValue']/Docs/*" />
		public static object GetSelectedValue(BindableObject bindableObject)
		{
			return bindableObject.GetValue(SelectedValueProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadioButtonGroup.xml" path="//Member[@MemberName='SetSelectedValue']/Docs/*" />
		public static void SetSelectedValue(BindableObject bindable, object selectedValue)
		{
			bindable.SetValue(SelectedValueProperty, selectedValue);
		}

		internal static void UpdateRadioButtonGroup(RadioButton radioButton)
		{
			string groupName = radioButton.GroupName;

			Element scope = string.IsNullOrEmpty(groupName)
				? GroupByParent(radioButton)
				: GetVisualRoot(radioButton);

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			MessagingCenter.Send(radioButton, GroupSelectionChangedMessage,
				new RadioButtonGroupSelectionChanged(scope));
#pragma warning restore CS0618 // Type or member is obsolete
		}

		internal static Element GroupByParent(RadioButton radioButton)
		{
			Element parent = radioButton.Parent;

			if (parent != null)
			{
				// Traverse logical children
				IEnumerable children = ((IElementController)parent).LogicalChildren;
				IEnumerator itor = children.GetEnumerator();
				while (itor.MoveNext())
				{
					var rb = itor.Current as RadioButton;
					if (rb != null && rb != radioButton && string.IsNullOrEmpty(rb.GroupName) && (rb.IsChecked == true))
						rb.SetValueFromRenderer(RadioButton.IsCheckedProperty, false);
				}
			}

			return parent;
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
				parent = parent.Parent;
			return parent;
		}
	}
}