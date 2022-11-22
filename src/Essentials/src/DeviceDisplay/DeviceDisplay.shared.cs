#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	public interface IDeviceDisplay
	{
		bool KeepScreenOn { get; set; }

		DisplayInfo MainDisplayInfo { get; }

		event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfoChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DisplayInfoChangedEventArgs']/Docs/*" />
	public class DisplayInfoChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfoChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DisplayInfoChangedEventArgs(DisplayInfo displayInfo) =>
			DisplayInfo = displayInfo;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfoChangedEventArgs.xml" path="//Member[@MemberName='DisplayInfo']/Docs/*" />
		public DisplayInfo DisplayInfo { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceDisplay']/Docs/*" />
	public static class DeviceDisplay
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='KeepScreenOn']/Docs/*" />
		public static bool KeepScreenOn
		{
			get => Current.KeepScreenOn;
			set => Current.KeepScreenOn = value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='MainDisplayInfo']/Docs/*" />
		public static DisplayInfo MainDisplayInfo => Current.MainDisplayInfo;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='MainDisplayInfoChanged']/Docs/*" />
		public static event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add => Current.MainDisplayInfoChanged += value;
			remove => Current.MainDisplayInfoChanged -= value;
		}

#if WINDOWS
		internal const float BaseLogicalDpi = 96.0f;
#elif ANDROID || TIZEN
		internal const float BaseLogicalDpi = 160.0f;
#endif

		static IDeviceDisplay? currentImplementation;

		public static IDeviceDisplay Current =>
			currentImplementation ??= new DeviceDisplayImplementation();

		internal static void SetCurrent(IDeviceDisplay? implementation) =>
			currentImplementation = implementation;
	}

	sealed partial class DeviceDisplayImplementation : DeviceDisplayImplementationBase
	{
	}

	abstract class DeviceDisplayImplementationBase : IDeviceDisplay
	{
		event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChangedInternal;

		DisplayInfo _currentMetrics;

		public DisplayInfo MainDisplayInfo => GetMainDisplayInfo();

		public bool KeepScreenOn
		{
			get => GetKeepScreenOn();
			set => SetKeepScreenOn(value);
		}

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
				var wasStopped = MainDisplayInfoChangedInternal is null;
				MainDisplayInfoChangedInternal -= value;
				if (!wasStopped && MainDisplayInfoChangedInternal is null)
					StopScreenMetricsListeners();
			}
		}

		void SetCurrent(DisplayInfo metrics) =>
			_currentMetrics = new DisplayInfo(
				metrics.Width, metrics.Height,
				metrics.Density,
				metrics.Orientation,
				metrics.Rotation,
				metrics.RefreshRate);

		protected void OnMainDisplayInfoChanged(DisplayInfoChangedEventArgs e)
		{
			if (!_currentMetrics.Equals(e.DisplayInfo))
			{
				SetCurrent(e.DisplayInfo);
				MainDisplayInfoChangedInternal?.Invoke(null, e);
			}
		}

		protected void OnMainDisplayInfoChanged()
		{
			var metrics = GetMainDisplayInfo();
			OnMainDisplayInfoChanged(new DisplayInfoChangedEventArgs(metrics));
		}

		protected abstract DisplayInfo GetMainDisplayInfo();

		protected abstract bool GetKeepScreenOn();

		protected abstract void SetKeepScreenOn(bool keepScreenOn);

		protected abstract void StartScreenMetricsListeners();

		protected abstract void StopScreenMetricsListeners();
	}
}
