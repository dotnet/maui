using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ElmSharp;
using Microsoft.Maui.ApplicationModel;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		static Dictionary<Window, Func<bool>> s_windowBackButtonPressedHandler = new Dictionary<Window, Func<bool>>();
		static Dictionary<Window, Action> s_windowCloseRequestHandler = new Dictionary<Window, Action>();
		static Dictionary<Window, ELayout> s_windowBaseLayout = new Dictionary<Window, ELayout>();
		static Dictionary<Window, ModalStack> s_windowModalStack = new Dictionary<Window, ModalStack>();

		public static void Initialize(this Window platformWindow)
		{
			var baseLayout = (ELayout?)platformWindow.GetType().GetProperty("BaseLayout")?.GetValue(platformWindow);

			if (baseLayout == null)
			{
				var conformant = new Conformant(platformWindow);
				conformant.Show();

				var layout = new ApplicationLayout(conformant);
				layout.Show();

				baseLayout = layout;
				conformant.SetContent(baseLayout);
			}
			platformWindow.SetBaseLayout(baseLayout);
			var modalStack = new ModalStack(baseLayout)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			modalStack.Show();
			baseLayout.SetContent(modalStack);
			platformWindow.SetModalStack(modalStack);

			platformWindow.Active();
			platformWindow.Show();
			platformWindow.AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_90 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270;

			platformWindow.RotationChanged += (sender, e) =>
			{
				// TODO : should update later
			};

			platformWindow.BackButtonPressed += (s, e) => OnBackButtonPressed(platformWindow);
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

		public static float GetDisplayDensity(this Window window)
		{
			return (float)DeviceInfo.ScalingFactor;
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

		internal static Devices.DisplayOrientation GetOrientation(this IWindow? window)
		{
			if (window == null)
				return Devices.DeviceDisplay.Current.MainDisplayInfo.Orientation;

			bool isTV = Elementary.GetProfile() == "tv";
			return window.Handler?.MauiContext?.GetPlatformWindow()?.Rotation switch
			{
				0 => isTV ? Devices.DisplayOrientation.Landscape : Devices.DisplayOrientation.Portrait,
				90 => isTV ? Devices.DisplayOrientation.Portrait : Devices.DisplayOrientation.Landscape,
				180 => isTV ? Devices.DisplayOrientation.Landscape : Devices.DisplayOrientation.Portrait,
				270 => isTV ? Devices.DisplayOrientation.Portrait : Devices.DisplayOrientation.Landscape,
				_ => Devices.DisplayOrientation.Unknown
			};
		}
	}
}