#nullable disable
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	internal class RadioButtonGroupController
	{
		static readonly ConditionalWeakTable<RadioButton, RadioButtonGroupController> groupControllers = new();
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
			_layout.ChildRemoved += ChildRemoved;

			if (!string.IsNullOrEmpty(_groupName))
			{
				UpdateGroupNames(_layout, _groupName);
			}
		}

		internal static RadioButtonGroupController GetGroupController(RadioButton radioButton)
		{
			if (radioButton is not null && groupControllers.TryGetValue(radioButton, out var controller))
			{
				return controller;
			}
			return null;
		}

		internal void HandleRadioButtonGroupSelectionChanged(RadioButton radioButton)
		{
			if (radioButton.GroupName != _groupName)
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, radioButton.Value);
		}

		void ChildAdded(object sender, ElementEventArgs e)
		{
			if (string.IsNullOrEmpty(_groupName) || _layout == null)
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
					if (element is RadioButton childRadioButton)
					{
						AddRadioButton(childRadioButton);
					}
				}
			}
		}

		void ChildRemoved(object sender, ElementEventArgs e)
		{
			if (e.Element is RadioButton radioButton)
			{
				if (groupControllers.TryGetValue(radioButton, out _))
				{
					groupControllers.Remove(radioButton);
				}
			}
			else
			{
				foreach (var element in e.Element.Descendants())
				{
					if (element is RadioButton radioButton1)
					{
						if (groupControllers.TryGetValue(radioButton1, out _))
						{
							groupControllers.Remove(radioButton1);
						}
					}
				}
			}
		}

		internal void HandleRadioButtonValueChanged(RadioButton radioButton)
		{
			if (radioButton?.GroupName != _groupName)
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, radioButton.Value);
		}

		internal void HandleRadioButtonGroupNameChanged(string oldGroupName)
		{
			if (oldGroupName != _groupName)
			{
				return;
			}

			_layout.ClearValue(RadioButtonGroup.SelectedValueProperty);
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

			if (!groupControllers.TryGetValue(radioButton, out _))
			{
				groupControllers.Add(radioButton, this);
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
			if(object.Equals(_selectedValue, radioButtonValue))
			{
				return;
			}

			_selectedValue = radioButtonValue;

			if (radioButtonValue != null)
			{
				foreach (var child in _layout.Descendants())
				{
					if (child is RadioButton radioButton && radioButton.GroupName == _groupName && radioButton.Value is not null && radioButton.Value.Equals(radioButtonValue))
					{
						radioButton.SetValue(RadioButton.IsCheckedProperty, true, specificity: SetterSpecificity.FromHandler);
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