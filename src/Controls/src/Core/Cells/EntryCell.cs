using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.EntryCell']/Docs" />
	public class EntryCell : Cell, ITextAlignmentElement, IEntryCellController, ITextAlignment
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='TextProperty']/Docs" />
		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(EntryCell), null, BindingMode.TwoWay);

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='LabelProperty']/Docs" />
		public static readonly BindableProperty LabelProperty = BindableProperty.Create("Label", typeof(string), typeof(EntryCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='PlaceholderProperty']/Docs" />
		public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(EntryCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='LabelColorProperty']/Docs" />
		public static readonly BindableProperty LabelColorProperty = BindableProperty.Create("LabelColor", typeof(Color), typeof(EntryCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='KeyboardProperty']/Docs" />
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create("Keyboard", typeof(Keyboard), typeof(EntryCell), Keyboard.Default);

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='HorizontalTextAlignmentProperty']/Docs" />
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='VerticalTextAlignmentProperty']/Docs" />
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='Keyboard']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(Converters.KeyboardTypeConverter))]
		public Keyboard Keyboard
		{
			get { return (Keyboard)GetValue(KeyboardProperty); }
			set { SetValue(KeyboardProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='Label']/Docs" />
		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='LabelColor']/Docs" />
		public Color LabelColor
		{
			get { return (Color)GetValue(LabelColorProperty); }
			set { SetValue(LabelColorProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='Placeholder']/Docs" />
		public string Placeholder
		{
			get { return (string)GetValue(PlaceholderProperty); }
			set { SetValue(PlaceholderProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public event EventHandler Completed;

		/// <include file="../../../docs/Microsoft.Maui.Controls/EntryCell.xml" path="//Member[@MemberName='SendCompleted']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCompleted()
			=> Completed?.Invoke(this, EventArgs.Empty);

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}
	}
}