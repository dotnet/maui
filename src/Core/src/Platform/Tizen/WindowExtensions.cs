using System;
using System.Collections.Generic;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui.Platform
{
	public static class WindowExtensions
	{
		static Dictionary<Window, Func<bool>> s_windowBackButtonPressedHandler = new Dictionary<Window, Func<bool>>();
		static Dictionary<Window, Action> s_windowCloseRequestHandler = new Dictionary<Window, Action>();
		static Dictionary<Window, ELayout> s_windowBaseLayout = new Dictionary<Window, ELayout>();
		static Dictionary<Window, ModalStack> s_windowModalStack = new Dictionary<Window, ModalStack>();

		public static void Initialize(this Window nativeWindow)
		{
			var baseLayout = (ELayout?)nativeWindow.GetType().GetProperty("BaseLayout")?.GetValue(nativeWindow);

			if (baseLayout == null)
			{
				var conformant = new Conformant(nativeWindow);
				conformant.Show();

				var layout = new ApplicationLayout(conformant);
				layout.Show();

				baseLayout = layout;
				conformant.SetContent(baseLayout);
			}
			nativeWindow.SetBaseLayout(baseLayout);
			var modalStack = new ModalStack(baseLayout)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			modalStack.Show();
			baseLayout.SetContent(modalStack);
			nativeWindow.SetModalStack(modalStack);

			nativeWindow.Active();
			nativeWindow.Show();
			nativeWindow.AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_90 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270;
			
			nativeWindow.RotationChanged += (sender, e) =>
			{
				// TODO : should update later
			};

			nativeWindow.BackButtonPressed += (s, e) => OnBackButtonPressed(nativeWindow);
		}

		public static void SetOverlay(this Window window, EvasObject content)
		{
			content?.SetAlignment(-1, -1);
			content?.SetWeight(1, 1);
			content?.Show();
			window.AddResizeObject(content);
		}

		public static void SetWindowCloseRequestHandler(this Window window, Action handler)
		{
			s_windowCloseRequestHandler[window] = handler;
		}

		public static void SetBackButtonPressedHandler(this Window window, Func<bool> handler)
		{
			s_windowBackButtonPressedHandler[window] = handler;
		}

		public static ELayout GetBaseLayout(this Window window)
		{
			return s_windowBaseLayout[window];
		}

		public static void SetBaseLayout(this Window window, ELayout layout)
		{
			s_windowBaseLayout[window] = layout;
		}

		public static ModalStack GetModalStack(this Window window)
		{
			return s_windowModalStack[window];
		}

		public static void SetModalStack(this Window window, ModalStack modalStack)
		{
			s_windowModalStack[window] = modalStack;
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