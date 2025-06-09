#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.View"/> control for picking an element in a list.</summary>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class Picker : View, IFontElement, ITextElement, ITextAlignmentElement, IElementConfiguration<Picker>, IPicker
	{
		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(Picker), default(string));

		/// <summary>Bindable property for <see cref="TitleColor"/>.</summary>
		public static readonly BindableProperty TitleColorProperty =
			BindableProperty.Create(nameof(TitleColor), typeof(Color), typeof(Picker), default(Color));

		/// <summary>Bindable property for <see cref="SelectedIndex"/>.</summary>
		public static readonly BindableProperty SelectedIndexProperty =
			BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Picker), -1, BindingMode.TwoWay,
									propertyChanged: OnSelectedIndexChanged, coerceValue: CoerceSelectedIndex);

		/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(Picker), default(IList),
									propertyChanged: OnItemsSourceChanged);

		/// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(Picker), null, BindingMode.TwoWay,
									propertyChanged: OnSelectedItemChanged);

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>Bindable property for <see cref="HorizontalTextAlignment"/>.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="VerticalTextAlignment"/>.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
		public static readonly BindableProperty IsOpenProperty =
			BindableProperty.Create(nameof(IPicker.IsOpen), typeof(bool), typeof(Picker), default, BindingMode.TwoWay,
				propertyChanged: OnIsOpenPropertyChanged);

		readonly Lazy<PlatformConfigurationRegistry<Picker>> _platformConfigurationRegistry;

		/// <summary>Initializes a new instance of the Picker class.</summary>
		public Picker()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += OnItemsCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Picker>>(() => new PlatformConfigurationRegistry<Picker>(this));
		}
		/// <summary>Gets a value that indicates whether the font for the searchbar text is bold, italic, or neither. This is a bindable property.</summary>
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <summary>Gets or sets the font family for the picker text. This is a bindable property.</summary>
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <summary>Gets or sets the size of the font for the text in the picker.</summary>
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		TextTransform ITextElement.TextTransform
		{
			get => TextTransform.Default;
			set { }
		}

		/// <param name="source">The source parameter.</param>
		/// <param name="textTransform">The textTransform parameter.</param>
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilities.GetTransformedText(source, textTransform);

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		/// <summary>Gets the list of choices.</summary>
		/// <remarks>This property is read-only, but exposes the IList interface, so items can be added using Add().</remarks>
		public IList<string> Items { get; } = new LockableObservableListWrapper();

		/// <summary>Gets or sets the source list of items to template and display. This is a bindable property.</summary>
		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		/// <summary>Gets or sets the index of the selected item of the picker. This is a bindable property.</summary>
		/// <remarks>A value of -1 represents no item selected.</remarks>
		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		/// <summary>Gets or sets the selected item. This is a bindable property.</summary>
		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		/// <summary>Gets or sets the text color. This is a bindable property.</summary>
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <summary>Gets or sets the title for the Picker. This is a bindable property.</summary>
		/// <remarks>Depending on the platform, the Title is shown as a placeholder, headline, or not showed at all.</remarks>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='TitleColor']/Docs/*" />
		public Color TitleColor
		{
			get { return (Color)GetValue(TitleColorProperty); }
			set { SetValue(TitleColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs/*" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Picker.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs/*" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		BindingBase _itemDisplayBinding;
		/// <summary>Gets or sets a binding that selects the property that will be displayed for each object in the list of items.</summary>
		[DoesNotInheritDataType]
		public BindingBase ItemDisplayBinding
		{
			get { return _itemDisplayBinding; }
			set
			{
				if (_itemDisplayBinding == value)
					return;

				OnPropertyChanging();
				var oldValue = value;
				_itemDisplayBinding = value;
				OnItemDisplayBindingChanged(oldValue, _itemDisplayBinding);
				OnPropertyChanged();
			}
		}

		public bool IsOpen
		{
			get => (bool)GetValue(IsOpenProperty);
			set => SetValue(IsOpenProperty, value);
		}

		static void OnIsOpenPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Picker)bindable).OnIsOpenPropertyChanged((bool)oldValue, (bool)newValue);
		}

		public event EventHandler SelectedIndexChanged;
		public event EventHandler<PickerOpenedEventArgs> Opened;
		public event EventHandler<PickerClosedEventArgs> Closed;

		static readonly BindableProperty s_displayProperty =
			BindableProperty.Create("Display", typeof(string), typeof(Picker), default(string));

		string GetDisplayMember(object item)
		{
			if (ItemDisplayBinding == null)
				return item == null ? string.Empty : item.ToString();

			ItemDisplayBinding.Apply(item, this, s_displayProperty, false, SetterSpecificity.FromBinding);
			ItemDisplayBinding.Unapply();
			return (string)GetValue(s_displayProperty);
		}

		static object CoerceSelectedIndex(BindableObject bindable, object value)
		{
			var picker = (Picker)bindable;
			return picker.Items == null ? -1 : ((int)value).Clamp(-1, picker.Items.Count - 1);
		}

		void OnItemDisplayBindingChanged(BindingBase oldValue, BindingBase newValue)
		{
			ResetItems();
		}

		void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Do not execute when Items is locked because updates to ItemsSource will
			// take care of it
			if (((LockableObservableListWrapper)Items).IsLocked)
				return;

			int index = GetSelectedIndex();
			ClampSelectedIndex(index);
			Handler?.UpdateValue(nameof(IPicker.Items));
		}

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Picker)bindable).OnItemsSourceChanged((IList)oldValue, (IList)newValue);
		}

		void OnItemsSourceChanged(IList oldValue, IList newValue)
		{
			// Unsubscribe from old items
			if (oldValue != null)
			{
				foreach (var item in oldValue)
				{
					if (item is INotifyPropertyChanged npc)
					{
						npc.PropertyChanged -= OnPickerItemPropertyChanged;
					}
				}
			}
			// Subscribe to new items
			if (newValue != null)
			{
				foreach (var item in newValue)
				{
					if (item is INotifyPropertyChanged npc)
					{
						npc.PropertyChanged += OnPickerItemPropertyChanged;
					}
				}
			}

			var oldObservable = oldValue as INotifyCollectionChanged;
			if (oldObservable != null)
				oldObservable.CollectionChanged -= CollectionChanged;

			var newObservable = newValue as INotifyCollectionChanged;
			if (newObservable != null)
			{
				newObservable.CollectionChanged += CollectionChanged;
			}

			if (newValue != null)
			{
				((LockableObservableListWrapper)Items).IsLocked = true;
				ResetItems();
			}
			else
			{
				// Unlock, then clear, so OnItemsCollectionChanged executes
				((LockableObservableListWrapper)Items).IsLocked = false;
				((LockableObservableListWrapper)Items).InternalClear();
			}
		}

		readonly Queue<Action> _pendingIsOpenActions = new Queue<Action>();

		void OnIsOpenPropertyChanged(bool oldValue, bool newValue)
		{
			if (Handler?.VirtualView is Picker)
			{
				HandleIsOpenChanged();
			}
			else
			{
				_pendingIsOpenActions.Enqueue(HandleIsOpenChanged);
			}
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();

			// Process any pending actions when handler becomes available
			while (_pendingIsOpenActions.Count > 0 && Handler != null)
			{
				var action = _pendingIsOpenActions.Dequeue();
				action.Invoke();
			}
		}

		void HandleIsOpenChanged()
		{
			if (Handler?.VirtualView is not Picker picker)
				return;

			if (picker.IsOpen)
				picker.Opened?.Invoke(picker, PickerOpenedEventArgs.Empty);
			else
				picker.Closed?.Invoke(picker, PickerClosedEventArgs.Empty);
		}
		void OnPickerItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ItemDisplayBinding is Binding binding && !string.IsNullOrEmpty(binding.Path))
			{
				if (e.PropertyName == binding.Path)
				{
					ResetItems();
				}
			}
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Unsubscribe from removed items
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					if (item is INotifyPropertyChanged npc)
					{
						npc.PropertyChanged -= OnPickerItemPropertyChanged;
					}
				}
			}
			// Subscribe to added items
			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					if (item is INotifyPropertyChanged npc)
					{
						npc.PropertyChanged += OnPickerItemPropertyChanged;
					}
				}
			}
			
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(e);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems(e);
					break;
				default: //Move, Replace, Reset
					ResetItems();
					break;
			}

			Handler?.UpdateValue(nameof(IPicker.Items));
		}

		void AddItems(NotifyCollectionChangedEventArgs e)
		{
			int insertIndex = e.NewStartingIndex < 0 ? Items.Count : e.NewStartingIndex;
			int index = insertIndex;
			foreach (object newItem in e.NewItems)
				((LockableObservableListWrapper)Items).InternalInsert(index++, GetDisplayMember(newItem));

			index = GetSelectedIndex();
			if (insertIndex <= index)
			{
				// When an item is inserted before the current selection, the selected item changes because the selected index is not properly updated.
				ClampSelectedIndex(index);
			}
		}

		void RemoveItems(NotifyCollectionChangedEventArgs e)
		{
			int removeStart;
			// Items are removed in reverse order, so index starts at the index of the last item to remove
			int index;

			if (e.OldStartingIndex < Items.Count)
			{
				// Remove e.OldItems.Count items starting at e.OldStartingIndex
				removeStart = e.OldStartingIndex;
				index = e.OldStartingIndex + e.OldItems.Count - 1;
			}
			else
			{
				// Remove e.OldItems.Count items at the end when e.OldStartingIndex is past the end of the Items collection
				removeStart = Items.Count - e.OldItems.Count;
				index = Items.Count - 1;
			}

			foreach (object _ in e.OldItems)
				((LockableObservableListWrapper)Items).InternalRemoveAt(index--);

			index = GetSelectedIndex();
			if (removeStart <= index)
			{
				ClampSelectedIndex(index);
			}
		}

		int GetSelectedIndex()
		{
			if (SelectedItem is null)
			{
				return SelectedIndex;
			}

			int newIndex = ItemsSource?.IndexOf(SelectedItem) ?? Items?.IndexOf(SelectedItem) ?? -1;
			return newIndex >= 0 ? newIndex : SelectedIndex;
		}

		void ResetItems()
		{
			if (ItemsSource == null)
				return;
			((LockableObservableListWrapper)Items).InternalClear();
			foreach (object item in ItemsSource)
				((LockableObservableListWrapper)Items).InternalAdd(GetDisplayMember(item));
			Handler?.UpdateValue(nameof(IPicker.Items));

			ClampSelectedIndex(SelectedIndex);
		}

		static void OnSelectedIndexChanged(object bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			picker.UpdateSelectedItem(picker.SelectedIndex);
			picker.SelectedIndexChanged?.Invoke(bindable, EventArgs.Empty);
		}

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			picker.UpdateSelectedIndex(newValue);
		}

		void ClampSelectedIndex(int selectedIndex)
		{
			var oldIndex = selectedIndex;
			var newIndex = selectedIndex.Clamp(-1, Items.Count - 1);
			//FIXME use the specificity of the caller
			SetValue(SelectedIndexProperty, newIndex, SetterSpecificity.FromHandler);
			// If the index has not changed, still need to change the selected item
			if (newIndex == oldIndex)
				UpdateSelectedItem(newIndex);
		}

		void UpdateSelectedIndex(object selectedItem)
		{
			//FIXME use the specificity of the caller

			if (ItemsSource != null)
			{
				SetValue(SelectedIndexProperty, ItemsSource.IndexOf(selectedItem), SetterSpecificity.FromHandler);
				return;
			}
			SetValue(SelectedIndexProperty, Items.IndexOf(selectedItem), SetterSpecificity.FromHandler);
		}

		void UpdateSelectedItem(int index)
		{
			//FIXME use the specificity of the caller

			if (index == -1)
			{
				SetValue(SelectedItemProperty, null, SetterSpecificity.FromHandler);
				return;
			}

			if (ItemsSource != null)
			{
				var item = index < ItemsSource.Count ? ItemsSource[index] : null;
				SetValue(SelectedItemProperty, item, SetterSpecificity.FromHandler);
				return;
			}

			SetValue(SelectedItemProperty, Items[index], SetterSpecificity.FromHandler);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Picker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{
			InvalidateMeasure();
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{

		}

		Font ITextStyle.Font => this.ToFont();

		IList<string> IPicker.Items => Items;

		int IPicker.SelectedIndex
		{
			get => SelectedIndex;
			set => SetValue(SelectedIndexProperty, value, SetterSpecificity.FromHandler);
		}

		int IItemDelegate<string>.GetCount() => Items?.Count ?? ItemsSource?.Count ?? 0;

		string IItemDelegate<string>.GetItem(int index)
		{
			if (index < 0)
				return string.Empty;
			if (index < Items?.Count)
				return GetItem(index);
			if (index < ItemsSource?.Count)
				return GetDisplayMember(ItemsSource[index]);
			return string.Empty;
		}

		string GetItem(int index)
		{
			if (index < Items?.Count)
			{
				var item = Items[index];
				return item ?? string.Empty;
			}

			return string.Empty;
		}

		private protected override string GetDebuggerDisplay()
		{
			var selectedItemText = DebuggerDisplayHelpers.GetDebugText(nameof(SelectedItem), SelectedItem);
			return $"{base.GetDebuggerDisplay()}, Items = {ItemsSource?.Count ?? 0}, {selectedItemText}";
		}
	}
}
