#nullable enable
using System;
using System.ComponentModel;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceDisplay']/Docs" />
	public static partial class DeviceDisplay
	{
#if WINDOWS
		internal const float BaseLogicalDpi = 96.0f;
#elif ANDROID
		internal const float BaseLogicalDpi = 160.0f;
#endif

		static readonly object locker = new object();
		static IDeviceDisplay currentImplementation;

		static DeviceDisplay()
		{
			currentImplementation = new DeviceDisplayImplementation();
			currentImplementation.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='Current']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDeviceDisplay Current => currentImplementation;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='SetCurrent']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IDeviceDisplay? implementation)
		{
			lock (locker)
			{
				if (currentImplementation == implementation)
					return;

				var newImplementation = implementation ?? new DeviceDisplayImplementation();

				var oldImplementation = currentImplementation;
				currentImplementation = newImplementation;

				if (oldImplementation is not null)
				{
					oldImplementation.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;

					var wasAlwaysOn = oldImplementation.KeepScreenOn;
					if (wasAlwaysOn)
					{
						oldImplementation.KeepScreenOn = false;
						newImplementation.KeepScreenOn = true;
					}

					var wasRunning = MainDisplayInfoChangedInternal != null;
					if (wasRunning)
					{
						oldImplementation.StopScreenMetricsListeners();
						newImplementation.StartScreenMetricsListeners();
					}
				}

				newImplementation.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

				SetCurrent(newImplementation.GetMainDisplayInfo());
			}
		}

		static event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChangedInternal;

		static DisplayInfo currentMetrics;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='KeepScreenOn']/Docs" />
		public static bool KeepScreenOn
		{
			get => Current.KeepScreenOn;
			set => Current.KeepScreenOn = value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='MainDisplayInfo']/Docs" />
		public static DisplayInfo MainDisplayInfo => Current.GetMainDisplayInfo();

		static void SetCurrent(DisplayInfo metrics) =>
			currentMetrics = new DisplayInfo(metrics.Width, metrics.Height, metrics.Density, metrics.Orientation, metrics.Rotation, metrics.RefreshRate);

		public static event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add
			{
				var wasRunning = MainDisplayInfoChangedInternal != null;

				MainDisplayInfoChangedInternal += value;

				if (!wasRunning && MainDisplayInfoChangedInternal != null)
				{
					SetCurrent(Current.GetMainDisplayInfo());
					Current.StartScreenMetricsListeners();
				}
			}

			remove
			{
				var wasRunning = MainDisplayInfoChangedInternal != null;

				MainDisplayInfoChangedInternal -= value;

				if (wasRunning && MainDisplayInfoChangedInternal == null)
					Current.StopScreenMetricsListeners();
			}
		}

		static void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) =>
			OnMainDisplayInfoChanged(e);

		static void OnMainDisplayInfoChanged(DisplayInfoChangedEventArgs e)
		{
			if (!currentMetrics.Equals(e.DisplayInfo))
			{
				SetCurrent(e.DisplayInfo);
				MainDisplayInfoChangedInternal?.Invoke(null, e);
			}
		}
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

	public interface IDeviceDisplay
	{
		bool KeepScreenOn { get; set; }

		void StartScreenMetricsListeners();

		void StopScreenMetricsListeners();

		DisplayInfo GetMainDisplayInfo();

		event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged;
	}
}
