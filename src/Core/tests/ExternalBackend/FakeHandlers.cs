using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// A label handler for a platform that .NET MAUI does not ship support for.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This handler cannot implement <see cref="ILabelHandler"/>: that interface declares
	/// <c>PlatformView</c> through a <c>using PlatformView = ...</c> alias that resolves to one of
	/// .NET MAUI's own native types on a platform target framework (<c>MauiLabel</c>,
	/// <c>AppCompatTextView</c>, <c>TextBlock</c>), or to <c>System.Object</c> on the platform-neutral
	/// one. An external native type therefore produces <c>CS0738</c> or <c>CS9333</c>.
	/// </para>
	/// <para>
	/// It does not need to. <see cref="ViewHandler{TVirtualView, TPlatformView}"/> implements
	/// <see cref="IViewHandler{TVirtualView, TPlatformView}"/>, so this type already satisfies
	/// <c>IViewHandler&lt;ILabel, FakeNativeLabel&gt;</c> — and, through covariance,
	/// <c>IViewHandler&lt;ILabel, object&gt;</c> — with no extra members at all, on every target
	/// framework.
	/// </para>
	/// </remarks>
	public class FakeLabelHandler : ViewHandler<ILabel, FakeNativeLabel>
	{
		public static IPropertyMapper<ILabel, FakeLabelHandler> Mapper =
			new PropertyMapper<ILabel, FakeLabelHandler>(ViewMapper)
			{
				[nameof(ILabel.Text)] = MapText,
				[nameof(IView.Opacity)] = MapOpacity,
			};

		public FakeLabelHandler()
			: base(Mapper)
		{
		}

		protected override FakeNativeLabel CreatePlatformView() =>
#if MONOANDROID
			new FakeNativeLabel(Context);
#else
			new FakeNativeLabel();
#endif

		public static void MapText(FakeLabelHandler handler, ILabel label) =>
			handler.PlatformView.FakeText = label.Text;

		public static void MapOpacity(FakeLabelHandler handler, ILabel label) =>
			handler.PlatformView.FakeOpacity = label.Opacity;
	}

	/// <summary>
	/// A content view handler for a platform that .NET MAUI does not ship support for.
	/// </summary>
	public class FakeContentViewHandler : ViewHandler<IContentView, FakeNativeContentView>
	{
		public static IPropertyMapper<IContentView, FakeContentViewHandler> Mapper =
			new PropertyMapper<IContentView, FakeContentViewHandler>(ViewMapper)
			{
				[nameof(IContentView.Content)] = MapContent,
			};

		public FakeContentViewHandler()
			: base(Mapper)
		{
		}

		protected override FakeNativeContentView CreatePlatformView() =>
#if MONOANDROID
			new FakeNativeContentView(Context);
#else
			new FakeNativeContentView();
#endif

		public static void MapContent(FakeContentViewHandler handler, IContentView contentView)
		{
			handler.PlatformView.FakeContent =
				(contentView.PresentedContent as IView)?.Handler?.PlatformView as FakeNativeView;
		}
	}

	/// <summary>
	/// A window handler for a platform that .NET MAUI does not ship support for.
	/// </summary>
	/// <remarks>
	/// A window is not a view, so this derives from
	/// <see cref="ElementHandler{TVirtualView, TPlatformView}"/> and is matched platform-neutrally
	/// through <see cref="IElementHandler{TVirtualView, TPlatformView}"/>.
	/// </remarks>
	public class FakeWindowHandler : ElementHandler<IWindow, FakeNativeWindow>
	{
		public static IPropertyMapper<IWindow, FakeWindowHandler> Mapper =
			new PropertyMapper<IWindow, FakeWindowHandler>(ElementMapper)
			{
				[nameof(IWindow.Title)] = MapTitle,
			};

		public FakeWindowHandler()
			: base(Mapper)
		{
		}

		protected override FakeNativeWindow CreatePlatformElement() => new FakeNativeWindow();

		public static void MapTitle(FakeWindowHandler handler, IWindow window) =>
			handler.PlatformView.FakeTitle = window.Title;
	}
}
