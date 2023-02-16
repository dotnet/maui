using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using DeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		static Dictionary<Window, NavigationStack> s_modalStacks = new Dictionary<Window, NavigationStack>();
		static Dictionary<Window, Func<bool>> s_windowBackButtonPressedHandler = new Dictionary<Window, Func<bool>>();
		static Dictionary<Window, Action> s_windowCloseRequestHandler = new Dictionary<Window, Action>();

		public static void SetContent(this Window platformWindow, View content)
		{
			content.HeightSpecification = LayoutParamPolicies.MatchParent;
			content.WidthSpecification = LayoutParamPolicies.MatchParent;
			content.HeightResizePolicy = ResizePolicyType.FillToParent;
			content.WidthResizePolicy = ResizePolicyType.FillToParent;

			if (s_modalStacks.ContainsKey(platformWindow))
			{
				var modalStack = s_modalStacks[platformWindow];
				modalStack.Clear();
				modalStack.Push(content, true);
			}
		}

		public static void Initialize(this Window platformWindow)
		{
			platformWindow.AddAvailableOrientation(Window.WindowOrientation.Landscape);
			platformWindow.AddAvailableOrientation(Window.WindowOrientation.LandscapeInverse);
			platformWindow.AddAvailableOrientation(Window.WindowOrientation.Portrait);
			platformWindow.AddAvailableOrientation(Window.WindowOrientation.PortraitInverse);
			platformWindow.Resized += (s, e) => OnRotate(platformWindow);

			platformWindow.KeyEvent += (s, e) =>
			{
				if (e.Key.IsDeclineKeyEvent())
				{
					if (Popup.HasOpenedPopup)
					{
						if (Popup.CloseLast())
							return;
					}
					OnBackButtonPressed(platformWindow);
				}
			};

			var modalStack = s_modalStacks[platformWindow] = new NavigationStack
			{
				HeightSpecification = LayoutParamPolicies.MatchParent,
				WidthSpecification = LayoutParamPolicies.MatchParent,
				WidthResizePolicy = ResizePolicyType.FillToParent,
				HeightResizePolicy = ResizePolicyType.FillToParent,
			};
			platformWindow.GetDefaultLayer().Add(modalStack);
		}

		public static NavigationStack? GetModalStack(this Window platformWindow)
		{
			if (s_modalStacks.ContainsKey(platformWindow))
				return s_modalStacks[platformWindow];
			return null;
		}

		public static void SetWindowCloseRequestHandler(this Window platformWindow, Action handler)
		{
			s_windowCloseRequestHandler[platformWindow] = handler;
		}

		public static void SetBackButtonPressedHandler(this Window platformWindow, Func<bool> handler)
		{
			s_windowBackButtonPressedHandler[platformWindow] = handler;
		}

		public static float GetDisplayDensity(this Window platformWindow)
		{
			return (float)DeviceInfo.ScalingFactor;
		}

		internal static DisplayOrientation GetOrientation(this IWindow? window)
		{
			if (window == null)
				return DeviceDisplay.Current.MainDisplayInfo.Orientation;

			return window.Handler?.MauiContext?.GetPlatformWindow()?.GetCurrentOrientation() switch
			{
				Window.WindowOrientation.Portrait => DisplayOrientation.Portrait,
				Window.WindowOrientation.PortraitInverse => DisplayOrientation.Portrait,
				Window.WindowOrientation.Landscape => DisplayOrientation.Landscape,
				Window.WindowOrientation.LandscapeInverse => DisplayOrientation.Landscape,
				_ => DisplayOrientation.Unknown
			};
		}

		static void OnRotate(Window platformWindow)
		{
		}

		static void OnBackButtonPressed(Window platformWindow)
		{
			if (s_windowBackButtonPressedHandler.ContainsKey(platformWindow))
			{
				if (s_windowBackButtonPressedHandler[platformWindow].Invoke())
					return;
			}

			if (s_windowCloseRequestHandler.ContainsKey(platformWindow))
				s_windowCloseRequestHandler[platformWindow].Invoke();
		}

		internal static void UpdateX(this Window platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateY(this Window platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateWidth(this Window platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateHeight(this Window platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateUnsupportedCoordinate(this Window platformWindow, IWindow window) =>
			window.FrameChanged(platformWindow.WindowPositionSize.ToDP());
	}
}
