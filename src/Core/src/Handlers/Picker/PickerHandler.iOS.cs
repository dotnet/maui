using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, UIButton>
	{
		internal bool UpdateImmediately { get; set; }

		protected override UIButton CreatePlatformView()
		{
			var button = new UIButton(UIButtonType.System);

			button.ShowsMenuAsPrimaryAction = true;
			button.ChangesSelectionAsPrimaryAction = true;

			button.BackgroundColor = UIColor.SystemBackground;
			button.Layer.BorderWidth = 0.5f;
			button.Layer.BorderColor = UIColor.Separator.CGColor;
			button.Layer.CornerRadius = 5f;
			button.ContentEdgeInsets = new UIEdgeInsets(8, 12, 8, 12);

			button.AccessibilityTraits = UIAccessibilityTrait.Button;

			return button;
		}

		void UpdateMenu()
		{
			if (PlatformView == null || VirtualView == null)
				return;

			var items = VirtualView.Items;
			var count = VirtualView.GetCount();

			if (count == 0)
			{
				PlatformView.Menu = null;
				return;
			}

			var menuElements = new UIMenuElement[count];

			for (int i = 0; i < count; i++)
			{
				var index = i;
				var title = VirtualView.GetItem(index);
				var action = UIAction.Create(title, null, null, _ => OnMenuItemSelected(index));
				menuElements[i] = action;
			}

			PlatformView.Menu = UIMenu.Create("Picker Menu", menuElements);
		}

		void OnMenuItemSelected(int index)
		{
			if (VirtualView == null)
				return;

			VirtualView.SelectedIndex = index;
			UpdateSelectedText();

			if (VirtualView is IPicker picker)
			{
				picker.IsFocused = false;
			}
		}

		void UpdateSelectedText()
		{
			if (PlatformView == null || VirtualView == null)
				return;

			var selectedIndex = VirtualView.SelectedIndex;

			if (selectedIndex >= 0 && selectedIndex < VirtualView.GetCount())
			{
				PlatformView.SetTitle(VirtualView.GetItem(selectedIndex), UIControlState.Normal);
			}
			else
			{
				PlatformView.SetTitle(VirtualView.Title ?? string.Empty, UIControlState.Normal);
			}
		}

		protected override void ConnectHandler(UIButton platformView)
		{
			UpdateMenu();
			UpdateSelectedText();

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			base.DisconnectHandler(platformView);
		}

		static void Reload(IPickerHandler handler)
		{
			if (handler is PickerHandler pickerHandler)
			{
				pickerHandler.UpdateMenu();
				pickerHandler.UpdateSelectedText();
			}
		}

		[Obsolete("Use Microsoft.Maui.Handlers.PickerHandler.MapItems instead")]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			if (handler is PickerHandler pickerHandler)
				pickerHandler.UpdateSelectedText();
		}

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			if (handler is PickerHandler pickerHandler)
				pickerHandler.UpdateTitleColor();
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			if (handler is PickerHandler pickerHandler)
				pickerHandler.UpdateSelectedText();
		}

		public static void MapCharacterSpacing(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapFont(IPickerHandler handler, IPicker picker)
		{
			if (handler is PickerHandler pickerHandler)
			{
				var fontManager = handler.GetRequiredService<IFontManager>();
				pickerHandler.UpdateFont(picker, fontManager);
			}
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			if (handler is PickerHandler pickerHandler)
				pickerHandler.UpdateTextColor();
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
		}

		internal static void MapIsOpen(IPickerHandler handler, IPicker picker)
		{
		}

		void UpdateTitleColor()
		{
			if (PlatformView == null || VirtualView == null)
				return;

			var titleColor = VirtualView.TitleColor;
			if (titleColor != null)
			{
				PlatformView.SetTitleColor(titleColor.ToPlatform(), UIControlState.Normal);
			}
			else
			{
				PlatformView.SetTitleColor(UIColor.Label, UIControlState.Normal);
			}
		}

		void UpdateTextColor()
		{
			if (PlatformView == null || VirtualView == null)
				return;

			var textColor = VirtualView.TextColor;
			if (textColor != null)
			{
				PlatformView.SetTitleColor(textColor.ToPlatform(), UIControlState.Normal);
			}
			else
			{
				PlatformView.SetTitleColor(UIColor.Label, UIControlState.Normal);
			}
		}

		void UpdateFont(IPicker picker, IFontManager fontManager)
		{
			if (PlatformView == null)
				return;

			var font = picker.Font;
			var uiFont = fontManager.GetFont(font);
			PlatformView.TitleLabel.Font = uiFont;
		}
	}
}
