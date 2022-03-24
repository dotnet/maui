#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	public interface IDeviceDisplay
	{
		bool KeepScreenOn { get; set; }

		DisplayInfo MainDisplayInfo { get; }

		event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfoChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DisplayInfoChangedEventArgs']/Docs" />
	public class DisplayInfoChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfoChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public DisplayInfoChangedEventArgs(DisplayInfo displayInfo) =>
			DisplayInfo = displayInfo;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfoChangedEventArgs.xml" path="//Member[@MemberName='DisplayInfo']/Docs" />
		public DisplayInfo DisplayInfo { get; }
	}

	public static class DeviceDisplay
	{
#if WINDOWS
		internal const float BaseLogicalDpi = 96.0f;
#elif ANDROID
		internal const float BaseLogicalDpi = 160.0f;
#endif

		static IDeviceDisplay? currentImplementation;

		public static IDeviceDisplay Current =>
			currentImplementation ??= new DeviceDisplayImplementation();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='SetCurrent']/Docs" />
		internal static void SetCurrent(IDeviceDisplay? implementation) =>
			currentImplementation = implementation;
	}

	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChangedInternal;

		DisplayInfo currentMetrics;

		public DisplayInfo MainDisplayInfo => GetMainDisplayInfo();

		public event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add
			{
				if (MainDisplayInfoChangedInternal is null)
				{
					SetCurrent(MainDisplayInfo);
					StartScreenMetricsListeners();
				}
				MainDisplayInfoChangedInternal += value;
			}
			remove
			{
				MainDisplayInfoChangedInternal -= value;
				if (MainDisplayInfoChangedInternal is null)
					StopScreenMetricsListeners();
			}
		}

		void SetCurrent(DisplayInfo metrics) =>
			currentMetrics = new DisplayInfo(
				metrics.Width, metrics.Height,
				metrics.Density,
				metrics.Orientation,
				metrics.Rotation,
				metrics.RefreshRate);

		void OnMainDisplayInfoChanged(DisplayInfoChangedEventArgs e)
		{
			if (!currentMetrics.Equals(e.DisplayInfo))
			{
				SetCurrent(e.DisplayInfo);
				MainDisplayInfoChangedInternal?.Invoke(null, e);
			}
		}

		void OnMainDisplayInfoChanged()
		{
			var metrics = GetMainDisplayInfo();
			OnMainDisplayInfoChanged(new DisplayInfoChangedEventArgs(metrics));
		}
	}
}