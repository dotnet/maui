#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.RadioButton;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : IRadioButtonHandler
	{
		public static IPropertyMapper<IRadioButton, IRadioButtonHandler> Mapper = new PropertyMapper<IRadioButton, IRadioButtonHandler>(ViewHandler.ViewMapper)
		{
#if ANDROID || WINDOWS
			[nameof(IRadioButton.Background)] = MapBackground,
#endif
			[nameof(IRadioButton.IsChecked)] = MapIsChecked,
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(IRadioButton.Content)] = MapContent,
			[nameof(IRadioButton.StrokeColor)] = MapStrokeColor,
			[nameof(IRadioButton.StrokeThickness)] = MapStrokeThickness,
			[nameof(IRadioButton.CornerRadius)] = MapCornerRadius,
		};

		public static CommandMapper<IRadioButton, IRadioButtonHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public RadioButtonHandler() : base(Mapper, CommandMapper)
		{
		}

		public RadioButtonHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public RadioButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IRadioButton IRadioButtonHandler.VirtualView => VirtualView;

		PlatformView IRadioButtonHandler.PlatformView => PlatformView;
	}
}