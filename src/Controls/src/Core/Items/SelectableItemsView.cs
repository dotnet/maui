#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A structured items view that supports item selection.
	/// </summary>
	/// <remarks>
	/// <see cref="SelectableItemsView"/> extends <see cref="StructuredItemsView"/> to add selection capabilities.
	/// Use <see cref="SelectionMode"/> to control whether single or multiple items can be selected.
	/// The <see cref="SelectedItem"/> and <see cref="SelectedItems"/> properties track the current selection,
	/// and the <see cref="SelectionChanged"/> event notifies when selection changes.
	/// </remarks>
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

		/// <summary>Bindable property for <see cref="ItemTappedCommand"/>.</summary>
		public static readonly BindableProperty ItemTappedCommandProperty =
			BindableProperty.Create(nameof(ItemTappedCommand), typeof(ICommand), typeof(SelectableItemsView),
				propertyChanged: (bindable, oldValue, newValue) =>
					((SelectableItemsView)bindable).OnIsItemTapEnabledChanged(oldValue is not null || ((SelectableItemsView)bindable)._itemTapped is not null));

		/// <summary>Bindable property for <see cref="ItemTappedCommandParameter"/>.</summary>
		public static readonly BindableProperty ItemTappedCommandParameterProperty =
			BindableProperty.Create(nameof(ItemTappedCommandParameter), typeof(object), typeof(SelectableItemsView));

		static readonly IList<object> s_empty = new List<object>(0);

		bool _suppressSelectionChangeNotification;
		EventHandler<ItemTappedEventArgs> _itemTapped;

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectableItemsView"/> class.
		/// </summary>
		public SelectableItemsView()
		{
		}

		/// <summary>
		/// Gets or sets the currently selected item when <see cref="SelectionMode"/> is <see cref="SelectionMode.Single"/>.
		/// </summary>
		/// <value>The selected data item, or <see langword="null"/> if no item is selected.</value>
		/// <remarks>
		/// This property is only meaningful when <see cref="SelectionMode"/> is <see cref="SelectionMode.Single"/>.
		/// For multiple selection, use the <see cref="SelectedItems"/> property instead.
		/// The binding mode defaults to <see cref="BindingMode.TwoWay"/>.
		/// </remarks>
		public object SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		/// <summary>
		/// Gets or sets the collection of currently selected items when <see cref="SelectionMode"/> is <see cref="SelectionMode.Multiple"/>.
		/// </summary>
		/// <value>A list of selected data items. Never <see langword="null"/>; returns an empty list when no items are selected.</value>
		/// <remarks>
		/// This property tracks all selected items when <see cref="SelectionMode"/> is <see cref="SelectionMode.Multiple"/>.
		/// For single selection, use the <see cref="SelectedItem"/> property instead for simpler binding.
		/// </remarks>
		public IList<object> SelectedItems
		{
			get => (IList<object>)GetValue(SelectedItemsProperty);
			set => SetValue(SelectedItemsProperty, new SelectionList(this, value));
		}

		/// <summary>
		/// Gets or sets the command to execute when the selection changes.
		/// </summary>
		/// <value>An <see cref="ICommand"/> to execute when selection changes, or <see langword="null"/> for no command.</value>
		/// <remarks>
		/// The command's parameter is set to the <see cref="SelectionChangedCommandParameter"/> value.
		/// The command executes in addition to the <see cref="SelectionChanged"/> event firing.
		/// </remarks>
		public ICommand SelectionChangedCommand
		{
			get => (ICommand)GetValue(SelectionChangedCommandProperty);
			set => SetValue(SelectionChangedCommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="SelectionChangedCommand"/>.
		/// </summary>
		/// <value>The command parameter, or <see langword="null"/> if no parameter is needed.</value>
		public object SelectionChangedCommandParameter
		{
			get => GetValue(SelectionChangedCommandParameterProperty);
			set => SetValue(SelectionChangedCommandParameterProperty, value);
		}

		/// <summary>
		/// Gets or sets the selection mode, which determines whether users can select no items, one item, or multiple items.
		/// </summary>
		/// <value>A <see cref="SelectionMode"/> value. The default is <see cref="SelectionMode.None"/>.</value>
		/// <remarks>
		/// Set to <see cref="SelectionMode.Single"/> to allow selecting one item at a time,
		/// <see cref="SelectionMode.Multiple"/> to allow selecting multiple items,
		/// or <see cref="SelectionMode.None"/> to disable selection entirely.
		/// </remarks>
		public SelectionMode SelectionMode
		{
			get => (SelectionMode)GetValue(SelectionModeProperty);
			set => SetValue(SelectionModeProperty, value);
		}

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

		/// <summary>
		/// Gets or sets the command to execute when the user taps an item.
		/// </summary>
		/// <value>An <see cref="ICommand"/> to execute when an item is tapped, or <see langword="null"/> for no command.</value>
		/// <remarks>
		/// The command's parameter is the <see cref="ItemTappedCommandParameter"/> value when that property is set;
		/// otherwise it is the tapped item. The command executes before the <see cref="ItemTapped"/> event is raised.
		/// See <see cref="ItemTapped"/> for when a tap is reported.
		/// </remarks>
		public ICommand ItemTappedCommand
		{
			get => (ICommand)GetValue(ItemTappedCommandProperty);
			set => SetValue(ItemTappedCommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="ItemTappedCommand"/>.
		/// </summary>
		/// <value>The command parameter. When this property is not set, the tapped item is passed to the command.</value>
		public object ItemTappedCommandParameter
		{
			get => GetValue(ItemTappedCommandParameterProperty);
			set => SetValue(ItemTappedCommandParameterProperty, value);
		}

		/// <summary>
		/// Occurs when the user taps or otherwise activates an item.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The event is raised for every user activation of an item, including when the item is already selected,
		/// and regardless of <see cref="SelectionMode"/>. When selection is enabled, the selection is updated
		/// (and <see cref="SelectionChanged"/> is raised, if it changed) before this event is raised.
		/// </para>
		/// <para>
		/// The event is not raised when the selection is changed programmatically, or for taps that are handled
		/// by a control or gesture recognizer inside the item template.
		/// </para>
		/// <para>
		/// When <see cref="SelectionMode"/> is <see cref="SelectionMode.None"/>, items only respond to taps
		/// while a handler is attached to this event or <see cref="ItemTappedCommand"/> is set.
		/// </para>
		/// <para>
		/// <see cref="ItemTappedEventArgs.ItemIndex"/> follows <see cref="ListView.ItemTapped"/>: for an ungrouped source
		/// it is the index of the item in the source; when <see cref="GroupableItemsView.IsGrouped"/> is <see langword="true"/>,
		/// it is the position in the flattened list where each group occupies one row for its header followed by its items.
		/// </para>
		/// </remarks>
		public event EventHandler<ItemTappedEventArgs> ItemTapped
		{
			add
			{
				var wasEnabled = IsItemTapEnabled;
				_itemTapped += value;
				OnIsItemTapEnabledChanged(wasEnabled);
			}
			remove
			{
				var wasEnabled = IsItemTapEnabled;
				_itemTapped -= value;
				OnIsItemTapEnabledChanged(wasEnabled);
			}
		}

		/// <summary>Handler mapper key that is updated when <see cref="IsItemTapEnabled"/> changes.</summary>
		internal const string IsItemTapEnabledMapperKey = nameof(IsItemTapEnabled);

		/// <summary>
		/// Gets whether items should report taps, which is when <see cref="ItemTapped"/> has a handler or <see cref="ItemTappedCommand"/> is set.
		/// </summary>
		internal bool IsItemTapEnabled => _itemTapped is not null || ItemTappedCommand is not null;

		void OnIsItemTapEnabledChanged(bool wasEnabled)
		{
			if (wasEnabled != IsItemTapEnabled)
			{
				Handler?.UpdateValue(IsItemTapEnabledMapperKey);
			}
		}

		/// <summary>
		/// Captures a user tap on an item. Platform handlers call this before the tap updates the selection, so that
		/// selection callbacks that change the items source can't invalidate the tapped position, and pass the result
		/// to <see cref="SendItemTapped(ItemTappedEventArgs)"/> after the selection has been updated.
		/// </summary>
		/// <param name="item">The tapped item.</param>
		/// <param name="group">The group containing the item, or <see langword="null"/> when the source is not grouped.</param>
		/// <param name="groupIndex">The index of the group, or -1 when the source is not grouped or the position is unknown.</param>
		/// <param name="indexInGroup">The index of the item within its group (or within the source when it is not grouped), or -1 when unknown.</param>
		/// <returns>The event arguments, or <see langword="null"/> when taps aren't observed.</returns>
		internal ItemTappedEventArgs CreateItemTappedEventArgs(object item, object group, int groupIndex, int indexInGroup)
		{
			if (!IsItemTapEnabled)
			{
				return null;
			}

			var isGrouped = this is GroupableItemsView { IsGrouped: true };

			if (isGrouped && indexInGroup >= 0)
			{
				// Handlers may know only one of the group and its index
				if (groupIndex < 0 && group is not null)
				{
					groupIndex = GetGroupIndex(group);
				}
				else if (groupIndex >= 0 && group is null)
				{
					group = GetGroup(groupIndex);
				}
			}

			if (indexInGroup < 0 || (isGrouped && groupIndex < 0))
			{
				FindItem(item, isGrouped, out group, out groupIndex, out indexInGroup);
			}

			var itemIndex = isGrouped ? GetListViewGroupedIndex(groupIndex, indexInGroup) : indexInGroup;

			return new ItemTappedEventArgs(isGrouped ? group : null, item, itemIndex);
		}

		internal void SendItemTapped(object item, object group, int groupIndex, int indexInGroup) =>
			SendItemTapped(CreateItemTappedEventArgs(item, group, groupIndex, indexInGroup));

		/// <summary>
		/// Executes <see cref="ItemTappedCommand"/> and raises <see cref="ItemTapped"/> for a tap captured by
		/// <see cref="CreateItemTappedEventArgs(object, object, int, int)"/>.
		/// </summary>
		internal void SendItemTapped(ItemTappedEventArgs args)
		{
			if (args is null || !IsItemTapEnabled || !IsEnabled)
			{
				return;
			}

			var command = ItemTappedCommand;

			if (command is not null)
			{
				// The parameter can't be bound to the tapped item from outside the item template, so default to it
				var commandParameter = IsSet(ItemTappedCommandParameterProperty) ? ItemTappedCommandParameter : args.Item;

				if (command.CanExecute(commandParameter))
				{
					command.Execute(commandParameter);
				}
			}

			_itemTapped?.Invoke(this, args);
		}

		// Matches ListView: when grouped, each group contributes one row for its header followed by its items.
		int GetListViewGroupedIndex(int groupIndex, int indexInGroup)
		{
			if (groupIndex < 0 || indexInGroup < 0 || ItemsSource is not IEnumerable groups)
			{
				return -1;
			}

			var index = 0;
			var currentGroup = 0;

			foreach (var group in groups)
			{
				// Platform group sources skip entries that aren't groups
				if (!IsGroup(group))
				{
					continue;
				}

				if (currentGroup == groupIndex)
				{
					return index + 1 + indexInGroup;
				}

				index += 1 + Count(group);
				currentGroup++;
			}

			return -1;
		}

		void FindItem(object item, bool isGrouped, out object group, out int groupIndex, out int indexInGroup)
		{
			group = null;
			groupIndex = -1;
			indexInGroup = -1;

			if (ItemsSource is not IEnumerable source)
			{
				return;
			}

			if (!isGrouped)
			{
				indexInGroup = IndexOf(source, item);
				return;
			}

			var currentGroup = 0;

			foreach (var candidate in source)
			{
				if (!IsGroup(candidate))
				{
					continue;
				}

				if (candidate is IEnumerable groupItems)
				{
					var index = IndexOf(groupItems, item);

					if (index >= 0)
					{
						group = candidate;
						groupIndex = currentGroup;
						indexInGroup = index;
						return;
					}
				}

				currentGroup++;
			}
		}

		int GetGroupIndex(object group)
		{
			var index = 0;

			if (ItemsSource is IEnumerable groups)
			{
				foreach (var candidate in groups)
				{
					if (!IsGroup(candidate))
					{
						continue;
					}

					if (ReferenceEquals(candidate, group))
					{
						return index;
					}

					index++;
				}
			}

			return -1;
		}

		object GetGroup(int groupIndex)
		{
			var index = 0;

			if (ItemsSource is IEnumerable groups)
			{
				foreach (var candidate in groups)
				{
					if (!IsGroup(candidate))
					{
						continue;
					}

					if (index == groupIndex)
					{
						return candidate;
					}

					index++;
				}
			}

			return null;
		}

		static bool IsGroup(object candidate) => candidate is IEnumerable and not string;

		static int Count(object group)
		{
			if (group is ICollection collection)
			{
				return collection.Count;
			}

			var count = 0;

			if (group is IEnumerable enumerable)
			{
				foreach (var _ in enumerable)
				{
					count++;
				}
			}

			return count;
		}

		static int IndexOf(IEnumerable source, object item)
		{
			if (source is IList list)
			{
				return list.IndexOf(item);
			}

			var index = 0;

			foreach (var candidate in source)
			{
				if (Equals(candidate, item))
				{
					return index;
				}

				index++;
			}

			return -1;
		}

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
