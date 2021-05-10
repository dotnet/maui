using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public class LayoutView : UIView
	{
		public override CGSize SizeThatFits(CGSize size)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.SizeThatFits(size);
			}

			var width = size.Width;
			var height = size.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);

			return base.SizeThatFits(crossPlatformSize.ToCGSize());
		}

		public IView? View { get; set; }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = Frame.ToRectangle();
			if (View is ISafeAreaView sav && !sav.IgnoreSafeArea && RespondsToSafeArea())
			{
				var safe = SafeAreaInsets;
				bounds.X += safe.Left;
				bounds.Y += safe.Top;
				bounds.Height -= safe.Top + safe.Bottom;
				bounds.Width -= safe.Left + safe.Right;
			}

			CrossPlatformMeasure?.Invoke(bounds.Width, bounds.Height);
			CrossPlatformArrange?.Invoke(bounds);
		}
		static bool? respondsToSafeArea;
		bool RespondsToSafeArea()
		{
			if (respondsToSafeArea.HasValue)
				return respondsToSafeArea.Value;
			return (bool)(respondsToSafeArea = this.RespondsToSelector(new Selector("safeAreaInsets")));
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }
	}

	public class PageView : UIView
	{
		public override CGSize SizeThatFits(CGSize size)
		{
			return size;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var width = Frame.Width;
			var height = Frame.Height;

			CrossPlatformArrange?.Invoke(Frame.ToRectangle());
		}

		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }
	}

	public class PageViewController : ContainerViewController
	{
		public PageViewController(IPage page, IMauiContext mauiContext)
		{
			CurrentView = page;
			Context = mauiContext;
			LoadFirstView(page);
		}

		protected override UIView CreateNativeView(IView view)
		{
			return new PageView
			{
				CrossPlatformArrange = view.Arrange,
			};
		}
	}
}
