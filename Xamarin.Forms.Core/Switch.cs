using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_SwitchRenderer))]
	public class Switch : View, IElementConfiguration<Switch>
	{
		public static readonly BindableProperty IsToggledProperty = BindableProperty.Create("IsToggled", typeof(bool), typeof(Switch), false, propertyChanged: (bindable, oldValue, newValue) =>
		{
			EventHandler<ToggledEventArgs> eh = ((Switch)bindable).Toggled;
			if (eh != null)
				eh(bindable, new ToggledEventArgs((bool)newValue));
		}, defaultBindingMode: BindingMode.TwoWay);

		readonly Lazy<PlatformConfigurationRegistry<Switch>> _platformConfigurationRegistry;

		public Switch()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Switch>>(() => new PlatformConfigurationRegistry<Switch>(this));
		}

		public bool IsToggled
		{
			get { return (bool)GetValue(IsToggledProperty); }
			set { SetValue(IsToggledProperty, value); }
		}

		public event EventHandler<ToggledEventArgs> Toggled;

		public IPlatformElementConfiguration<T, Switch> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}