using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_SwitchRenderer))]
	public class Switch : View
	{
		public static readonly BindableProperty IsToggledProperty = BindableProperty.Create("IsToggled", typeof(bool), typeof(Switch), false, propertyChanged: (bindable, oldValue, newValue) =>
		{
			EventHandler<ToggledEventArgs> eh = ((Switch)bindable).Toggled;
			if (eh != null)
				eh(bindable, new ToggledEventArgs((bool)newValue));
		}, defaultBindingMode: BindingMode.TwoWay);

		public bool IsToggled
		{
			get { return (bool)GetValue(IsToggledProperty); }
			set { SetValue(IsToggledProperty, value); }
		}

		public event EventHandler<ToggledEventArgs> Toggled;
	}
}