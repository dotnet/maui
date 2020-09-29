using System;
using System.ComponentModel;

namespace Xamarin.Forms
{
	public class EntryCell : Cell, ITextAlignmentElement, IEntryCellController
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(EntryCell), null, BindingMode.TwoWay);

		public static readonly BindableProperty LabelProperty = BindableProperty.Create("Label", typeof(string), typeof(EntryCell), null);

		public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(EntryCell), null);

		public static readonly BindableProperty LabelColorProperty = BindableProperty.Create("LabelColor", typeof(Color), typeof(EntryCell), Color.Default);

		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create("Keyboard", typeof(Keyboard), typeof(EntryCell), Keyboard.Default);

		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		[Obsolete("XAlignProperty is obsolete as of version 2.0.0. Please use HorizontalTextAlignmentProperty instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty XAlignProperty = HorizontalTextAlignmentProperty;

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
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

		[Obsolete("XAlign is obsolete as of version 2.0.0. Please use HorizontalTextAlignment instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TextAlignment XAlign
		{
			get { return (TextAlignment)GetValue(XAlignProperty); }
			set { SetValue(XAlignProperty, value); }
		}

		public event EventHandler Completed;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCompleted()
			=> Completed?.Invoke(this, EventArgs.Empty);

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
#pragma warning disable 0618 // retain until XAlign removed
			OnPropertyChanged(nameof(XAlign));
#pragma warning restore
		}
	}
}