using System;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Handlers;
using AActivity = Android.App.Activity;
using AViewGroup = Android.Views.ViewGroup;
using AView = Android.Views.View;
using AndroidX.AppCompat.Widget;
using System.Threading.Tasks;
using Android.Views;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.DeviceTests
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
			rootManager.Connect(VirtualView.Content);
		}
		public static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			handler.UpdateContent();
		}

		protected override void DisconnectHandler(AActivity nativeView)
		{
			base.DisconnectHandler(nativeView);
			var windowManager = MauiContext.GetNavigationRootManager();
			windowManager.Disconnect();
		}



		public WindowHandlerStub()
			: base(WindowMapper)
		{
		}

		protected override AActivity CreateNativeElement()
		{
			return MauiProgram.CurrentContext.GetActivity();
		}
	}
}
