using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public class SelectableItemsView : ItemsView
	{
		public static readonly BindableProperty SelectionModeProperty =
			BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(SelectableItemsView),
				SelectionMode.None, propertyChanged: SelectionModePropertyChanged );

		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SelectableItemsView), default(object),
				propertyChanged: SelectedItemPropertyChanged);

		static readonly BindablePropertyKey SelectedItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(SelectedItems), typeof(IList<object>), typeof(SelectableItemsView), null);

		public static readonly BindableProperty SelectedItemsProperty = SelectedItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty SelectionChangedCommandProperty =
			BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(SelectableItemsView));

		public static readonly BindableProperty SelectionChangedCommandParameterProperty =
			BindableProperty.Create(nameof(SelectionChangedCommandParameter), typeof(object),
				typeof(SelectableItemsView));

		public SelectableItemsView()
		{
			var selectionList = new SelectionList(this);
			SetValue(SelectedItemsPropertyKey, selectionList);
		}

		public object SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		public IList<object> SelectedItems
		{
			get => (IList<object>)GetValue(SelectedItemsProperty);
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

		protected virtual void OnSelectionChanged(SelectionChangedEventArgs args)
		{
		}

		internal void SelectedItemsPropertyChanged(IList<object> oldSelection, IList<object> newSelection)
		{
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
