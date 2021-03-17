using System;

namespace Microsoft.Maui.Controls
{
	public partial class ActivityIndicator : View, IColorElement, IElementConfiguration<ActivityIndicator>
	{
		public static readonly BindableProperty IsRunningProperty = BindableProperty.Create("IsRunning", typeof(bool), typeof(ActivityIndicator), default(bool));

		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		readonly Lazy<PlatformConfigurationRegistry<ActivityIndicator>> _platformConfigurationRegistry;

		public ActivityIndicator()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ActivityIndicator>>(() => new PlatformConfigurationRegistry<ActivityIndicator>(this));
		}

		public Color Color
		{
			get { return (Color)GetValue(ColorElement.ColorProperty); }
			set { SetValue(ColorElement.ColorProperty, value); }
		}

		public bool IsRunning
		{
			get { return (bool)GetValue(IsRunningProperty); }
			set { SetValue(IsRunningProperty, value); }
		}
		public IPlatformElementConfiguration<T, ActivityIndicator> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}