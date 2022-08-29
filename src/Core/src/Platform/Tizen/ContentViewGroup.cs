using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Platform
{
	public class ContentViewGroup : ViewGroup, IMeasurable
	{
		IView? _virtualView;
		Size _measureCache;
		bool _needMeasureUpdate;

		public ContentViewGroup(IView? view)
		{
			_virtualView = view;
			LayoutUpdated += OnLayoutUpdated;
		}

		public Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		public Func<Rect, Size>? CrossPlatformArrange { get; set; }

		public void SetNeedMeasureUpdate()
		{
			_needMeasureUpdate = true;
			MarkChanged();
		}

		public void ClearNeedMeasureUpdate()
		{
			_needMeasureUpdate = false;
		}

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return InvokeCrossPlatformMeasure(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel();
		}

		public Size InvokeCrossPlatformMeasure(double availableWidth, double availableHeight)
		{
			if (CrossPlatformMeasure == null)
				return Graphics.Size.Zero;

			var measured = CrossPlatformMeasure(availableWidth, availableHeight);
			if (measured != _measureCache && _virtualView?.Parent is IView parentView)
			{
				parentView?.InvalidateMeasure();
			}
			_measureCache = measured;
			ClearNeedMeasureUpdate();
			return measured;
		}

		void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			if (CrossPlatformArrange == null || CrossPlatformMeasure == null)
				return;

			var platformGeometry = this.GetBounds().ToDP();
			if (_needMeasureUpdate || _measureCache != platformGeometry.Size)
			{
				InvokeCrossPlatformMeasure(platformGeometry.Width, platformGeometry.Height);
			}

			if (platformGeometry.Width > 0 && platformGeometry.Height > 0)
			{
				platformGeometry.X = 0;
				platformGeometry.Y = 0;
				CrossPlatformArrange(platformGeometry);
			}
		}
	}
}
