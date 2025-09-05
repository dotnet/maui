#nullable disable
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SelectableItemsView.xml" path="Type[@FullName='Microsoft.Maui.Controls.SelectableItemsView']/Docs/*" />
	public class SelectableItemsView : StructuredItemsView
	{
		/// <summary>Bindable property for <see cref="SelectionMode"/>.</summary>
		public static readonly BindableProperty SelectionModeProperty =
			BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(SelectableItemsView),
				SelectionMode.None, propertyChanged: SelectionModePropertyChanged);

		/// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SelectableItemsView), default(object),
				defaultBindingMode: BindingMode.TwoWay,
				propertyChanged: SelectedItemPropertyChanged);

		/// <summary>Bindable property for <see cref="SelectedItems"/>.</summary>
		public static readonly BindableProperty SelectedItemsProperty =
			BindableProperty.Create(nameof(SelectedItems), typeof(IList<object>), typeof(SelectableItemsView), null,
				defaultBindingMode: BindingMode.OneWay,
				propertyChanged: SelectedItemsPropertyChanged,
				coerceValue: CoerceSelectedItems,
				defaultValueCreator: DefaultValueCreator);

		/// <summary>Bindable property for <see cref="SelectionChangedCommand"/>.</summary>
		public static readonly BindableProperty SelectionChangedCommandProperty =
			BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(SelectableItemsView));

		/// <summary>Bindable property for <see cref="SelectionChangedCommandParameter"/>.</summary>
		public static readonly BindableProperty SelectionChangedCommandParameterProperty =
			BindableProperty.Create(nameof(SelectionChangedCommandParameter), typeof(object),
				typeof(SelectableItemsView));

		static readonly IList<object> s_empty = new List<object>(0);

		bool _suppressSelectionChangeNotification;

		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectableItemsView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public SelectableItemsView()
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectableItemsView.xml" path="//Member[@MemberName='SelectedItem']/Docs/*" />
		public object SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectableItemsView.xml" path="//Member[@MemberName='SelectedItems']/Docs/*" />
		public IList<object> SelectedItems
		{
			get => (IList<object>)GetValue(SelectedItemsProperty);
			set => SetValue(SelectedItemsProperty, new SelectionList(this, value));
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectableItemsView.xml" path="//Member[@MemberName='SelectionChangedCommand']/Docs/*" />
		public ICommand SelectionChangedCommand
		{
			get => (ICommand)GetValue(SelectionChangedCommandProperty);
			set => SetValue(SelectionChangedCommandProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectableItemsView.xml" path="//Member[@MemberName='SelectionChangedCommandParameter']/Docs/*" />
		public object SelectionChangedCommandParameter
		{
			get => GetValue(SelectionChangedCommandParameterProperty);
			set => SetValue(SelectionChangedCommandParameterProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectableItemsView.xml" path="//Member[@MemberName='SelectionMode']/Docs/*" />
		public SelectionMode SelectionMode
		{
			get => (SelectionMode)GetValue(SelectionModeProperty);
			set => SetValue(SelectionModeProperty, value);
		}

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

		/// <param name="newSelection">The newSelection parameter.</param>
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
