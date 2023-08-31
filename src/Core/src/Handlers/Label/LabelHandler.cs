#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiLabel;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatTextView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBlock;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Label;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Represents the view handler for the abstract <see cref="ILabel"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class LabelHandler : ILabelHandler
	{
		public static IPropertyMapper<ILabel, ILabelHandler> Mapper = new PropertyMapper<ILabel, ILabelHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS
			[nameof(ILabel.Height)] = MapHeight,
#endif
#if TIZEN
			[nameof(ILabel.Shadow)] = MapShadow,
#endif
			[nameof(ILabel.Background)] = MapBackground,
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(ILabel.LineHeight)] = MapLineHeight,
			[nameof(ILabel.Padding)] = MapPadding,
			[nameof(ILabel.Text)] = MapText,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(ILabel.TextDecorations)] = MapTextDecorations,
		};

		public static CommandMapper<ILabel, ILabelHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public LabelHandler() : base(Mapper, CommandMapper)
		{
		}

		public LabelHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public LabelHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ILabel ILabelHandler.VirtualView => VirtualView;

		PlatformView ILabelHandler.PlatformView => PlatformView;

#if WINDOWS
		/// <summary>
		/// Maps the abstract <see cref="IView.Height"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapHeight(ILabelHandler handler, ILabel view);
#endif

#if TIZEN
		/// <summary>
		/// Maps the abstract <see cref="IView.Shadow"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapShadow(ILabelHandler handler, ILabel label);
#endif

		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapBackground(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="IText.Text"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapText(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="ITextStyle.TextColor"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapTextColor(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="ITextStyle.CharacterSpacing"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapCharacterSpacing(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="ITextStyle.Font"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapFont(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="ITextAlignment.HorizontalTextAlignment"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="ITextAlignment.VerticalTextAlignment"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapVerticalTextAlignment(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="ILabel.TextDecorations"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapTextDecorations(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="IPadding.Padding"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapPadding(ILabelHandler handler, ILabel label);

		/// <summary>
		/// Maps the abstract <see cref="ILabel.LineHeight"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="label">The associated <see cref="ILabel"/> instance.</param>
		public static partial void MapLineHeight(ILabelHandler handler, ILabel label);
	}
}