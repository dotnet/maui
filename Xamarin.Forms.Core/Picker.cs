using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_PickerRenderer))]
	public class Picker : View, IElementConfiguration<Picker>
	{
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(Picker), Color.Default);

		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Picker), default(string));

		public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Picker), -1, BindingMode.TwoWay,
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				EventHandler eh = ((Picker)bindable).SelectedIndexChanged;
				if (eh != null)
					eh(bindable, EventArgs.Empty);
			}, coerceValue: CoerceSelectedIndex);

		readonly Lazy<PlatformConfigurationRegistry<Picker>> _platformConfigurationRegistry;

		public Picker()
		{
			Items = new ObservableList<string>();
			((ObservableList<string>)Items).CollectionChanged += OnItemsCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Picker>>(() => new PlatformConfigurationRegistry<Picker>(this));
		}

		public IList<string> Items { get; }

		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
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

		static object CoerceSelectedIndex(BindableObject bindable, object value)
		{
			var picker = (Picker)bindable;
			return picker.Items == null ? -1 : ((int)value).Clamp(-1, picker.Items.Count - 1);
		}

		void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SelectedIndex = SelectedIndex.Clamp(-1, Items.Count - 1);
		}

		public IPlatformElementConfiguration<T, Picker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}