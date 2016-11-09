using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_PickerRenderer))]
	public class Picker : View, IElementConfiguration<Picker>
	{
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(Picker), Color.Default);

		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Picker), default(string));

		public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Picker), -1, BindingMode.TwoWay,
			propertyChanged: OnSelectedIndexChanged,
			coerceValue: CoerceSelectedIndex);

		public static readonly BindableProperty SelectedValueMemberPathProperty = BindableProperty.Create(nameof(SelectedValueMemberPath), typeof(string), typeof(Picker));

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(Picker), default(IList), propertyChanged: OnItemsSourceChanged);

		public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(Picker), propertyChanged: OnDisplayMemberPathChanged);

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(Picker), null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

		readonly Lazy<PlatformConfigurationRegistry<Picker>> _platformConfigurationRegistry;

		public static readonly BindableProperty DisplayFuncProperty =
			BindableProperty.Create(
				nameof(DisplayFunc),
				typeof(Func<object, string>),
				typeof(Picker));

		public static readonly BindableProperty DisplayConverterProperty =
			BindableProperty.Create(
				nameof(DisplayConverter),
				typeof(IValueConverter),
				typeof(Picker),
				default(IValueConverter));

		
		public Picker()
		{
			((ObservableList<string>)Items).CollectionChanged += OnItemsCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Picker>>(() => new PlatformConfigurationRegistry<Picker>(this));
		}

		public string DisplayMemberPath
		{
			get { return (string)GetValue(DisplayMemberPathProperty); }
			set { SetValue(DisplayMemberPathProperty, value); }
		}

		public Func<object, string> DisplayFunc
		{
			get { return (Func<object, string>)GetValue(DisplayFuncProperty); }
			set { SetValue(DisplayFuncProperty, value); }
		}

		public IValueConverter DisplayConverter
		{
			get { return (IValueConverter)GetValue(DisplayConverterProperty); }
			set { SetValue(DisplayConverterProperty, value); }
		}

		public IList<string> Items { get; } = new ObservableList<string>();

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public string SelectedValueMemberPath
		{
			get { return (string)GetValue(SelectedValueMemberPathProperty); }
			set { SetValue(SelectedValueMemberPathProperty, value); }
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public event EventHandler SelectedIndexChanged;

		protected virtual string GetDisplayMember(object item)
		{
			if (DisplayConverter != null)
			{
				var display = DisplayConverter.Convert(item, typeof(string), null, CultureInfo.CurrentUICulture) as string;
				if (display == null)
				{
					throw new ArgumentException("value must be converted to string");
				}
				return display;
			}
			if (DisplayFunc != null)
			{
				return DisplayFunc(item);
			}
			if (item == null)
			{
				return null;
			}
			bool isValueType = item.GetType().GetTypeInfo().IsValueType;
			if (isValueType || string.IsNullOrEmpty(DisplayMemberPath) || item is string)
			{
				// For a mix of objects in ItemsSourc to be handled correctly in conjunction with DisplayMemberPath
				// we need to handle value types and string so that GetPropertyValue doesn't throw exception if the property
				// doesn't exist on the item object
				return item.ToString();
			}
			return GetPropertyValue(item, DisplayMemberPath) as string;
		}

		void AddItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.NewStartingIndex < 0 ? Items.Count : e.NewStartingIndex;
			foreach (object newItem in e.NewItems)
			{
				Items.Insert(index++, GetDisplayMember(newItem));
			}
		}

		void BindItems()
		{
			Items.Clear();
			foreach (object item in ItemsSource)
			{
				Items.Add(GetDisplayMember(item));
			}
			UpdateSelectedItem();
		}

		static object CoerceSelectedIndex(BindableObject bindable, object value)
		{
			var picker = (Picker)bindable;
			return picker.Items == null ? -1 : ((int)value).Clamp(-1, picker.Items.Count - 1);
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					// Clear Items collection and re-populate it with values from ItemsSource
					BindItems();
					return;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems(e);
					break;
				case NotifyCollectionChangedAction.Add:
					AddItems(e);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static object GetPropertyValue(object item, string memberPath)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}
			if (string.IsNullOrEmpty(memberPath))
			{
				throw new ArgumentNullException(nameof(memberPath));
			}
			// Find the property by walking the display member path to find any nested properties
			string[] propertyPathParts = memberPath.Split('.');
			object propertyValue = item;
			foreach (string propertyPathPart in propertyPathParts)
			{
				PropertyInfo propInfo = propertyValue.GetType().GetTypeInfo().GetDeclaredProperty(propertyPathPart);
				if (propInfo == null)
				{
					throw new ArgumentException(
						$"No property with name '{propertyPathPart}' was found on '{propertyValue.GetType().FullName}'");
				}
				propertyValue = propInfo.GetValue(propertyValue);
			}
			return propertyValue;
		}

		static void OnDisplayMemberPathChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			if (picker.ItemsSource?.Count > 0)
			{
				picker.BindItems();
			}
		}

		void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SelectedIndex = SelectedIndex.Clamp(-1, Items.Count - 1);
			UpdateSelectedItem();
		}

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			// Check if the ItemsSource value has changed and if so, unsubscribe from collection changed
			var observable = oldValue as INotifyCollectionChanged;
			if (observable != null)
			{
				observable.CollectionChanged -= picker.CollectionChanged;
			}
			observable = newValue as INotifyCollectionChanged;
			if (observable != null)
			{
				observable.CollectionChanged += picker.CollectionChanged;
				picker.BindItems();
			}
			else
			{
				// newValue is null so clear the items collection
				picker.Items.Clear();
			}
		}

		static void OnSelectedIndexChanged(object bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			EventHandler eh = picker.SelectedIndexChanged;
			eh?.Invoke(bindable, EventArgs.Empty);
			picker.UpdateSelectedItem();
		}

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = (Picker)bindable;
			picker.UpdateSelectedIndex(newValue);
		}

		void RemoveItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.OldStartingIndex < Items.Count ? e.OldStartingIndex : Items.Count;
			// TODO: How do we determine the order of which the items were removed
			foreach (object _ in e.OldItems)
			{
				Items.RemoveAt(index--);
			}
		}

		void UpdateSelectedIndex(object selectedItem)
		{
			string displayMember = GetDisplayMember(selectedItem);
			int index = Items.IndexOf(displayMember);
			// TODO Should we prevent call to FindObject since the object is already known
			// by setting a flag, or otherwise indicate, that we, internally, forced a SelectedIndex changed
			SelectedIndex = index;
		}

		void UpdateSelectedItem()
		{
			// coerceSelectedIndex ensures that SelectedIndex is in range [-1,ItemsSource.Count)
			SelectedItem = SelectedIndex == -1 ? null : ItemsSource?[SelectedIndex];
		}

		public IPlatformElementConfiguration<T, Picker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}
