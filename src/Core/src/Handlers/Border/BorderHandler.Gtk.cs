using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{

	public partial class BorderHandler : ViewHandler<IBorderView, BorderView>
	{

		[MissingMapper]
		protected override BorderView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Border");

			var view = new BorderView()
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};
			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		static partial void UpdateContent(IBorderHandler handler)
		{
			var platformView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var mauiContext = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var virtualView = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (virtualView is { PresentedContent: IView view })
				platformView.Content = view.ToPlatform(mauiContext);
		}
		
		public override void PlatformArrange(Rect rect)
		{
			PlatformView?.CrossPlatformArrange?.Invoke(rect);

			PlatformView?.Arrange(rect);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (PlatformView?.CrossPlatformMeasure is { })
			{
				var cf= PlatformView.CrossPlatformMeasure(widthConstraint, heightConstraint);

				return cf;
			}

			var size = base.GetDesiredSize(widthConstraint, heightConstraint);

			return size;
		}

	}

}