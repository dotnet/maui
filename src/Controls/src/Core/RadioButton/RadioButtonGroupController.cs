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

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			MessagingCenter.Subscribe<RadioButton, RadioButtonGroupSelectionChanged>(this,
				RadioButtonGroup.GroupSelectionChangedMessage, HandleRadioButtonGroupSelectionChanged);
			MessagingCenter.Subscribe<RadioButton, RadioButtonGroupNameChanged>(this, RadioButton.GroupNameChangedMessage,
				HandleRadioButtonGroupNameChanged);
			MessagingCenter.Subscribe<RadioButton, RadioButtonValueChanged>(this, RadioButton.ValueChangedMessage,
				HandleRadioButtonValueChanged);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		bool MatchesScope(RadioButtonScopeMessage message)
		{
			return RadioButtonGroup.GetVisualRoot(_layout) == message.Scope;
		}

		void HandleRadioButtonGroupSelectionChanged(RadioButton selected, RadioButtonGroupSelectionChanged args)
		{
			if (selected.GroupName != _groupName || !MatchesScope(args))
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, selected.Value);
		}

		void HandleRadioButtonGroupNameChanged(RadioButton radioButton, RadioButtonGroupNameChanged args)
		{
			if (args.OldName != _groupName || !MatchesScope(args))
			{
				return;
			}

			_layout.ClearValue(RadioButtonGroup.SelectedValueProperty);
		}

		void HandleRadioButtonValueChanged(RadioButton radioButton, RadioButtonValueChanged args)
		{
			if (radioButton.GroupName != _groupName || !MatchesScope(args))
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, radioButton.Value);
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
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				MessagingCenter.Send<Element, RadioButtonGroupValueChanged>(_layout, RadioButtonGroup.GroupValueChangedMessage,
					new RadioButtonGroupValueChanged(_groupName, RadioButtonGroup.GetVisualRoot(_layout), radioButtonValue));
#pragma warning restore CS0618 // Type or member is obsolete
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