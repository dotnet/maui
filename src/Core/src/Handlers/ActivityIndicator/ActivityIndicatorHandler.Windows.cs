#nullable enable

using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ProgressRing>
	{
		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		protected override ProgressRing CreatePlatformView() => new ProgressRing
		{
			IsIndeterminate = true
		};

		public static void MapBackground(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.ToPlatform().UpdateBackground(activityIndicator);
		}

		public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}

		public static void MapWidth(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			if (handler.PlatformView is ProgressRing platformView)
			{
				platformView.UpdateWidth(activityIndicator);
			}
		}

		public static void MapHeight(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			if (handler.PlatformView is ProgressRing platformView)
			{
				platformView.UpdateHeight(activityIndicator);
			}
		}
	}
}