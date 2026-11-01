#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.Card.MaterialCardView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Button;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	/// <summary>Maps a content button to its native implementation.</summary>
	public partial class ContentButtonHandler : ViewHandler<IContentButton, PlatformView>, IContentButtonHandler
	{
		/// <summary>The property mapper for content buttons.</summary>
		public static IPropertyMapper<IContentButton, IContentButtonHandler> Mapper =
			new PropertyMapper<IContentButton, IContentButtonHandler>(ViewMapper)
			{
				[nameof(IContentView.Content)] = MapContent,
				[nameof(IView.Background)] = MapBackground,
				[nameof(IView.IsEnabled)] = MapIsEnabled,
				[nameof(IPadding.Padding)] = MapPadding,
				[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
				[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
				[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
			};

		/// <summary>The command mapper for content buttons.</summary>
		public static CommandMapper<IContentButton, IContentButtonHandler> CommandMapper = new(ViewCommandMapper);

		/// <summary>Creates a content button handler with the default mappers.</summary>
		public ContentButtonHandler() : base(Mapper, CommandMapper) { }

		/// <summary>Creates a content button handler with a custom property mapper.</summary>
		/// <param name="mapper">The property mapper.</param>
		public ContentButtonHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		/// <summary>Creates a content button handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper.</param>
		/// <param name="commandMapper">The command mapper.</param>
		public ContentButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		IContentButton IContentButtonHandler.VirtualView => VirtualView;
		PlatformView IContentButtonHandler.PlatformView => PlatformView;

		/// <summary>Updates the native content.</summary>
		public static partial void MapContent(IContentButtonHandler handler, IContentButton button);
		/// <summary>Updates the native background.</summary>
		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button);
		/// <summary>Updates the native enabled state.</summary>
		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button);
		/// <summary>Updates content padding.</summary>
		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button);
		/// <summary>Updates the border color.</summary>
		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button);
		/// <summary>Updates the border width.</summary>
		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button);
		/// <summary>Updates the corner radius.</summary>
		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button);
	}
}
