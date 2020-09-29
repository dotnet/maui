using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public class SelectableItemsView : StructuredItemsView
	{
		public static readonly BindableProperty SelectionModeProperty =
			BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(SelectableItemsView),
				SelectionMode.None, propertyChanged: SelectionModePropertyChanged);

		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SelectableItemsView), default(object),
				defaultBindingMode: BindingMode.TwoWay,
				propertyChanged: SelectedItemPropertyChanged);

		public static readonly BindableProperty SelectedItemsProperty =
			BindableProperty.Create(nameof(SelectedItems), typeof(IList<object>), typeof(SelectableItemsView), null,
				defaultBindingMode: BindingMode.OneWay,
				propertyChanged: SelectedItemsPropertyChanged,
				coerceValue: CoerceSelectedItems,
				defaultValueCreator: DefaultValueCreator);

		public static readonly BindableProperty SelectionChangedCommandProperty =
			BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(SelectableItemsView));

		public static readonly BindableProperty SelectionChangedCommandParameterProperty =
			BindableProperty.Create(nameof(SelectionChangedCommandParameter), typeof(object),
				typeof(SelectableItemsView));

		static readonly IList<object> s_empty = new List<object>(0);

		bool _suppressSelectionChangeNotification;

		public SelectableItemsView()
		{
		}

		public object SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		public IList<object> SelectedItems
		{
			get => (IList<object>)GetValue(SelectedItemsProperty);
			set => SetValue(SelectedItemsProperty, new SelectionList(this, value));
		}

		public ICommand SelectionChangedCommand
		{
			get => (ICommand)GetValue(SelectionChangedCommandProperty);
			set => SetValue(SelectionChangedCommandProperty, value);
		}

		public object SelectionChangedCommandParameter
		{
			get => GetValue(SelectionChangedCommandParameterProperty);
			set => SetValue(SelectionChangedCommandParameterProperty, value);
		}

		public SelectionMode SelectionMode
		{
			get => (SelectionMode)GetValue(SelectionModeProperty);
			set => SetValue(SelectionModeProperty, value);
		}

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

		public void UpdateSelectedItems(IList<object> newSelection)
		{
			var oldSelection = new List<object>(SelectedItems);

			_suppressSelectionChangeNotification = true;

			SelectedItems.Clear();

			if (newSelection?.Count > 0)
			{
				for (int n = 0; n < newSelection.Count; n++)
				{
					SelectedItems.Add(newSelection[n]);
				}
			}

			_suppressSelectionChangeNotification = false;

			SelectedItemsPropertyChanged(oldSelection, newSelection);
		}

		protected virtual void OnSelectionChanged(SelectionChangedEventArgs args)
		{
		}

		static object CoerceSelectedItems(BindableObject bindable, object value)
		{
			if (value == null)
			{
				return new SelectionList((SelectableItemsView)bindable);
			}

			if (value is SelectionList)
			{
				return value;
			}

			return new SelectionList((SelectableItemsView)bindable, value as IList<object>);
		}

		static object DefaultValueCreator(BindableObject bindable)
		{
			return new SelectionList((SelectableItemsView)bindable);
		}

		static void SelectedItemsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var selectableItemsView = (SelectableItemsView)bindable;
			var oldSelection = (IList<object>)oldValue ?? s_empty;
			var newSelection = (IList<object>)newValue ?? s_empty;

			selectableItemsView.SelectedItemsPropertyChanged(oldSelection, newSelection);
		}

		internal void SelectedItemsPropertyChanged(IList<object> oldSelection, IList<object> newSelection)
		{
			if (_suppressSelectionChangeNotification)
			{
				return;
			}

			SelectionPropertyChanged(this, new SelectionChangedEventArgs(oldSelection, newSelection));

			OnPropertyChanged(SelectedItemsProperty.PropertyName);
		}

		static void SelectionPropertyChanged(SelectableItemsView selectableItemsView, SelectionChangedEventArgs args)
		{
			var command = selectableItemsView.SelectionChangedCommand;

			if (command != null)
			{
				var commandParameter = selectableItemsView.SelectionChangedCommandParameter;

				if (command.CanExecute(commandParameter))
				{
					command.Execute(commandParameter);
				}
			}

			selectableItemsView.SelectionChanged?.Invoke(selectableItemsView, args);
			selectableItemsView.OnSelectionChanged(args);
		}

		static void SelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var selectableItemsView = (SelectableItemsView)bindable;

			var args = new SelectionChangedEventArgs(oldValue, newValue);

			SelectionPropertyChanged(selectableItemsView, args);
		}

		static void SelectionModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var selectableItemsView = (SelectableItemsView)bindable;

			var oldMode = (SelectionMode)oldValue;
			var newMode = (SelectionMode)newValue;

			IList<object> previousSelection = new List<object>();
			IList<object> newSelection = new List<object>();

			switch (oldMode)
			{
				case SelectionMode.None:
					break;
				case SelectionMode.Single:
					if (selectableItemsView.SelectedItem != null)
					{
						previousSelection.Add(selectableItemsView.SelectedItem);
					}
					break;
				case SelectionMode.Multiple:
					previousSelection = selectableItemsView.SelectedItems;
					break;
			}

			switch (newMode)
			{
				case SelectionMode.None:
					break;
				case SelectionMode.Single:
					if (selectableItemsView.SelectedItem != null)
					{
						newSelection.Add(selectableItemsView.SelectedItem);
					}
					break;
				case SelectionMode.Multiple:
					newSelection = selectableItemsView.SelectedItems;
					break;
			}

			if (previousSelection.Count == newSelection.Count)
			{
				if (previousSelection.Count == 0 || (previousSelection[0] == newSelection[0]))
				{
					// Both selections are empty or have the same single item; no reason to signal a change
					return;
				}
			}

			var args = new SelectionChangedEventArgs(previousSelection, newSelection);
			SelectionPropertyChanged(selectableItemsView, args);
		}
	}
}