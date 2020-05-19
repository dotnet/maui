using System;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.DualScreen.UnitTests
{
	internal class TestDualScreenService : IDualScreenService
	{
		Point _location;
		public TestDualScreenService()
		{
			DeviceInfo = new TestDeviceInfo();
			IsSpanned = false;
			_location = Point.Zero;
		}

		public bool IsSpanned { get; set; }

		public bool IsLandscape => DeviceInfo.CurrentOrientation == DeviceOrientation.Landscape;

		public DeviceInfo DeviceInfo { get; set; }

		public Size ScaledScreenSize => DeviceInfo.ScaledScreenSize;

		public event EventHandler OnScreenChanged;

		public void Dispose()
		{
		}

		public Rectangle GetHinge()
		{
			if (!IsSpanned)
				return Rectangle.Zero;

			if(IsLandscape)
				return new Rectangle(0, 490, DeviceInfo.ScaledScreenSize.Width, 20);

			return new Rectangle(490, 0, 20, DeviceInfo.ScaledScreenSize.Height);
		}


		public Point? GetLocationOnScreen(VisualElement visualElement) => _location;

		public Point? SetLocationOnScreen(Point point) => _location = point;

		public object WatchForChangesOnLayout(VisualElement visualElement, Action action)
		{
			EventHandler<EventArg<VisualElement>> handler = (_, __) => action();
			visualElement.BatchCommitted += handler;
			return handler;
		}

		public void StopWatchingForChangesOnLayout(VisualElement visualElement, object handle)
		{
			if(handle is EventHandler<EventArg<VisualElement>> eh)
				visualElement.BatchCommitted -= eh;
		}

		public Task<int> GetHingeAngleAsync() => Task.FromResult(0);
	}

	internal class TestDualScreenServiceLandscape : TestDualScreenService
	{
		public TestDualScreenServiceLandscape()
		{
			DeviceInfo.CurrentOrientation = DeviceOrientation.Landscape;
		}
	}

	internal class TestDualScreenServicePortrait : TestDualScreenService
	{
		public TestDualScreenServicePortrait()
		{
			DeviceInfo.CurrentOrientation = DeviceOrientation.Portrait;
		}
	}
}
