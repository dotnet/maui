using System;
using Microsoft.Maui.Devices;
using Xunit;

namespace Tests
{
	public class DeviceDisplay_Tests : IDisposable
	{
		public void Dispose()
		{
			DeviceDisplay.SetCurrent(null);
		}

		[Theory]
		[InlineData(0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, true)]
		[InlineData(1.1, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 1.1, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, true)]
		[InlineData(0.0, 0.0, 0.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0, true)]
		[InlineData(1.1, 0.0, 2.2, DisplayOrientation.Landscape, DisplayRotation.Rotation180, 1.1, 0.0, 2.2, DisplayOrientation.Landscape, DisplayRotation.Rotation180, true)]
		[InlineData(1.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(0.0, 1.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(0.0, 0.0, 1.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(0.0, 0.0, 0.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(1.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation180, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		public void DeviceDisplay_Comparison(
			double width1,
			double height1,
			double density1,
			DisplayOrientation orientation1,
			DisplayRotation rotation1,
			double width2,
			double height2,
			double density2,
			DisplayOrientation orientation2,
			DisplayRotation rotation2,
			bool equals)
		{
			var device1 = new DisplayInfo(
				width: width1,
				height: height1,
				density: density1,
				orientation: orientation1,
				rotation: rotation1);

			var device2 = new DisplayInfo(
				width: width2,
				height: height2,
				density: density2,
				orientation: orientation2,
				rotation: rotation2);

			if (equals)
			{
				Assert.True(device1.Equals(device2));
				Assert.True(device1 == device2);
				Assert.False(device1 != device2);
				Assert.Equal(device1, device2);
				Assert.Equal(device1.GetHashCode(), device2.GetHashCode());
			}
			else
			{
				Assert.False(device1.Equals(device2));
				Assert.True(device1 != device2);
				Assert.False(device1 == device2);
				Assert.NotEqual(device1, device2);
				Assert.NotEqual(device1.GetHashCode(), device2.GetHashCode());
			}
		}

		[Fact]
		public void Default_Current_Is_Correct()
		{
			Assert.NotNull(DeviceDisplay.Current);
			Assert.Equal("DeviceDisplayImplementation", DeviceDisplay.Current.GetType().Name);
		}

		[Fact]
		public void Setting_Null_Is_Default()
		{
			DeviceDisplay.SetCurrent(new MyDisplay());
			DeviceDisplay.SetCurrent(null);

			Assert.NotNull(DeviceDisplay.Current);
			Assert.Equal("DeviceDisplayImplementation", DeviceDisplay.Current.GetType().Name);
		}

		[Fact]
		public void Setting_Custom_Null_Is_Custom()
		{
			DeviceDisplay.SetCurrent(new MyDisplay());

			Assert.NotNull(DeviceDisplay.Current);
			Assert.Equal("MyDisplay", DeviceDisplay.Current.GetType().Name);
		}

		[Fact]
		public void Setting_KeepScreenOn_Invokes_Correct_Members()
		{
			var display = new MyDisplay();

			DeviceDisplay.SetCurrent(display);
			display.ResetCounts();

			DeviceDisplay.KeepScreenOn = true;

			Assert.Equal(0, display.StartedCount);
			Assert.Equal(0, display.StoppedCount);
			Assert.Equal(0, display.AddedCount);
			Assert.Equal(0, display.RemovedCount);
			Assert.Equal(1, display.SetKeepScreenOnCount);
			Assert.Equal(0, display.GetKeepScreenOnCount);
			Assert.Equal(0, display.GetMainDisplayInfoCount);

			Assert.True(display.KeepScreenOn);
		}

		[Fact]
		public void MainDisplayInfo_Invokes_Correct_Members()
		{
			var expected = new DisplayInfo(100, 100, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0);
			var display = new MyDisplay(expected);

			DeviceDisplay.SetCurrent(display);
			display.ResetCounts();

			var main = DeviceDisplay.MainDisplayInfo;
			Assert.Equal(expected, main);

			Assert.Equal(0, display.StartedCount);
			Assert.Equal(0, display.StoppedCount);
			Assert.Equal(0, display.AddedCount);
			Assert.Equal(0, display.RemovedCount);
			Assert.Equal(0, display.SetKeepScreenOnCount);
			Assert.Equal(0, display.GetKeepScreenOnCount);
			Assert.Equal(1, display.GetMainDisplayInfoCount);

			Assert.False(display.KeepScreenOn);
		}

		[Fact]
		public void Adding_MainDisplayInfoChanged_Invokes_Correct_Members()
		{
			var onChangedInvokeCount = 0;
			var display = new MyDisplay();

			DeviceDisplay.SetCurrent(display);
			display.ResetCounts();

			DeviceDisplay.MainDisplayInfoChanged += OnChanged;

			Assert.Equal(1, display.StartedCount);
			Assert.Equal(0, display.StoppedCount);
			Assert.Equal(1, display.AddedCount);
			Assert.Equal(0, display.RemovedCount);
			Assert.Equal(0, display.SetKeepScreenOnCount);
			Assert.Equal(0, display.GetKeepScreenOnCount);
			Assert.Equal(1, display.GetMainDisplayInfoCount);
			Assert.Equal(0, onChangedInvokeCount);

			Assert.False(display.KeepScreenOn);

			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;

			void OnChanged(object sender, DisplayInfoChangedEventArgs e)
			{
				onChangedInvokeCount++;
			}
		}

		[Fact]
		public void Adding_Second_MainDisplayInfoChanged_Invokes_Correct_Members()
		{
			var onChangedInvokeCount = 0;
			var display = new MyDisplay();

			DeviceDisplay.SetCurrent(display);
			DeviceDisplay.MainDisplayInfoChanged += OnChanged;
			display.ResetCounts();

			DeviceDisplay.MainDisplayInfoChanged += OnChanged;

			Assert.Equal(0, display.StartedCount);
			Assert.Equal(0, display.StoppedCount);
			Assert.Equal(1, display.AddedCount);
			Assert.Equal(0, display.RemovedCount);
			Assert.Equal(0, display.SetKeepScreenOnCount);
			Assert.Equal(0, display.GetKeepScreenOnCount);
			Assert.Equal(0, display.GetMainDisplayInfoCount);
			Assert.Equal(0, onChangedInvokeCount);

			Assert.False(display.KeepScreenOn);

			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;
			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;

			void OnChanged(object sender, DisplayInfoChangedEventArgs e)
			{
				onChangedInvokeCount++;
			}
		}

		[Fact]
		public void Removing_None_MainDisplayInfoChanged_Invokes_Correct_Members()
		{
			var onChangedInvokeCount = 0;
			var display = new MyDisplay();

			DeviceDisplay.SetCurrent(display);
			display.ResetCounts();

			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;

			Assert.Equal(0, display.StartedCount);
			Assert.Equal(0, display.StoppedCount);
			Assert.Equal(0, display.AddedCount);
			Assert.Equal(1, display.RemovedCount);
			Assert.Equal(0, display.SetKeepScreenOnCount);
			Assert.Equal(0, display.GetKeepScreenOnCount);
			Assert.Equal(0, display.GetMainDisplayInfoCount);
			Assert.Equal(0, onChangedInvokeCount);

			Assert.False(display.KeepScreenOn);

			void OnChanged(object sender, DisplayInfoChangedEventArgs e)
			{
				onChangedInvokeCount++;
			}
		}

		[Fact]
		public void Removing_First_MainDisplayInfoChanged_Invokes_Correct_Members()
		{
			var onChangedInvokeCount = 0;
			var display = new MyDisplay();

			DeviceDisplay.SetCurrent(display);
			DeviceDisplay.MainDisplayInfoChanged += OnChanged;
			display.ResetCounts();

			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;

			Assert.Equal(0, display.StartedCount);
			Assert.Equal(1, display.StoppedCount);
			Assert.Equal(0, display.AddedCount);
			Assert.Equal(1, display.RemovedCount);
			Assert.Equal(0, display.SetKeepScreenOnCount);
			Assert.Equal(0, display.GetKeepScreenOnCount);
			Assert.Equal(0, display.GetMainDisplayInfoCount);
			Assert.Equal(0, onChangedInvokeCount);

			Assert.False(display.KeepScreenOn);

			void OnChanged(object sender, DisplayInfoChangedEventArgs e)
			{
				onChangedInvokeCount++;
			}
		}

		[Fact]
		public void Removing_Second_MainDisplayInfoChanged_Invokes_Correct_Members()
		{
			var onChangedInvokeCount = 0;
			var display = new MyDisplay();

			DeviceDisplay.SetCurrent(display);
			DeviceDisplay.MainDisplayInfoChanged += OnChanged;
			DeviceDisplay.MainDisplayInfoChanged += OnChanged;
			display.ResetCounts();

			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;

			Assert.Equal(0, display.StartedCount);
			Assert.Equal(0, display.StoppedCount);
			Assert.Equal(0, display.AddedCount);
			Assert.Equal(1, display.RemovedCount);
			Assert.Equal(0, display.SetKeepScreenOnCount);
			Assert.Equal(0, display.GetKeepScreenOnCount);
			Assert.Equal(0, display.GetMainDisplayInfoCount);
			Assert.Equal(0, onChangedInvokeCount);

			Assert.False(display.KeepScreenOn);

			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;
			DeviceDisplay.MainDisplayInfoChanged -= OnChanged;

			void OnChanged(object sender, DisplayInfoChangedEventArgs e)
			{
				onChangedInvokeCount++;
			}
		}

		class MyDisplay : DeviceDisplayImplementationBase, IDeviceDisplay
		{
			private bool _keepScreenOn;
			private DisplayInfo _displayInfo;

			public MyDisplay(DisplayInfo displayInfo = default)
			{
				_displayInfo = displayInfo;
			}

			public int StartedCount { get; private set; }

			public int StoppedCount { get; private set; }

			public int AddedCount { get; private set; }

			public int RemovedCount { get; private set; }

			public int SetKeepScreenOnCount { get; private set; }

			public int GetKeepScreenOnCount { get; private set; }

			public int GetMainDisplayInfoCount { get; private set; }

			event EventHandler<DisplayInfoChangedEventArgs> IDeviceDisplay.MainDisplayInfoChanged
			{
				add
				{
					MainDisplayInfoChanged += value;
					AddedCount++;
				}
				remove
				{
					MainDisplayInfoChanged -= value;
					RemovedCount++;
				}
			}

			protected override bool GetKeepScreenOn()
			{
				GetKeepScreenOnCount++;
				return _keepScreenOn;
			}

			protected override void SetKeepScreenOn(bool keepScreenOn)
			{
				SetKeepScreenOnCount++;
				_keepScreenOn = keepScreenOn;
			}

			protected override DisplayInfo GetMainDisplayInfo()
			{
				GetMainDisplayInfoCount++;
				return _displayInfo;
			}

			protected override void StartScreenMetricsListeners()
			{
				StartedCount++;
			}

			protected override void StopScreenMetricsListeners()
			{
				StoppedCount++;
			}

			internal void ResetCounts()
			{
				StartedCount = 0;
				StoppedCount = 0;
				AddedCount = 0;
				RemovedCount = 0;
				SetKeepScreenOnCount = 0;
				GetKeepScreenOnCount = 0;
				GetMainDisplayInfoCount = 0;
			}
		}
	}
}