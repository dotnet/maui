#nullable disable
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml.Controls;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.RefreshView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class RefreshViewExtensions
	{
		public static void UpdateRefreshPullDirection(this RefreshContainer refreshContainer, RefreshView refreshView)
		{
			var refreshPullDirection = refreshView.OnThisPlatform().GetRefreshPullDirection();

			switch (refreshPullDirection)
			{
				case Specifics.RefreshPullDirection.TopToBottom:
					refreshContainer.PullDirection = RefreshPullDirection.TopToBottom;
					break;
				case Specifics.RefreshPullDirection.BottomToTop:
					refreshContainer.PullDirection = RefreshPullDirection.BottomToTop;
					break;
				case Specifics.RefreshPullDirection.LeftToRight:
					refreshContainer.PullDirection = RefreshPullDirection.LeftToRight;
					break;
				case Specifics.RefreshPullDirection.RightToLeft:
					refreshContainer.PullDirection = RefreshPullDirection.RightToLeft;
					break;
				default:
					goto case Specifics.RefreshPullDirection.TopToBottom;
			}
		}
	}
}