using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<ITemplatedIndicatorView, UIPageControl>
	{
		MauiPageControl? UIPager => NativeView as MauiPageControl;

		bool _updatingPosition;
		protected override UIPageControl CreateNativeView() => new MauiPageControl();

		public override void NativeArrange(Rectangle rect)
		{
			base.NativeArrange(rect);
			if (UIPager != null)
				UIPager.Frame = new CGRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		protected override void ConnectHandler(UIPageControl nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.ValueChanged += UIPagerValueChanged;
			UpdateIndicator();
		}

		protected override void DisconnectHandler(UIPageControl nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.ValueChanged -= UIPagerValueChanged;
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdateIndicatorCount();
		}
		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdatePosition();
		}
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView?.UpdateHideSingle(indicator);
		}
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UpdateIndicatorCount();
		}
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UIPager?.UpdateIndicatorSize(indicator);
		}
		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView?.UpdatePagesIndicatorTintColor(indicator);
		}
		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView?.UpdateCurrentPagesIndicatorTintColor(indicator);
		}
		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UIPager?.UpdateIndicatorShape(indicator);
		}

		void UpdateIndicator()
		{
			if (VirtualView.IndicatorsLayoutOverride == null)
				return;

			UIView? handler;
			if (MauiContext != null)
			{
				ClearIndicators();
				handler = VirtualView.IndicatorsLayoutOverride.ToNative(MauiContext);
				NativeView.AddSubview(handler);
			}

			void ClearIndicators()
			{
				foreach (var child in NativeView.Subviews)
					child.RemoveFromSuperview();
			}
		}

		void UIPagerValueChanged(object sender, EventArgs e)
		{
			if (_updatingPosition || UIPager == null)
				return;

			VirtualView.Position = (int)UIPager.CurrentPage;
		}

		void UpdatePosition()
		{
			_updatingPosition = true;
			UIPager?.UpdateCurrentPage(GetCurrentPage());
			_updatingPosition = false;

			int GetCurrentPage()
			{
				var maxVisible = GetMaximumVisible();
				var position = VirtualView.Position;
				var index = position >= maxVisible ? maxVisible - 1 : position;
				return index;
			}
		}

		void UpdateIndicatorCount()
		{
			NativeView?.UpdatePages(GetMaximumVisible());
			UpdatePosition();
		}

		int GetMaximumVisible()
		{
			var minValue = Math.Min(VirtualView.MaximumVisible, VirtualView.Count);
			var maximumVisible = minValue <= 0 ? 0 : minValue;
			bool hideSingle = VirtualView.HideSingle;

			if (maximumVisible == 1 && hideSingle)
				maximumVisible = 0;

			return maximumVisible;
		}
	}
}