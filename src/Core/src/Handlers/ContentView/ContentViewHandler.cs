#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.ContentPanel;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Represents the view handler for the abstract <see cref="IContentView"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class ContentViewHandler : IContentViewHandler
	{
		public static IPropertyMapper<IContentView, IContentViewHandler> Mapper =
			new PropertyMapper<IContentView, IContentViewHandler>(ViewMapper)
			{
				[nameof(IContentView.Content)] = MapContent,
#if TIZEN
				[nameof(IContentView.Background)] = MapBackground,
#endif
			};

		public static CommandMapper<IContentView, IContentViewHandler> CommandMapper =
			new(ViewCommandMapper);

		public ContentViewHandler() : base(Mapper, CommandMapper)
		{

		}

		public ContentViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ContentViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IContentView IContentViewHandler.VirtualView => VirtualView;

		PlatformView IContentViewHandler.PlatformView => PlatformView;

#if TIZEN
		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IContentView"/> instance.</param>
		public static partial void MapBackground(IContentViewHandler handler, IContentView view);
#endif

		/// <summary>
		/// Maps the abstract <see cref="IContentView.Content"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="page">The associated <see cref="IContentView"/> instance.</param>
		public static partial void MapContent(IContentViewHandler handler, IContentView page);
	}
}
