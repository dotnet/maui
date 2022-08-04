using System;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, ContentViewGroup>
	{
		IPlatformViewHandler? _contentHandler;

		protected override ContentViewGroup CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentViewGroup)}");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			return new ContentViewGroup(VirtualView)
			{
				Focusable = true,
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};
		}

		protected override void ConnectHandler(ContentViewGroup platformView)
		{
			platformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			platformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
			platformView.KeyEvent += OnKeyEvent;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ContentViewGroup platformView)
		{
			if (!platformView.HasBody())
				return;

			platformView.KeyEvent -= OnKeyEvent;
			base.DisconnectHandler(platformView);
		}

		bool OnKeyEvent(object source, Tizen.NUI.BaseComponents.View.KeyEventArgs e)
		{
			if (e.Key.IsAcceptKeyEvent())
			{
				VirtualView.IsChecked = !VirtualView.IsChecked;
				return true;
			}
			return false;
		}

		void UpdateContent()
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			PlatformView.Children.Clear();
			_contentHandler?.Dispose();
			_contentHandler = null;

			if (VirtualView.PresentedContent is IView view)
			{
				PlatformView.Children.Add(view.ToPlatform(MauiContext));
				if (view.Handler is IPlatformViewHandler thandler)
				{
					_contentHandler = thandler;
				}
			}
		}

		public static void MapContent(IRadioButtonHandler handler, IContentView page)
		{
			(handler as RadioButtonHandler)?.UpdateContent();
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