#if IOS && !MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif MACCATALYST
using PlatformView = UIKit.UIDatePicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.CalendarDatePicker;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Entry;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Represents the view handler for the abstract <see cref="IDatePicker"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class DatePickerHandler : IDatePickerHandler
	{
		public static IPropertyMapper<IDatePicker, IDatePickerHandler> Mapper = new PropertyMapper<IDatePicker, IDatePickerHandler>(ViewHandler.ViewMapper)
		{
#if ANDROID || WINDOWS
			[nameof(IDatePicker.Background)] = MapBackground,
#elif IOS
			[nameof(IDatePicker.FlowDirection)] = MapFlowDirection,
#endif
			[nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IDatePicker.Date)] = MapDate,
			[nameof(IDatePicker.Font)] = MapFont,
			[nameof(IDatePicker.Format)] = MapFormat,
			[nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
			[nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
			[nameof(IDatePicker.TextColor)] = MapTextColor,
			[nameof(IDatePicker.IsOpen)] = MapIsOpen,
		};

		public static CommandMapper<IPicker, IDatePickerHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public DatePickerHandler() : base(Mapper, CommandMapper)
		{
		}

		public DatePickerHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public DatePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IDatePicker IDatePickerHandler.VirtualView => VirtualView;

		PlatformView IDatePickerHandler.PlatformView => PlatformView;

#if ANDROID || WINDOWS
		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapBackground(IDatePickerHandler handler, IDatePicker datePicker);
#endif

#if IOS || MACCATALYST
		/// <summary>
		/// Maps the abstract <see cref="IView.FlowDirection"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapFlowDirection(IDatePickerHandler handler, IDatePicker datePicker);
#endif

		/// <summary>
		/// Maps the abstract <see cref="IDatePicker.Format"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker);

		/// <summary>
		/// Maps the abstract <see cref="IDatePicker.Date"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker);

		/// <summary>
		/// Maps the abstract <see cref="IDatePicker.MinimumDate"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker);

		/// <summary>
		/// Maps the abstract <see cref="IDatePicker.MaximumDate"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker);

		/// <summary>
		/// Maps the abstract <see cref="ITextStyle.CharacterSpacing"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker);

		/// <summary>
		/// Maps the abstract <see cref="ITextStyle.Font"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker);

		/// <summary>
		/// Maps the abstract <see cref="ITextStyle.TextColor"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="datePicker">The associated <see cref="IDatePicker"/> instance.</param>
		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker);

		/// <summary>
		/// Maps the abstract <see cref="IDatePicker.IsOpen"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler that controls platform behavior.</param>
		/// <param name="datePicker">The <see cref="IDatePicker"/> instance that determines whether the date picker dropdown is open.</param>
		/// <remarks>
		/// This method is responsible for ensuring that opening and closing the date picker is correctly handled across different platforms.
		/// </remarks>
		internal static partial void MapIsOpen(IDatePickerHandler handler, IDatePicker datePicker);
	}
}