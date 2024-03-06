#nullable disable
using System;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Controls.Platform
{
	public abstract class PlatformEffect : PlatformEffect<PlatformView, PlatformView>
	{
		internal override void SendAttached()
		{
			_ = Element ?? throw new InvalidOperationException("Element cannot be null here");
			Control = (PlatformView)Element.Handler.PlatformView;

			if (Element.Handler is IViewHandler vh)

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
After:
			{
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-ios)'
Before:
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
After:
			{
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
After:
			{
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
After:
			{
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
After:
			{
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
After:
			{
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			}
*/
			
/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				Container = Control;
After:
			{
				Container = Control;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-ios)'
Before:
				Container = Control;
After:
			{
				Container = Control;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				Container = Control;
After:
			{
				Container = Control;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				Container = Control;
After:
			{
				Container = Control;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				Container = Control;
After:
			{
				Container = Control;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				Container = Control;
After:
			{
				Container = Control;
			}
*/
{
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			}
			else
			{
				Container = Control;
			}

			base.SendAttached();
		}
	}
}