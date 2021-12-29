using System;
using CommunityToolkit.Mvvm.Messaging;

namespace Microsoft.Maui.Controls
{
	internal class RadioButtonGroupController
	{
		readonly Compatibility.Layout<View> _layout;

		string _groupName;
		private object _selectedValue;

		public string GroupName { get => _groupName; set => SetGroupName(value); }
		public object SelectedValue { get => _selectedValue; set => SetSelectedValue(value); }

		public RadioButtonGroupController(Compatibility.Layout<View> layout)
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

			WeakReferenceMessenger.Default.Register<RadioButtonGroupController, RadioButtonGroupSelectionChanged>(this, HandleRadioButtonGroupSelectionChanged);
			WeakReferenceMessenger.Default.Register<RadioButtonGroupController, RadioButtonGroupNameChanged>(this, HandleRadioButtonGroupNameChanged);
			WeakReferenceMessenger.Default.Register<RadioButtonGroupController, RadioButtonValueChanged>(this, HandleRadioButtonValueChanged);
		}

		bool MatchesScope(RadioButtonScopeMessage message)
		{
			return RadioButtonGroup.GetVisualRoot(_layout) == message.Scope;
		}

		void HandleRadioButtonGroupSelectionChanged(RadioButtonGroupController controller, RadioButtonGroupSelectionChanged args)
		{
			var selected = args.RadioButton;

			if (selected.GroupName != _groupName || !MatchesScope(args))
			{
				return;
			}

			_layout.SetValue(RadioButtonGroup.SelectedValueProperty, selected.Value);
		}

		void HandleRadioButtonGroupNameChanged(RadioButtonGroupController controller, RadioButtonGroupNameChanged args)
		{
			if (args.OldName != _groupName || !MatchesScope(args))
			{
				return;
			}

			_layout.ClearValue(RadioButtonGroup.SelectedValueProperty);
		}

		void HandleRadioButtonValueChanged(RadioButtonGroupController controller, RadioButtonValueChanged args)
		{
			var radioButton = args.RadioButton;

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

		void UpdateGroupNames(Compatibility.Layout<View> layout, string name, string oldName = null)
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
				WeakReferenceMessenger.Default.Send(new RadioButtonGroupValueChanged(_groupName, RadioButtonGroup.GetVisualRoot(_layout), radioButtonValue));
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