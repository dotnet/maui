using EProgressBar = ElmSharp.ProgressBar;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, EProgressBar>
	{
		static readonly EColor s_defaultColor = new EColor(129, 198, 255);

		public ActivityIndicatorRenderer()
		{
			RegisterPropertyHandler(ActivityIndicator.ColorProperty, UpdateColor);
			RegisterPropertyHandler(ActivityIndicator.IsRunningProperty, UpdateIsRunning);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (Control == null)
			{
				SetNativeControl(new EProgressBar(Forms.NativeParent)
				{
					Style = "process_medium",
					IsPulseMode = true,
				});
			}
			base.OnElementChanged(e);
		}

		void UpdateColor(bool initialize)
		{
			if (initialize && Element.Color.IsDefault)
				return;

			Control.Color = (Element.Color == Color.Default) ? s_defaultColor : Element.Color.ToNative();
		}

		void UpdateIsRunning()
		{
			if (Element.IsRunning)
			{
				Control.PlayPulse();
			}
			else
			{
				Control.StopPulse();
			}
		}

	};
}
