using System.Maui.Core.Controls;

namespace System.Maui.Platform
{
	public partial class ActivityIndicatorRenderer : AbstractViewRenderer<IActivityIndicator, MauiProgressRing>
	{
		protected override MauiProgressRing CreateView() => new MauiProgressRing();

		public static void MapPropertyIsRunning(IViewRenderer renderer, IActivityIndicator activityIndicator) => (renderer as ActivityIndicatorRenderer)?.UpdateIsActive();
		public static void MapPropertyColor(IViewRenderer renderer, IActivityIndicator activityIndicator) => (renderer as ActivityIndicatorRenderer)?.UpdateColor();
		public virtual void UpdateColor()
		{
			TypedNativeView.UpdateDependencyColor(MauiProgressRing.ForegroundProperty, !VirtualView.Color.IsDefault ? VirtualView.Color : Color.Accent);
		}

		public virtual void UpdateIsActive()
		{
			TypedNativeView.IsActive = VirtualView.IsRunning;
		}
	}
}