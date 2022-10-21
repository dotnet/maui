using System;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, ContentView>
	{
		protected override ContentView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentView)}");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			return new MauiRadioButton(VirtualView);
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.View = view;
			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		static void UpdateContent(IRadioButtonHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// Cleanup the old view when reused
			handler.PlatformView.ClearSubviews();

			if (handler.VirtualView.PresentedContent is IView view)
				handler.PlatformView.AddSubview(view.ToPlatform(handler.MauiContext));
		}

		public static void MapContent(IRadioButtonHandler handler, IContentView page)
		{
			UpdateContent(handler);
		}

		[MissingMapper]
		public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapTextColor(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapFont(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapCornerRadius(IRadioButtonHandler handler, IRadioButton radioButton) { }
	}
}