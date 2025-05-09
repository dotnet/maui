#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	internal class RadioButtonGroupController
	{
		readonly Element _layout;
		string _groupName;
		private object _selectedValue;

		public string GroupName { get => _groupName; set => SetGroupName(value); }
		public object SelectedValue { get => _selectedValue; set => SetSelectedValue(value); }

		public RadioButtonGroupController(Maui.ILayout layout)
		{
			if (layout is null)
			{
				throw new ArgumentNullException(nameof(layout));
			}

			_layout = (Element)layout;
			_layout.ChildAdded += ChildAdded;

			if (!string.IsNullOrEmpty(_groupName))
			{
				UpdateGroupNames(_layout, _groupName);
			}
		}

		internal void HandleRadioButtonGroupSelectionChanged(RadioButton selected)
		{
			if (selected.GroupName != _groupName)
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, selected.Value);
		}

		void ChildAdded(object sender, ElementEventArgs e)
		{
			if (string.IsNullOrEmpty(_groupName))
			{
				return;
			}

			if (e.Element is RadioButton radioButton)
			{
				AddRadioButton(radioButton);
			}
			else
			{
				foreach (var element in e.Element.Descendants())
				{
					if (element is RadioButton radioButton1)
					{
						AddRadioButton(radioButton1);
					}
				}
			}
		}

		void AddRadioButton(RadioButton radioButton)
		{
			UpdateGroupName(radioButton, _groupName);

			if (radioButton.IsChecked)
			{
				_layout.SetValue(RadioButtonGroup.SelectedValueProperty, radioButton.Value);
			}

			if (object.Equals(radioButton.Value, this.SelectedValue))
			{
				radioButton.SetValue(RadioButton.IsCheckedProperty, true, specificity: SetterSpecificity.FromHandler);
			}
		}

		void UpdateGroupName(Element element, string name, string oldName = null)
		{
			if (!(element is RadioButton radioButton))
			{
				return;
			}

			var currentName = radioButton.GroupName;

			if (string.IsNullOrEmpty(currentName) || currentName == oldName)
			{
				radioButton.GroupName = name;
			}
		}

		void UpdateGroupNames(Element element, string name, string oldName = null)
		{
			foreach (Element descendant in element.Descendants())
			{
				UpdateGroupName(descendant, name, oldName);
			}
		}

		void SetSelectedValue(object radioButtonValue)
		{
			_selectedValue = radioButtonValue;

			if (radioButtonValue != null)
			{
				foreach (var child in ((IVisualTreeElement)_layout).GetVisualChildren())
				{
					if (child is RadioButton radioButton && radioButton.GroupName == _groupName && radioButton.Value is not null && radioButton.Value.Equals(radioButtonValue))
					{
						radioButton.SetValue(RadioButton.IsCheckedProperty, true);
					}
				}
			}
		}

		void SetGroupName(string groupName)
		{
			var oldName = _groupName;
			_groupName = groupName;
			UpdateGroupNames(_layout, _groupName, oldName);
		}
	}
}