using System;

namespace Xamarin.Forms
{
	public class SwitchCell : Cell
	{
		public static readonly BindableProperty OnProperty = BindableProperty.Create("On", typeof(bool), typeof(SwitchCell), false, propertyChanged: (obj, oldValue, newValue) =>
		{
			var switchCell = (SwitchCell)obj;
			EventHandler<ToggledEventArgs> handler = switchCell.OnChanged;
			if (handler != null)
				handler(obj, new ToggledEventArgs((bool)newValue));
		}, defaultBindingMode: BindingMode.TwoWay);

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(SwitchCell), default(string));

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