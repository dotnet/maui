using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ActivityIndicatorRenderer))]
	public class ActivityIndicator : View
	{
		public static readonly BindableProperty IsRunningProperty = BindableProperty.Create("IsRunning", typeof(bool), typeof(ActivityIndicator), default(bool));

		public static readonly BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(ActivityIndicator), Color.Default);

		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		public bool IsRunning
		{
			get { return (bool)GetValue(IsRunningProperty); }
			set { SetValue(IsRunningProperty, value); }
		}
	}
}