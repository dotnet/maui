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

		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(Slider), Color.Default);

		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}


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