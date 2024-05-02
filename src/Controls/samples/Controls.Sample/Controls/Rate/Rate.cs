using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Controls
{
	public class Rate : TemplatedView
	{
		const string ElementPanel = "PART_Panel";

		StackLayout _rateLayout = default!;

		public static readonly BindableProperty IconProperty =
			BindableProperty.Create(nameof(Icon), typeof(Geometry), typeof(Rate), GetDefaultIcon());

		public Geometry Icon
		{
			get => (Geometry)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		static Geometry GetDefaultIcon()
		{
			Application.Current!.Resources.TryGetValue("StarGeometry", out object resource);
			PathGeometry pathGeometry = (PathGeometry)resource;

			return pathGeometry;
		}

		public static readonly BindableProperty ItemSizeProperty =
			BindableProperty.Create(nameof(ItemSize), typeof(double), typeof(Rate), 30.0d);

		public double ItemSize
		{
			get => (double)GetValue(ItemSizeProperty);
			set => SetValue(ItemSizeProperty, value);
		}

		public static readonly BindableProperty ItemCountProperty =
			BindableProperty.Create(nameof(ItemCount), typeof(int), typeof(Rate), 5);

		public int ItemCount
		{
			get => (int)GetValue(ItemCountProperty);
			set => SetValue(ItemCountProperty, value);
		}

		public static readonly BindableProperty SelectedFillProperty =
			BindableProperty.Create(nameof(SelectedFill), typeof(Color), typeof(Rate), Color.FromArgb("#F6C602"));

		public Color SelectedFill
		{
			get => (Color)GetValue(SelectedFillProperty);
			set => SetValue(SelectedFillProperty, value);
		}

		public static readonly BindableProperty UnSelectedFillProperty =
			BindableProperty.Create(nameof(UnSelectedFill), typeof(Color), typeof(Rate), Colors.Transparent);

		public Color UnSelectedFill
		{
			get => (Color)GetValue(UnSelectedFillProperty);
			set => SetValue(UnSelectedFillProperty, value);
		}

		public static readonly BindableProperty SelectedStrokeProperty =
			BindableProperty.Create(nameof(SelectedStroke), typeof(Color), typeof(Rate), Color.FromArgb("#F6C602"));

		public Color SelectedStroke
		{
			get => (Color)GetValue(SelectedStrokeProperty);
			set => SetValue(SelectedStrokeProperty, value);
		}

		public static readonly BindableProperty UnSelectedStrokeProperty =
			BindableProperty.Create(nameof(UnSelectedStroke), typeof(Color), typeof(Rate), Colors.Black);

		public Color UnSelectedStroke
		{
			get => (Color)GetValue(UnSelectedStrokeProperty);
			set => SetValue(UnSelectedStrokeProperty, value);
		}

		public static readonly BindableProperty SelectedStrokeWidthProperty =
			BindableProperty.Create(nameof(SelectedStrokeWidth), typeof(double), typeof(Rate), 1.0d);

		public double SelectedStrokeWidth
		{
			get => (double)GetValue(SelectedStrokeWidthProperty);
			set => SetValue(SelectedStrokeWidthProperty, value);
		}

		public static readonly BindableProperty UnSelectedStrokeWidthProperty =
			BindableProperty.Create(nameof(UnSelectedStrokeWidth), typeof(double), typeof(Rate), 1.0d);

		public double UnSelectedStrokeWidth
		{
			get => (double)GetValue(UnSelectedStrokeWidthProperty);
			set => SetValue(UnSelectedStrokeWidthProperty, value);
		}

		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(Rate), default(string));

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly BindableProperty ShowTextProperty =
			BindableProperty.Create(nameof(ShowText), typeof(bool), typeof(Rate), false);

		public bool ShowText
		{
			get => (bool)GetValue(ShowTextProperty);
			set => SetValue(ShowTextProperty, value);
		}

		public static readonly BindableProperty IsReadOnlyProperty =
			BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(Rate), false,
				propertyChanged: OnIsReadOnlyChanged);

		static void OnIsReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as Rate)?.UpdateIsReadOnly();
		}

		public bool IsReadOnly
		{
			get => (bool)GetValue(IsReadOnlyProperty);
			set => SetValue(IsReadOnlyProperty, value);
		}

		public static readonly BindableProperty ValueProperty =
			BindableProperty.Create(nameof(Value), typeof(int), typeof(Rate), default(int));

		public int Value
		{
			get => (int)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public event EventHandler<ValueChangedEventArgs>? ValueChanged;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_rateLayout = (GetTemplateChild(ElementPanel) as StackLayout)!;

			UpdateRateItems();
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == ValueProperty.PropertyName)
				UpdateValue();
			else if (propertyName == IconProperty.PropertyName)
				UpdateIcon();
			else if (propertyName == ItemCountProperty.PropertyName)
				UpdateItemCount();
			else if (propertyName == ItemSizeProperty.PropertyName)
				UpdateItemSize();
			else if (propertyName == SelectedFillProperty.PropertyName)
				UpdateSelectedFill();
			else if (propertyName == UnSelectedFillProperty.PropertyName)
				UpdateUnSelectedFill();
			else if (propertyName == SelectedStrokeProperty.PropertyName)
				UpdateSelectedStroke();
			else if (propertyName == UnSelectedStrokeProperty.PropertyName)
				UpdateUnSelectedStroke();
			else if (propertyName == SelectedStrokeWidthProperty.PropertyName)
				UpdateSelectedStrokeWidth();
			else if (propertyName == UnSelectedStrokeWidthProperty.PropertyName)
				UpdateUnSelectedStrokeWidth();
		}

		void UpdateRateItems()
		{
			_rateLayout.Children.Clear();

			for (var i = 1; i <= ItemCount; i++)
			{
				var rateItem = new RateItem();

				_rateLayout.Children.Add((IView)rateItem);
			}

			UpdateIsReadOnly();
			UpdateIcon();
			UpdateItemSize();
			UpdateSelectedFill();
			UpdateUnSelectedFill();
			UpdateSelectedStroke();
			UpdateUnSelectedStroke();
			UpdateSelectedStrokeWidth();
			UpdateUnSelectedStrokeWidth();
		}

		void UpdateIsReadOnly()
		{
			if (IsReadOnly)
			{
				foreach (var rateItem in _rateLayout.Children)
					(rateItem as View)?.GestureRecognizers.Clear();
			}
			else
			{
				foreach (var rateItem in _rateLayout.Children)
				{
					var tapGestureRecognizer = new TapGestureRecognizer();
					tapGestureRecognizer.Tapped += OnTapped;
					(rateItem as View)?.GestureRecognizers.Add(tapGestureRecognizer);
				}
			}
		}

		void UpdateIcon()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).Icon = Icon;
		}

		void UpdateItemCount()
		{
			UpdateRateItems();
		}

		void UpdateItemSize()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).ItemSize = ItemSize;
		}

		void UpdateSelectedFill()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).SelectedFill = SelectedFill;
		}

		void UpdateUnSelectedFill()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).UnSelectedFill = UnSelectedFill;
		}

		void UpdateSelectedStroke()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).SelectedFill = SelectedFill;
		}

		void UpdateUnSelectedStroke()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).UnSelectedFill = UnSelectedFill;
		}

		void UpdateSelectedStrokeWidth()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).SelectedStrokeWidth = SelectedStrokeWidth;
		}

		void UpdateUnSelectedStrokeWidth()
		{
			foreach (var child in _rateLayout.Children)
				((RateItem)child).UnSelectedStrokeWidth = UnSelectedStrokeWidth;
		}

		void UpdateValue()
		{
			for (int i = 0; i < ItemCount; i++)
				((RateItem)_rateLayout.Children[i]).IsSelected = i < Value;

			ValueChanged?.Invoke(this, new ValueChangedEventArgs(Value));
		}

		void OnTapped(object? sender, EventArgs e)
		{
			var star = (IView)sender!;
			var index = _rateLayout.Children.IndexOf(star);

			Value = index + 1;
		}
	}
}