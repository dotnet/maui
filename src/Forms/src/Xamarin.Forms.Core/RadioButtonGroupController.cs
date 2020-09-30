using System;

namespace Xamarin.Forms
{
	internal class RadioButtonGroupController
	{
		readonly Layout<View> _layout;

		string _groupName;
		private object _selectedValue;

		public string GroupName { get => _groupName; set => SetGroupName(value); }
		public object SelectedValue { get => _selectedValue; set => SetSelectedValue(value); }

		public RadioButtonGroupController(Layout<View> layout)
		{
			if (layout is null)
			{
				throw new ArgumentNullException(nameof(layout));
			}

			_layout = layout;
			layout.ChildAdded += ChildAdded;

			if (!string.IsNullOrEmpty(_groupName))
			{
				UpdateGroupNames(layout, _groupName);
			}

			MessagingCenter.Subscribe<RadioButton, RadioButtonGroupSelectionChanged>(this,
				RadioButtonGroup.GroupSelectionChangedMessage, HandleRadioButtonGroupSelectionChanged);
			MessagingCenter.Subscribe<RadioButton, RadioButtonGroupNameChanged>(this, RadioButton.GroupNameChangedMessage,
				HandleRadioButtonGroupNameChanged);
			MessagingCenter.Subscribe<RadioButton, RadioButtonValueChanged>(this, RadioButton.ValueChangedMessage,
				HandleRadioButtonValueChanged);
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

		void UpdateGroupNames(Layout<View> layout, string name, string oldName = null)
		{
			foreach (var descendant in layout.Descendants())
			{
				UpdateGroupName(descendant, name, oldName);
			}
		}

		void SetSelectedValue(object radioButtonValue)
		{
			_selectedValue = radioButtonValue;

			if (radioButtonValue != null)
			{
				MessagingCenter.Send(_layout, RadioButtonGroup.GroupValueChangedMessage,
					new RadioButtonGroupValueChanged(_groupName, RadioButtonGroup.GetVisualRoot(_layout), radioButtonValue));
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