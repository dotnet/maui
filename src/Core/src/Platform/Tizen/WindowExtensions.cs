using System;
using System.Collections.Generic;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui
{
	public static class WindowExtensions
	{
		static Dictionary<Window, NavigationStack> s_modalStacks = new Dictionary<Window, NavigationStack>();
		static Dictionary<Window, Func<bool>> s_windowBackButtonPressedHandler = new Dictionary<Window, Func<bool>>();
		static Dictionary<Window, Action> s_windowCloseRequestHandler = new Dictionary<Window, Action>();

		public static void SetContent(this Window nativeWindow, View content)
		{
			content.HeightSpecification = LayoutParamPolicies.MatchParent;
			content.WidthSpecification = LayoutParamPolicies.MatchParent;
			content.HeightResizePolicy = ResizePolicyType.FillToParent;
			content.WidthResizePolicy = ResizePolicyType.FillToParent;

			if (s_modalStacks.ContainsKey(nativeWindow))
			{
				var modalStack = s_modalStacks[nativeWindow];
				modalStack.Clear();
				modalStack.Push(content, true);
			}
		}

		public static void Initialize(this Window nativeWindow)
		{
			nativeWindow.AddAvailableOrientation(Window.WindowOrientation.Landscape);
			nativeWindow.AddAvailableOrientation(Window.WindowOrientation.LandscapeInverse);
			nativeWindow.AddAvailableOrientation(Window.WindowOrientation.Portrait);
			nativeWindow.AddAvailableOrientation(Window.WindowOrientation.PortraitInverse);
			nativeWindow.Resized += (s, e) => OnRotate(nativeWindow);

			nativeWindow.KeyEvent += (s, e) =>
			{
				if (e.Key.State == Key.StateType.Down && (e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape"))
				{
					if (Popup.HasOpenedPopup)
					{
						Popup.CloseLast();
						return;
					}
					OnBackButtonPressed(nativeWindow);
				}
			};

			var modalStack = s_modalStacks[nativeWindow] = new NavigationStack
			{
				HeightSpecification = LayoutParamPolicies.MatchParent,
				WidthSpecification = LayoutParamPolicies.MatchParent,
				WidthResizePolicy = ResizePolicyType.FillToParent,
				HeightResizePolicy = ResizePolicyType.FillToParent,
				PushAnimation = (v, p) => v.Opacity = 0.5f + 0.5f * (float)p,
				PopAnimation = (v, p) => v.Opacity = 0.5f + 0.5f * (float)(1 - p)
			};
			nativeWindow.GetDefaultLayer().Add(modalStack);
		}

		public static NavigationStack? GetModalStack(this Window window)
		{
			if (s_modalStacks.ContainsKey(window))
				return s_modalStacks[window];
			return null;
		}

		public static void SetWindowCloseRequestHandler(this Window window, Action handler)
		{
			s_windowCloseRequestHandler[window] = handler;
		}

		public static void SetBackButtonPressedHandler(this Window window, Func<bool> handler)
		{
			s_windowBackButtonPressedHandler[window] = handler;
		}

		static void OnRotate(Window window)
		{
		}

		static void OnBackButtonPressed(Window window)
		{
			if (s_windowBackButtonPressedHandler.ContainsKey(window))
			{
				if (s_windowBackButtonPressedHandler[window].Invoke())
					return;
			}

			if (s_windowCloseRequestHandler.ContainsKey(window))
				s_windowCloseRequestHandler[window].Invoke();
		}

	}
}
