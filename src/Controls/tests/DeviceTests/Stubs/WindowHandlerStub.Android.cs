using System;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using static Microsoft.Maui.DeviceTests.ControlsHandlerTestBase;
using AActivity = Android.App.Activity;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowHandlerStub : ElementHandler<IWindow, AActivity>, IWindowHandler
	{
		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		public AView PlatformViewUnderTest { get; private set; }

		void UpdateContent()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			ConnectContent(VirtualView.Content);
		}

		internal void ConnectContent(IView content)
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = MauiContext.GetNavigationRootManager();
			rootManager.Connect(
				content,
				rootPrepared: rootView => OnRootPrepared(rootManager, rootView));
		}

		void OnRootPrepared(
			NavigationRootManager rootManager,
			AView platformView)
		{
			if (platformView is null)
				return;

			// This is used for cases where we are testing swapping out the page set on window
			if (PlatformViewUnderTest?.Parent is FakeActivityRootView farw)
			{
				PlatformViewUnderTest.RemoveFromParent();

				farw.AddView(platformView, 0);
#pragma warning disable XAOBS001 // Obsolete
				platformView.LayoutParameters = new FitWindowsFrameLayout.LayoutParams(AViewGroup.LayoutParams.MatchParent, AViewGroup.LayoutParams.MatchParent);
#pragma warning restore XAOBS001 // Obsolete
			}

			PlatformViewUnderTest = platformView;
		}

		public static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			handler.UpdateContent();
		}

		protected override void DisconnectHandler(AActivity platformView)
		{
			base.DisconnectHandler(platformView);
			WindowHandler.DisconnectHandler(MauiContext.GetNavigationRootManager());
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
