#nullable enable
using System;
using System.Collections.Specialized;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, MauiComboBox>
	{
		WBrush? _defaultForeground;

		protected override MauiComboBox CreatePlatformView()
		{
			var platformPicker = new MauiComboBox();

			if (VirtualView != null)
				platformPicker.ItemsSource = new ItemDelegateList<string>(VirtualView);

			return platformPicker;
		}

		protected override void ConnectHandler(MauiComboBox platformView)
		{
			platformView.SelectionChanged += OnControlSelectionChanged;

			if (VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged += OnRowsCollectionChanged;

			SetupDefaults(platformView);
		}

		protected override void DisconnectHandler(MauiComboBox platformView)
		{
			platformView.SelectionChanged -= OnControlSelectionChanged;

			if (VirtualView.Items is INotifyCollectionChanged notifyCollection)
				notifyCollection.CollectionChanged -= OnRowsCollectionChanged;
		}

		void SetupDefaults(MauiComboBox platformView)
		{
			_defaultForeground = platformView.Foreground;
		}

		static void Reload(IPickerHandler handler)
		{
			if (handler.VirtualView == null || handler.PlatformView == null)
				return;
			handler.PlatformView.ItemsSource = new ItemDelegateList<string>(handler.VirtualView);
		}

		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		public static void MapTitle(IPickerHandler handler, IPicker picker) 
		{
			handler.PlatformView?.UpdateTitle(picker);
		}

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateSelectedIndex(picker);
		}

		public static void MapCharacterSpacing(IPickerHandler handler, IPicker picker) 
		{
			handler.PlatformView?.UpdateCharacterSpacing(picker);
		}

		public static void MapFont(IPickerHandler handler, IPicker picker) 
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(picker, fontManager);
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			if (handler is PickerHandler platformHandler)
			{
				platformHandler.PlatformView?.UpdateTextColor(picker, platformHandler._defaultForeground);
			}
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(picker);
		}
		
		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(picker);
		}

		void OnControlSelectionChanged(object? sender, WSelectionChangedEventArgs e)
		{
			if (VirtualView != null && PlatformView != null)
				VirtualView.SelectedIndex = PlatformView.SelectedIndex;
		}

		void OnRowsCollectionChanged(object? sender, EventArgs e)
		{
			Reload(this);
		}
	}
}
