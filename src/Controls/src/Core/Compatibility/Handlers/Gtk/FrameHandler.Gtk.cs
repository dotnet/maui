using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public partial class FrameHandler : ViewHandler<Frame, FrameView>
	{
		public static IPropertyMapper<Frame, FrameHandler> Mapper = new PropertyMapper<Frame, FrameHandler>(ViewRenderer.VisualElementRendererMapper)
		{
			[VisualElement.BackgroundProperty.PropertyName] = MapBackground, 
			[Frame.ContentProperty.PropertyName] = MapContent, 
			[Frame.CornerRadiusProperty.PropertyName] = MapCornerRadius, 
			[Frame.BorderColorProperty.PropertyName] = MapBorderColor,
		};

		public static CommandMapper<Frame, FrameHandler> CommandMapper = new(ViewRenderer.VisualElementRendererCommandMapper) { };

		public FrameHandler() : base(Mapper, CommandMapper) { }

		public FrameHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper) { }

		public FrameHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		[MissingMapper]
		protected override FrameView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(Frame)}");

			var view = new FrameView() { CrossPlatformLayout = VirtualView };

			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;
		}

		public static void MapContent(FrameHandler handler, IView view)
		{
			if (handler.PlatformView is not FrameView platformView)
				return;

			UpdateContent(handler);
		}

		static void UpdateContent(FrameHandler handler)
		{
			var platformView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var mauiContext = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var virtualView = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (virtualView is { Content: IView view })
				platformView.Content = view.ToPlatform(mauiContext);
		}

		public static void MapCornerRadius(FrameHandler handler, Frame frame)
		{
			handler.PlatformView.UpdateMapCornerRadius(frame.CornerRadius);
		}

		public static void MapBorderColor(FrameHandler handler, Frame frame)
		{
			handler.PlatformView.UpdateBorderColor(frame.BorderColor);
		}
	}
}