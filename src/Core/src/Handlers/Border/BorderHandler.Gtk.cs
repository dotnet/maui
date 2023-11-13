using System;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, BorderView>
	{
		[MissingMapper]
		protected override BorderView CreatePlatformView() => new();

		static partial void UpdateContent(IBorderHandler handler)
		{
			var platformView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var mauiContext = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var virtualView = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (virtualView is { Content: IView view })
				platformView.Content = view.ToPlatform(mauiContext);
		}
	}
}
