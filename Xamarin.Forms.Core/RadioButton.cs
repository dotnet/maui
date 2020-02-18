using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_RadioButtonRenderer))]
	public class RadioButton : Button, IElementConfiguration<RadioButton>
	{
		readonly Lazy<PlatformConfigurationRegistry<RadioButton>> _platformConfigurationRegistry;

		static Dictionary<string, List<WeakReference<RadioButton>>> _groupNameToElements;

		public const string IsCheckedVisualState = "IsChecked";

		public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
			nameof(IsChecked), typeof(bool), typeof(RadioButton), false, propertyChanged: (b, o, n) => ((RadioButton)b).OnIsCheckedPropertyChanged((bool)n), defaultBindingMode: BindingMode.TwoWay);

		public static readonly BindableProperty GroupNameProperty = BindableProperty.Create(
			nameof(GroupName), typeof(string), typeof(RadioButton), null, propertyChanged: (b, o, n) => ((RadioButton)b).OnGroupNamePropertyChanged((string)o, (string)n));

		public static readonly BindableProperty ButtonSourceProperty = BindableProperty.Create(
			nameof(ButtonSource), typeof(ImageSource), typeof(RadioButton), null);

		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public string GroupName
		{
			get { return (string)GetValue(GroupNameProperty); }
			set { SetValue(GroupNameProperty, value); }
		}

		public ImageSource ButtonSource
		{
			get { return (ImageSource)GetValue(ButtonSourceProperty); }
			set { SetValue(ButtonSourceProperty, value); }
		}

		public RadioButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<RadioButton>>(() => new PlatformConfigurationRegistry<RadioButton>(this));
		}

		public new IPlatformElementConfiguration<T, RadioButton> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsChecked)
				VisualStateManager.GoToState(this, IsCheckedVisualState);
			else
				base.ChangeVisualState();
		}

		void OnIsCheckedPropertyChanged(bool isChecked)
		{
			if (isChecked)
				UpdateRadioButtonGroup();

			CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(isChecked));
			ChangeVisualState();
		}

		void OnGroupNamePropertyChanged(string oldGroupName, string newGroupName)
		{
			// Unregister the old group name if set
			if (!string.IsNullOrEmpty(oldGroupName))
				Unregister(this, oldGroupName);

			// Register the new group name is set
			if (!string.IsNullOrEmpty(newGroupName))
				Register(this, newGroupName);
		}

		void UpdateRadioButtonGroup()
		{
			string groupName = GroupName;
			if (!string.IsNullOrEmpty(groupName))
			{
				Element rootScope = GetVisualRoot(this);

				if (_groupNameToElements == null)
					_groupNameToElements = new Dictionary<string, List<WeakReference<RadioButton>>>(1);

				// Get all elements bound to this key and remove this element
				List<WeakReference<RadioButton>> elements = _groupNameToElements[groupName];
				for (int i = 0; i < elements.Count;)
				{
					WeakReference<RadioButton> weakRef = elements[i];
					if (weakRef.TryGetTarget(out RadioButton rb))
					{
						// Uncheck all checked RadioButtons different from the current one
						if (rb != this && (rb.IsChecked == true) && rootScope == GetVisualRoot(rb))
							rb.SetValueFromRenderer(IsCheckedProperty, false);

						i++;
					}
					else
					{
						// Remove dead instances
						elements.RemoveAt(i);
					}
				}
			}
			else // Logical parent should be the group
			{
				Element parent = Parent;
				if (parent != null)
				{
					// Traverse logical children
					IEnumerable children = parent.LogicalChildren;
					IEnumerator itor = children.GetEnumerator();
					while (itor.MoveNext())
					{
						var rb = itor.Current as RadioButton;
						if (rb != null && rb != this && string.IsNullOrEmpty(rb.GroupName) && (rb.IsChecked == true))
							rb.SetValueFromRenderer(IsCheckedProperty, false);
					}
				}
			}
		}

		static void Register(RadioButton radioButton, string groupName)
		{
			if (_groupNameToElements == null)
				_groupNameToElements = new Dictionary<string, List<WeakReference<RadioButton>>>(1);

			if (_groupNameToElements.TryGetValue(groupName, out List<WeakReference<RadioButton>> elements))
			{
				// There were some elements there, remove dead ones
				PurgeDead(elements, null);
			}
			else
			{
				elements = new List<WeakReference<RadioButton>>(1);
				_groupNameToElements[groupName] = elements;
			}

			elements.Add(new WeakReference<RadioButton>(radioButton));
		}

		static void Unregister(RadioButton radioButton, string groupName)
		{
			if (_groupNameToElements == null)
				return;

			// Get all elements bound to this key and remove this element
			if (_groupNameToElements.TryGetValue(groupName, out List<WeakReference<RadioButton>> elements))
			{
				PurgeDead(elements, radioButton);

				if (elements.Count == 0)
					_groupNameToElements.Remove(groupName);
			}
		}

		static void PurgeDead(List<WeakReference<RadioButton>> elements, object elementToRemove)
		{
			for (int i = 0; i < elements.Count;)
			{
				if (elements[i].TryGetTarget(out RadioButton rb) && rb == elementToRemove)
					elements.RemoveAt(i);
				else
					i++;
			}
		}

		static Element GetVisualRoot(Element element)
		{
			Element parent = element.Parent;
			while (parent != null && !(parent is Page))
				parent = parent.Parent;
			return parent;
		}
	}
}