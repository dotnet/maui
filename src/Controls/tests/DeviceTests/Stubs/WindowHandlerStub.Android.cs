using System;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using static Microsoft.Maui.DeviceTests.HandlerTestBase;
using AActivity = Android.App.Activity;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowHandlerStub : ElementHandler<IWindow, AActivity>, IWindowHandler
	{
		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		void UpdateContent()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			
			var rootManager = MauiContext.GetNavigationRootManager();

			var previousRootView = rootManager.RootView;
			rootManager.Connect(VirtualView.Content);

			// This is used for cases where we are testing swapping out the page set on window
			if (previousRootView?.Parent is FakeActivityRootView farw)
			{
				previousRootView.RemoveFromParent();
				rootManager.RootView.LayoutParameters = new LinearLayoutCompat.LayoutParams(500, 500);
				farw.AddView(rootManager.RootView);
			}
		}

		public static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			handler.UpdateContent();
		}

		protected override void DisconnectHandler(AActivity platformView)
		{
			base.DisconnectHandler(platformView);
			var windowManager = MauiContext.GetNavigationRootManager();
			windowManager.Disconnect();
		}

		public WindowHandlerStub()
			: base(WindowMapper)
		{
		}

		protected override AActivity CreatePlatformElement()
		{
			return MauiProgram.CurrentContext.GetActivity();
		}
	}
}
