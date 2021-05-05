using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class SwitchCell : Cell
	{
		public static readonly BindableProperty OnProperty = BindableProperty.Create("On", typeof(bool), typeof(SwitchCell), false, propertyChanged: (obj, oldValue, newValue) =>
		{
			var switchCell = (SwitchCell)obj;
			switchCell.OnChanged?.Invoke(obj, new ToggledEventArgs((bool)newValue));
		}, defaultBindingMode: BindingMode.TwoWay);

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(SwitchCell), default(string));

		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(SwitchCell), null);

		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		public bool On
		{
			get { return (bool)GetValue(OnProperty); }
			set { SetValue(OnProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public event EventHandler<ToggledEventArgs> OnChanged;
	}
}