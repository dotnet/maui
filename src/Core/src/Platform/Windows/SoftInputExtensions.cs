using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Input;
using Windows.UI.ViewManagement;

namespace Microsoft.Maui.Platform
{
	public static partial class SoftInputExtensions
	{

		internal static bool HideSoftInput(this FrameworkElement element)
		{
			if (TryGetInputPane(out var inputPane))
			{
				return inputPane.TryHide();
			}

			return false;
		}

		internal static bool ShowSoftInput(this FrameworkElement element)
		{
			if (TryGetInputPane(out var inputPane))
			{
				return inputPane.TryShow();
			}

			return false;
		}

		internal static bool IsSoftInputShowing(this FrameworkElement element)
		{
			if (TryGetInputPane(out var inputPane))
			{
				return inputPane.Visible;
			}

			return false;
		}

		internal static bool TryGetInputPane([NotNullWhen(true)] out InputPane? inputPane)
		{
			var handleId = Process.GetCurrentProcess().MainWindowHandle;
			if (handleId == IntPtr.Zero)
			{
				inputPane = null;

				return false;
			}

			inputPane = InputPaneInterop.GetForWindow(handleId);
			return true;
		}
	}
}