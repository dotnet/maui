using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using TSize = Tizen.UIExtensions.Common.Size;
using TTouch = Tizen.NUI.Touch;

namespace Microsoft.Maui.Platform
{
	public class LayoutViewGroup : ViewGroup, IMeasurable
	{
		IView _virtualView;
		Size _measureCache;

		bool _needMeasureUpdate;
		internal int IsLayoutUpdating { get; set; }

		public LayoutViewGroup(IView view)
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
			if (IsLayoutUpdating == 0)
			{
				Layout.RequestLayout();
			}
		}

		public void ClearNeedMeasureUpdate()
		{
			_needMeasureUpdate = false;
		}

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return InvokeCrossPlatformMeasure(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel();
		}

		public bool InputTransparent { get; set; }

		protected override bool HitTest(TTouch touch)
		{
			return !InputTransparent;
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
			IsLayoutUpdating++;
			var platformGeometry = this.GetBounds().ToDP();

			if (_needMeasureUpdate || _measureCache != platformGeometry.Size)
			{
				InvokeCrossPlatformMeasure(platformGeometry.Width, platformGeometry.Height);
			}
			if (platformGeometry.Width > 0 && platformGeometry.Height > 0)
			{
				platformGeometry.X = 0;
				platformGeometry.Y = 0;
				CrossPlatformArrange?.Invoke(platformGeometry);
			}
			IsLayoutUpdating--;
		}
	}
}
