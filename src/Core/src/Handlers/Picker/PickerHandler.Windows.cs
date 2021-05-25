#nullable enable
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiComboBox>
	{
		WBrush? _defaultForeground;

		protected override MauiComboBox CreateNativeView()
		{
			var nativePicker = new MauiComboBox();

			if (VirtualView != null)
				nativePicker.ItemsSource = new ItemDelegateList<string>(VirtualView);

			return nativePicker;
		}

		protected override void ConnectHandler(MauiComboBox nativeView)
		{
			nativeView.SelectionChanged += OnControlSelectionChanged;
		}

		protected override void DisconnectHandler(MauiComboBox nativeView)
		{
			nativeView.SelectionChanged -= OnControlSelectionChanged;
		}

		protected override void SetupDefaults(MauiComboBox nativeView)
		{
			_defaultForeground = nativeView.Foreground;

			base.SetupDefaults(nativeView);
		}
		void Reload()
		{

			if (VirtualView == null || NativeView == null)
				return;
			NativeView.ItemsSource = new ItemDelegateList<string>(VirtualView);
		}

		public static void MapReload(PickerHandler handler, IPicker picker) => handler.Reload();

		public static void MapTitle(PickerHandler handler, IPicker picker) 
		{
			handler.NativeView?.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateSelectedIndex(picker);
		}

		public static void MapCharacterSpacing(PickerHandler handler, IPicker picker) 
		{
			handler.NativeView?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(PickerHandler handler, IPicker picker) 
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(picker, fontManager);
		}

		public static void MapTextColor(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateTextColor(picker, handler._defaultForeground);
		}

		public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(picker);
		}

		void OnControlSelectionChanged(object? sender, WSelectionChangedEventArgs e)
		{
			if (VirtualView != null && NativeView != null)
				VirtualView.SelectedIndex = NativeView.SelectedIndex;
		}
	}
}
