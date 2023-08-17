using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
using ParentView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
using ParentView = Android.Views.IViewParent;
using Android.Content;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using ParentView = Microsoft.UI.Xaml.DependencyObject;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using ParentView = Tizen.NUI.BaseComponents.View;
#else
using PlatformView = System.Object;
using ParentView = System.Object;
#endif

namespace Microsoft.Maui.DeviceTests
{
	public class NestingView : View
	{
		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();

			if (Handler is NestingViewHandler sh)
			{
				foreach (var element in this.LogicalChildrenInternalBackingStore)
				{
					sh.PlatformView.AddChild(element.ToPlatform(Handler.MauiContext));
				}
			}
		}
	}

	public class NestingViewPlatformView :
#if WINDOWS
			UI.Xaml.Controls.StackPanel
#elif ANDROID
			AndroidX.AppCompat.Widget.LinearLayoutCompat
#else
			UIKit.UIView
#endif
	{

#if ANDROID
		public NestingViewPlatformView(Context context) : base(context)
		{
		}
#endif

		public NestingViewPlatformView AddChild()
		{
			var nextChild = new NestingViewPlatformView(
#if ANDROID
				Context
#endif
				);

			return (NestingViewPlatformView)AddChild(nextChild);
		}


		public PlatformView AddChild(IView view, NestingView rootNestingView)
		{
			var platformView = view.ToPlatform(rootNestingView.Handler.MauiContext);
			AddChild(platformView);
			rootNestingView.AddLogicalChild((Element)view);
			return platformView;
		}

		public T AddChild<T>(IView view, NestingView rootNestingView)
			where T : PlatformView
		{
			var platformView = view.ToPlatform(rootNestingView.Handler.MauiContext);
			AddChild(platformView);
			rootNestingView.AddLogicalChild((Element)view);
			return (T)platformView;
		}

		public PlatformView AddChild(PlatformView platformView)
		{
#if WINDOWS
			this.Children.Add(platformView);
#elif ANDROID
			this.AddView(platformView);
#else
			this.AddSubview(platformView);
#endif

			return platformView;
		}
	}

	public class NestingViewHandler : ViewHandler<NestingView, NestingViewPlatformView>
	{
		public static IPropertyMapper<NestingView, NestingViewHandler>
			Mapper = new PropertyMapper<NestingView, NestingViewHandler>();

		public NestingViewHandler()
			: base(Mapper)
		{
		}

		protected override NestingViewPlatformView CreatePlatformView()
		{
			return new NestingViewPlatformView(
#if ANDROID
				Context
#endif
				);
		}
	}
}
