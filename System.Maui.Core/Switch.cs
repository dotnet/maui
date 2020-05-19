using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_SwitchRenderer))]
	public class Switch : View, IElementConfiguration<Switch>
	{
		public const string SwitchOnVisualState = "On";
		public const string SwitchOffVisualState = "Off";

		public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(Switch), false, propertyChanged: (bindable, oldValue, newValue) =>
		{
			((Switch)bindable).Toggled?.Invoke(bindable, new ToggledEventArgs((bool)newValue));
			((Switch)bindable).ChangeVisualState();
		}, defaultBindingMode: BindingMode.TwoWay);

		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(Switch), Color.Default);

		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Switch), Color.Default);

		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
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
		protected internal override void ChangeVisualState()
		{
			base.ChangeVisualState();
			if (IsEnabled && IsToggled)
				VisualStateManager.GoToState(this, SwitchOnVisualState);
			else if (IsEnabled && !IsToggled)
				VisualStateManager.GoToState(this, SwitchOffVisualState);
		}

		public event EventHandler<ToggledEventArgs> Toggled;

		public IPlatformElementConfiguration<T, Switch> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}