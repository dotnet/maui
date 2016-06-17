using System;

namespace Xamarin.Forms
{
	public class EntryCell : Cell, IEntryCellController
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(EntryCell), null, BindingMode.TwoWay);

		public static readonly BindableProperty LabelProperty = BindableProperty.Create("Label", typeof(string), typeof(EntryCell), null);

		public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(EntryCell), null);

		public static readonly BindableProperty LabelColorProperty = BindableProperty.Create("LabelColor", typeof(Color), typeof(EntryCell), Color.Default);

		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create("Keyboard", typeof(Keyboard), typeof(EntryCell), Keyboard.Default);

		public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create("HorizontalTextAlignment", typeof(TextAlignment), typeof(EntryCell), TextAlignment.Start,
			propertyChanged: OnHorizontalTextAlignmentPropertyChanged);

		[Obsolete("XAlignProperty is obsolete. Please use HorizontalTextAlignmentProperty instead.")] public static readonly BindableProperty XAlignProperty = HorizontalTextAlignmentProperty;

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(HorizontalTextAlignmentProperty); }
			set { SetValue(HorizontalTextAlignmentProperty, value); }
		}

		public Keyboard Keyboard
		{
			get { return (Keyboard)GetValue(KeyboardProperty); }
			set { SetValue(KeyboardProperty, value); }
		}

		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public Color LabelColor
		{
			get { return (Color)GetValue(LabelColorProperty); }
			set { SetValue(LabelColorProperty, value); }
		}

		public string Placeholder
		{
			get { return (string)GetValue(PlaceholderProperty); }
			set { SetValue(PlaceholderProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		[Obsolete("XAlign is obsolete. Please use HorizontalTextAlignment instead.")]
		public TextAlignment XAlign
		{
			get { return (TextAlignment)GetValue(XAlignProperty); }
			set { SetValue(XAlignProperty, value); }
		}

		public event EventHandler Completed;

		void IEntryCellController.SendCompleted()
		{
			EventHandler handler = Completed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		static void OnHorizontalTextAlignmentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (EntryCell)bindable;
#pragma warning disable 0618 // retain until XAlign removed
			label.OnPropertyChanged(nameof(XAlign));
#pragma warning restore
		}
	}
}