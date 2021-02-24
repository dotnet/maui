using System;
using ElmSharp;
using ElmSharp.Wearable;
using static Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native.TableView;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using EEntry = ElmSharp.Entry;
using ELabel = ElmSharp.Label;
using ELayout = ElmSharp.Layout;
using EProgressBar = ElmSharp.ProgressBar;
using ESize = ElmSharp.Size;
using ESlider = ElmSharp.Slider;
using EToolbarItem = ElmSharp.ToolbarItem;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class ThemeManager
	{
		#region Layout
		public static EdjeTextPartObject GetContentPartEdjeObject(this ELayout layout)
		{
			return layout?.EdjeObject[ThemeConstants.Layout.Parts.Content];
		}

		public static EdjeTextPartObject GetTextPartEdjeObject(this ELayout layout)
		{
			return layout?.EdjeObject[ThemeConstants.Layout.Parts.Text];
		}

		public static bool SetTextPart(this ELayout layout, string text)
		{
			return layout.SetPartText(ThemeConstants.Layout.Parts.Text, text);
		}

		public static bool SetContentPart(this ELayout layout, EvasObject content, bool preserveOldContent = false)
		{
			var ret = layout.SetPartContent(ThemeConstants.Layout.Parts.Content, content, preserveOldContent);
			if (!ret)
			{
				// Restore theme to default if given layout is not available
				layout.SetTheme("layout", "application", "default");
				ret = layout.SetPartContent(ThemeConstants.Layout.Parts.Content, content, preserveOldContent);
			}
			return ret;
		}

		public static bool SetBackgroundPart(this ELayout layout, EvasObject content, bool preserveOldContent = false)
		{
			return layout.SetPartContent(ThemeConstants.Layout.Parts.Background, content, preserveOldContent);
		}

		public static bool SetOverlayPart(this ELayout layout, EvasObject content, bool preserveOldContent = false)
		{
			return layout.SetPartContent(ThemeConstants.Layout.Parts.Overlay, content, preserveOldContent);
		}
		#endregion

		#region Entry
		public static bool SetPlaceHolderTextPart(this EEntry entry, string text)
		{
			return entry.SetPartText(ThemeConstants.Entry.Parts.PlaceHolderText, text);
		}

		public static void SetVerticalTextAlignment(this EEntry entry, double valign)
		{
			entry.SetVerticalTextAlignment(ThemeConstants.Common.Parts.Text, valign);
		}

		public static void SetVerticalPlaceHolderTextAlignment(this EEntry entry, double valign)
		{
			entry.SetVerticalTextAlignment(ThemeConstants.Entry.Parts.PlaceHolderText, valign);
		}

		public static ESize GetTextBlockFormattedSize(this EEntry entry)
		{
			var textPart = entry.EdjeObject[ThemeConstants.Common.Parts.Text];
			if (textPart == null)
			{
				Log.Error("There is no elm.text part");
				return new ESize(0, 0);
			}
			return textPart.TextBlockFormattedSize;
		}

		public static ESize GetTextBlockNativeSize(this EEntry entry)
		{
			var textPart = entry.EdjeObject[ThemeConstants.Common.Parts.Text];
			if (textPart == null)
			{
				Log.Error("There is no elm.text part");
				return new ESize(0, 0);
			}
			return textPart.TextBlockNativeSize;
		}

		public static ESize GetPlaceHolderTextBlockFormattedSize(this EEntry entry)
		{
			var textPart = entry.EdjeObject[ThemeConstants.Entry.Parts.PlaceHolderText];
			if (textPart == null)
			{
				Log.Error("There is no elm.guide part");
				return new ESize(0, 0);
			}
			return textPart.TextBlockFormattedSize;
		}

		public static ESize GetPlaceHolderTextBlockNativeSize(this EEntry entry)
		{
			var textPart = entry.EdjeObject[ThemeConstants.Entry.Parts.PlaceHolderText];
			if (textPart == null)
			{
				Log.Error("There is no elm.guide part");
				return new ESize(0, 0);
			}
			return textPart.TextBlockNativeSize;
		}
		#endregion

		#region Label
		public static void SetVerticalTextAlignment(this ELabel label, double valign)
		{
			label.SetVerticalTextAlignment(ThemeConstants.Common.Parts.Text, valign);
		}

		public static double GetVerticalTextAlignment(this ELabel label)
		{
			return label.GetVerticalTextAlignment(ThemeConstants.Common.Parts.Text);
		}

		public static ESize GetTextBlockFormattedSize(this ELabel label)
		{
			var textPart = label.EdjeObject[ThemeConstants.Common.Parts.Text];
			if (textPart == null)
			{
				Log.Error("There is no elm.text part");
				return new ESize(0, 0);
			}
			return textPart.TextBlockFormattedSize;
		}
		#endregion

		#region Button
		public static ESize GetTextBlockNativeSize(this EButton button)
		{
			var textPart = button.EdjeObject[ThemeConstants.Common.Parts.Text];
			if (textPart == null)
			{
				Log.Error("There is no elm.text part");
				return new ESize(0, 0);
			}
			return textPart.TextBlockNativeSize;
		}

		public static void SetTextBlockStyle(this EButton button, string style)
		{
			var textBlock = button.EdjeObject[ThemeConstants.Common.Parts.Text];
			if (textBlock != null)
			{
				textBlock.TextStyle = style;
			}
		}

		public static void SendTextVisibleSignal(this EButton button, bool isVisible)
		{
			button.SignalEmit(isVisible ? ThemeConstants.Button.Signals.TextVisibleState : ThemeConstants.Button.Signals.TextHiddenState, ThemeConstants.Button.Signals.ElementaryCode);
		}

		public static EButton SetDefaultStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.Default;
			return button;
		}

		public static EButton SetBottomStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.Bottom;
			return button;
		}

		public static EButton SetPopupStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.Popup;
			return button;
		}

		public static EButton SetNavigationTitleRightStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.NavigationTitleRight;
			return button;
		}

		public static EButton SetNavigationTitleLeftStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.NavigationTitleLeft;
			return button;
		}

		public static EButton SetNavigationBackStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.NavigationBack;
			return button;
		}

		public static EButton SetNavigationDrawerStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.NavigationDrawers;
			return button;
		}

		public static EButton SetTransparentStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.Transparent;
			return button;
		}

		public static EButton SetWatchPopupRightStyle(this EButton button)
		{
			if (Device.Idiom != TargetIdiom.Watch)
			{
				Log.Error($"ToWatchPopupRightStyleButton is only supported on TargetIdiom.Watch : {0}", Device.Idiom);
				return button;
			}
			button.Style = ThemeConstants.Button.Styles.Watch.PopupRight;
			return button;
		}

		public static EButton SetWatchPopupLeftStyle(this EButton button)
		{
			if (Device.Idiom != TargetIdiom.Watch)
			{
				Log.Error($"WatchPopupLeftStyleButton is only supported on TargetIdiom.Watch : {0}", Device.Idiom);
				return button;
			}
			button.Style = ThemeConstants.Button.Styles.Watch.PopupLeft;
			return button;
		}

		public static EButton SetWatchTextStyle(this EButton button)
		{
			if (Device.Idiom != TargetIdiom.Watch)
			{
				Log.Error($"ToWatchPopupRightStyleButton is only supported on TargetIdiom.Watch : {0}", Device.Idiom);
				return button;
			}
			button.Style = ThemeConstants.Button.Styles.Watch.Text;
			return button;
		}

		public static bool SetIconPart(this EButton button, EvasObject content, bool preserveOldContent = false)
		{
			return button.SetPartContent(ThemeConstants.Button.Parts.Icon, content, preserveOldContent);
		}

		public static EButton SetEditFieldClearStyle(this EButton button)
		{
			button.Style = ThemeConstants.Button.Styles.EditFieldClear;
			return button;
		}

		public static EColor GetIconColor(this EButton button)
		{
			var ret = EColor.Default;
			if (button == null)
				return ret;

			ret = button.GetPartColor(ThemeConstants.Button.ColorClass.Icon);
			return ret;
		}

		public static void SetIconColor(this EButton button, EColor color)
		{
			if (button == null)
				return;

			button.SetPartColor(ThemeConstants.Button.ColorClass.Icon, color);
			button.SetPartColor(ThemeConstants.Button.ColorClass.IconPressed, color);
		}

		public static void SetEffectColor(this EButton button, EColor color)
		{
			if (button == null)
				return;

			button.SetPartColor(ThemeConstants.Button.ColorClass.Effect, color);
			button.SetPartColor(ThemeConstants.Button.ColorClass.EffectPressed, color);
		}

		#endregion

		#region Popup
		public static Popup SetWatchCircleStyle(this Popup popup)
		{
			if (Device.Idiom != TargetIdiom.Watch)
			{
				Log.Error($"WatchCircleStylePopup is only supported on TargetIdiom.Watch : {0}", Device.Idiom);
				return popup;
			}
			popup.Style = ThemeConstants.Popup.Styles.Watch.Circle;
			return popup;
		}

		public static void SetTitleColor(this Popup popup, EColor color)
		{
			popup.SetPartColor(Device.Idiom == TargetIdiom.TV ? ThemeConstants.Popup.ColorClass.TV.Title : ThemeConstants.Popup.ColorClass.Title, color);
		}

		public static void SetTitleBackgroundColor(this Popup popup, EColor color)
		{
			popup.SetPartColor(ThemeConstants.Popup.ColorClass.TitleBackground, color);
		}

		public static void SetContentBackgroundColor(this Popup popup, EColor color)
		{
			popup.SetPartColor(ThemeConstants.Popup.ColorClass.ContentBackground, color);
		}

		public static bool SetTitleTextPart(this Popup popup, string title)
		{
			return popup.SetPartText(ThemeConstants.Popup.Parts.Title, title);
		}

		public static bool SetButton1Part(this Popup popup, EvasObject content, bool preserveOldContent = false)
		{
			return popup.SetPartContent(ThemeConstants.Popup.Parts.Button1, content, preserveOldContent);
		}

		public static bool SetButton2Part(this Popup popup, EvasObject content, bool preserveOldContent = false)
		{
			return popup.SetPartContent(ThemeConstants.Popup.Parts.Button2, content, preserveOldContent);
		}

		public static bool SetButton3Part(this Popup popup, EvasObject content, bool preserveOldContent = false)
		{
			return popup.SetPartContent(ThemeConstants.Popup.Parts.Button3, content, preserveOldContent);
		}
		#endregion

		#region ProgressBar
		public static EProgressBar SetSmallStyle(this EProgressBar progressBar)
		{
			progressBar.Style = ThemeConstants.ProgressBar.Styles.Small;
			return progressBar;
		}

		public static EProgressBar SetLargeStyle(this EProgressBar progressBar)
		{
			progressBar.Style = ThemeConstants.ProgressBar.Styles.Large;
			return progressBar;
		}
		#endregion

		#region Check

		public static void SetOnColors(this Check check, EColor color)
		{
			foreach (string s in check.GetColorParts())
			{
				check.SetPartColor(s, color);
			}
		}

		public static void DeleteOnColors(this Check check)
		{
			foreach (string s in check.GetColorEdjeParts())
			{
				check.EdjeObject.DeleteColorClass(s);
			}
		}

		public static string[] GetColorParts(this Check check)
		{
			if (Device.Idiom == TargetIdiom.Watch)
			{
				if (check.Style == ThemeConstants.Check.Styles.Toggle)
				{
					return new string[] { ThemeConstants.Check.ColorClass.Watch.OuterBackgroundOn };
				}
				else
				{
					return new string[] {
						ThemeConstants.Check.ColorClass.Watch.OuterBackgroundOn,
						ThemeConstants.Check.ColorClass.Watch.OuterBackgroundOnPressed,
						ThemeConstants.Check.ColorClass.Watch.CheckOn,
						ThemeConstants.Check.ColorClass.Watch.CheckOnPressed
					};
				}
			}
			else if (Device.Idiom == TargetIdiom.TV)
			{
				if (check.Style == ThemeConstants.Check.Styles.Toggle)
				{
					return new string[] { ThemeConstants.Check.ColorClass.TV.SliderOn, ThemeConstants.Check.ColorClass.TV.SliderFocusedOn };
				}
				else
				{
					return new string[] {
						ThemeConstants.Check.ColorClass.TV.SliderOn,
						ThemeConstants.Check.ColorClass.TV.SliderFocusedOn,
					};
				}
			}
			else
			{
				if (check.Style == ThemeConstants.Check.Styles.Toggle)
				{
					return new string[] { ThemeConstants.Check.ColorClass.BackgroundOn };
				}
				else
				{
					return new string[] { ThemeConstants.Check.ColorClass.BackgroundOn, ThemeConstants.Check.ColorClass.Stroke };
				}
			}
		}

		public static string[] GetColorEdjeParts(this Check check)
		{
			string[] ret = check.GetColorParts();

			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = check.ClassName.ToLower().Replace("elm_", "") + "/" + ret[i];
			}
			return ret;
		}
		#endregion

		#region NaviItem
		public static void SetTitle(this NaviItem item, string text)
		{
			item.SetPartText(ThemeConstants.NaviItem.Parts.Title, text);
		}

		public static void SetBackButton(this NaviItem item, EvasObject content, bool preserveOldContent = false)
		{
			item.SetPartContent(ThemeConstants.NaviItem.Parts.BackButton, content, preserveOldContent);
		}

		public static void SetLeftToolbarButton(this NaviItem item, EvasObject content, bool preserveOldContent = false)
		{
			item.SetPartContent(ThemeConstants.NaviItem.Parts.LeftToolbarButton, content, preserveOldContent);
		}

		public static void SetRightToolbarButton(this NaviItem item, EvasObject content, bool preserveOldContent = false)
		{
			item.SetPartContent(ThemeConstants.NaviItem.Parts.RightToolbarButton, content, preserveOldContent);
		}

		public static void SetNavigationBar(this NaviItem item, EvasObject content, bool preserveOldContent = false)
		{
			item.SetPartContent(ThemeConstants.NaviItem.Parts.NavigationBar, content, preserveOldContent);
		}

		public static NaviItem SetNavigationBarStyle(this NaviItem item)
		{
			item.Style = ThemeConstants.NaviItem.Styles.NavigationBar;
			return item;
		}

		public static NaviItem SetTabBarStyle(this NaviItem item)
		{
			if (Device.Idiom == TargetIdiom.TV)
			{
				//According to TV UX Guideline, item style should be set to "tabbar" in case of TabbedPage only for TV profile.
				item.Style = ThemeConstants.NaviItem.Styles.TV.TabBar;
			}
			else
			{
				item.Style = ThemeConstants.NaviItem.Styles.Default;
			}
			return item;
		}
		#endregion

		#region Toolbar
		public static Toolbar SetNavigationBarStyle(this Toolbar toolbar)
		{
			toolbar.Style = ThemeConstants.Toolbar.Styles.NavigationBar;
			return toolbar;
		}

		public static Toolbar SetTVTabBarWithTitleStyle(this Toolbar toolbar)
		{
			if (Device.Idiom != TargetIdiom.TV)
			{
				Log.Error($"TabBarWithTitleStyle is only supported on TargetIdiom.TV : {0}", Device.Idiom);
				return toolbar;
			}
			toolbar.Style = ThemeConstants.Toolbar.Styles.TV.TabbarWithTitle;
			return toolbar;
		}
		#endregion

		#region ToolbarItem
		public static void SetIconPart(this EToolbarItem item, EvasObject content, bool preserveOldContent = false)
		{
			item.SetPartContent(ThemeConstants.ToolbarItem.Parts.Icon, content, preserveOldContent);
		}

		public static void SetBackgroundColor(this EToolbarItem item, EColor color)
		{
			item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.Background, color);
		}

		public static void SetUnderlineColor(this EToolbarItem item, EColor color)
		{
			item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.Underline, color);
		}

		public static void SetTextColor(this EToolbarItem item, EColor color)
		{
			if (string.IsNullOrEmpty(item.Icon))
			{
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.Text, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextPressed, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextSelected, color);
			}
			else
			{
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIcon, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconPressed, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconSelected, color);
			}
			item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.Underline, color);
		}

		public static void SetSelectedTabColor(this EToolbarItem item, EColor color)
		{
			if (string.IsNullOrEmpty(item.Icon))
			{
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextSelected, color);
			}
			else
			{
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconSelected, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.IconSelected, color);
			}
			item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.Underline, color);
		}

		public static void SetUnselectedTabColor(this EToolbarItem item, EColor color)
		{
			if (string.IsNullOrEmpty(item.Icon))
			{
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.Text, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextPressed, color);
			}
			else
			{
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIcon, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconPressed, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.Icon, color);
				item.SetPartColor(ThemeConstants.ToolbarItem.ColorClass.IconPressed, color);
			}
		}

		public static void DeleteBackgroundColor(this EToolbarItem item)
		{
			item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.Background);
		}

		public static void DeleteUnderlineColor(this EToolbarItem item)
		{
			item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.Underline);
		}

		public static void DeleteTextColor(this EToolbarItem item)
		{
			if (string.IsNullOrEmpty(item.Icon))
			{
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.Text);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextPressed);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextSelected);
			}
			else
			{
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIcon);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconPressed);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconSelected);
			}
			item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.Underline);
		}

		public static void DeleteSelectedTabColor(this EToolbarItem item)
		{
			if (string.IsNullOrEmpty(item.Icon))
			{
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextSelected);
			}
			else
			{
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconSelected);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.IconSelected);
			}
			item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.Underline);
		}

		public static void DeleteUnselectedTabColor(this EToolbarItem item)
		{
			if (string.IsNullOrEmpty(item.Icon))
			{
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.Text);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextPressed);
			}
			else
			{
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIcon);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.TextUnderIconPressed);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.Icon);
				item.DeletePartColor(ThemeConstants.ToolbarItem.ColorClass.IconPressed);
			}
		}

		#endregion

		#region Background
		public static bool SetOverlayPart(this Background bg, EvasObject content, bool preserveOldContent = false)
		{
			return bg.SetPartContent(ThemeConstants.Background.Parts.Overlay, content, preserveOldContent);
		}
		#endregion

		#region Panes
		public static bool SetLeftPart(this Panes panes, EvasObject content, bool preserveOldContent = false)
		{
			return panes.SetPartContent(ThemeConstants.Panes.Parts.Left, content, preserveOldContent);
		}

		public static bool SetRightPart(this Panes panes, EvasObject content, bool preserveOldContent = false)
		{
			return panes.SetPartContent(ThemeConstants.Panes.Parts.Right, content, preserveOldContent);
		}
		#endregion

		#region Cell
		public static void SendSignalToItem(this Cell cell, GenListItem item)
		{
			// This is only required for TV profile.
			if (Device.Idiom != TargetIdiom.TV)
				return;

			if (cell is ImageCell)
			{
				item.EmitSignal(ThemeConstants.GenListItem.Signals.TV.SinglelineIconTextTheme, "");
			}
			else if (cell is SwitchCell)
			{
				item.EmitSignal(ThemeConstants.GenListItem.Signals.TV.SinglelineTextIconTheme, "");
			}
		}
		#endregion

		#region CellRenderer
		public static string GetTextCellRendererStyle()
		{
			return Device.Idiom == TargetIdiom.TV ? ThemeConstants.GenItemClass.Styles.Default : ThemeConstants.GenItemClass.Styles.DoubleLabel;
		}

		public static string GetTextCellGroupModeStyle(bool isGroupMode)
		{
			return isGroupMode ? ThemeConstants.GenItemClass.Styles.GroupIndex : GetTextCellRendererStyle();
		}

		public static string GetMainPart(this CellRenderer cell)
		{
			switch (cell.Style)
			{
				default:
					return ThemeConstants.GenItemClass.Parts.Text;
			}
		}

		public static string GetDetailPart(this TextCellRenderer textCell)
		{
			// TextCell.Detail property is not supported on TV profile due to UX limitation.
			switch (textCell.Style)
			{
				case ThemeConstants.GenItemClass.Styles.Watch.TwoText1Icon1:
				case ThemeConstants.GenItemClass.Styles.Watch.Icon2Text:
					return ThemeConstants.GenItemClass.Parts.Watch.Text;
				case ThemeConstants.GenItemClass.Styles.GroupIndex:
					return textCell is SectionCellRenderer ? ThemeConstants.GenItemClass.Parts.Ignore : ThemeConstants.GenItemClass.Parts.EndText;
				default:
					return ThemeConstants.GenItemClass.Parts.SubText;
			}
		}

		public static string GetSwitchCellRendererStyle()
		{
			return Device.Idiom == TargetIdiom.Watch ? ThemeConstants.GenItemClass.Styles.Watch.Text1Icon1 : ThemeConstants.GenItemClass.Styles.Default;
		}

		public static string GetSwitchPart(this SwitchCellRenderer switchCell)
		{
			return Device.Idiom == TargetIdiom.Watch ? ThemeConstants.GenItemClass.Parts.Watch.Icon : ThemeConstants.GenItemClass.Parts.End;
		}

		public static int GetDefaultHeightPixel(this EntryCellRenderer entryCell)
		{
			return Forms.ConvertToScaledPixel(ThemeConstants.EntryCell.Resources.DefaultHeight);
		}

		public static string GetImageCellRendererStyle()
		{
			return Device.Idiom == TargetIdiom.Watch ? ThemeConstants.GenItemClass.Styles.Watch.Icon2Text : Device.Idiom == TargetIdiom.TV ? ThemeConstants.GenItemClass.Styles.Default : ThemeConstants.GenItemClass.Styles.DoubleLabel;
		}

		public static string GetImagePart(this ImageCellRenderer imageCell)
		{
			return Device.Idiom == TargetIdiom.Watch ? ThemeConstants.GenItemClass.Parts.Watch.Icon : ThemeConstants.GenItemClass.Parts.Icon;
		}

		public static int GetDefaultHeightPixel(this ImageCellRenderer imageCell)
		{
			return Forms.ConvertToScaledPixel(ThemeConstants.ImageCell.Resources.DefaultHeight);
		}

		public static string GetViewCellRendererStyle()
		{
			return ThemeConstants.GenItemClass.Styles.Full;
		}

		public static string GetMainContentPart(this ViewCellRenderer viewCell)
		{
			return ThemeConstants.GenItemClass.Parts.Content;
		}
		#endregion

		#region GenItemClass
		//public static string GetMainPart()
		#endregion

		#region GenList
		public static GenList SetSolidStyle(this GenList list)
		{
			list.Style = ThemeConstants.GenList.Styles.Solid;
			return list;
		}
		#endregion

		#region GenListItem
		public static void SetBottomlineColor(this GenListItem item, EColor color)
		{
			item.SetPartColor(ThemeConstants.GenListItem.ColorClass.BottomLine, color);
		}

		public static void SetBackgroundColor(this GenListItem item, EColor color)
		{
			item.SetPartColor(ThemeConstants.GenListItem.ColorClass.Background, color);
		}

		public static void DeleteBottomlineColor(this GenListItem item)
		{
			item.DeletePartColor(ThemeConstants.GenListItem.ColorClass.BottomLine);
		}

		public static void DeleteBackgroundColor(this GenListItem item)
		{
			item.DeletePartColor(ThemeConstants.GenListItem.ColorClass.Background);
		}
		#endregion

		#region Radio
		public static ESize GetTextBlockFormattedSize(this Radio radio)
		{
			return radio.EdjeObject[ThemeConstants.Common.Parts.Text].TextBlockFormattedSize;
		}

		public static void SetTextBlockStyle(this Radio radio, string style)
		{
			var textBlock = radio.EdjeObject[ThemeConstants.Common.Parts.Text];
			if (textBlock != null)
			{
				textBlock.TextStyle = style;
			}
		}

		public static void SendTextVisibleSignal(this Radio radio, bool isVisible)
		{
			radio.SignalEmit(isVisible ? ThemeConstants.Radio.Signals.TextVisibleState : ThemeConstants.Radio.Signals.TextHiddenState, ThemeConstants.Radio.Signals.ElementaryCode);
		}
		#endregion

		#region Slider
		public static EColor GetBarColor(this ESlider slider)
		{
			return slider.GetPartColor(ThemeConstants.Slider.ColorClass.Bar);
		}

		public static void SetBarColor(this ESlider slider, EColor color)
		{
			slider.SetPartColor(ThemeConstants.Slider.ColorClass.Bar, color);
			slider.SetPartColor(ThemeConstants.Slider.ColorClass.BarPressed, color);
		}

		public static EColor GetBackgroundColor(this ESlider slider)
		{
			return slider.GetPartColor(ThemeConstants.Slider.ColorClass.Background);
		}

		public static void SetBackgroundColor(this ESlider slider, EColor color)
		{
			slider.SetPartColor(ThemeConstants.Slider.ColorClass.Background, color);
		}

		public static EColor GetHandlerColor(this ESlider slider)
		{
			return slider.GetPartColor(ThemeConstants.Slider.ColorClass.Handler);
		}

		public static void SetHandlerColor(this ESlider slider, EColor color)
		{
			slider.SetPartColor(ThemeConstants.Slider.ColorClass.Handler, color);
			slider.SetPartColor(ThemeConstants.Slider.ColorClass.HandlerPressed, color);
		}
		#endregion

		#region Index
		public static Index SetStyledIndex(this Index index)
		{
			index.Style = Device.Idiom == TargetIdiom.Watch ? ThemeConstants.Index.Styles.Circle : ThemeConstants.Index.Styles.PageControl;
			return index;
		}
		#endregion

		#region IndexItem
		public static void SetIndexItemStyle(this IndexItem item, int itemCount, int offset, int evenMiddleItem, int oddMiddleItem)
		{
			string style;
			int position;

			if (itemCount % 2 == 0)  //Item count is even.
			{
				position = evenMiddleItem - itemCount / 2 + offset;
				style = ThemeConstants.IndexItem.Styles.EvenItemPrefix + position;
			}
			else  //Item count is odd.
			{
				position = oddMiddleItem - itemCount / 2 + offset;
				style = ThemeConstants.IndexItem.Styles.OddItemPrefix + position;
			}
			item.Style = style;
		}
		#endregion

		#region CircleSpinner
		public static bool SetTitleTextPart(this CircleSpinner spinner, string title)
		{
			return spinner.SetPartText(ThemeConstants.Common.Parts.Text, title);
		}
		#endregion

		#region BaseScale
		public static double GetBaseScale(string deviceType)
		{
			if (deviceType.StartsWith("Mobile"))
			{
				return ThemeConstants.Common.Resource.Mobile.BaseScale;
			}
			else if (deviceType.StartsWith("TV"))
			{
				return ThemeConstants.Common.Resource.TV.BaseScale;
			}
			else if (deviceType.StartsWith("Wearable"))
			{
				return ThemeConstants.Common.Resource.Watch.BaseScale;
			}
			else if (deviceType.StartsWith("Refrigerator"))
			{
				return ThemeConstants.Common.Resource.Refrigerator.BaseScale;
			}
			else if (deviceType.StartsWith("TizenIOT"))
			{
				return ThemeConstants.Common.Resource.Iot.BaseScale;
			}
			return 1.0;
		}
		#endregion

		#region ShellNavBar
		static double s_shellNavBarDefaultHeight = -1;
		public static double GetDefaultHeight(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultHeight > 0)
				return s_shellNavBarDefaultHeight;
			return s_shellNavBarDefaultHeight = CalculateDoubleScaledSizeInLargeScreen(70);
		}

		static double s_shellNavBarDefaultMenuSize = -1;
		public static double GetDefaultMenuSize(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultMenuSize > 0)
				return s_shellNavBarDefaultMenuSize;
			return s_shellNavBarDefaultMenuSize = CalculateDoubleScaledSizeInLargeScreen(Device.Idiom == TargetIdiom.TV ? 70 : 40);
		}

		static double s_shellNavBarDefaultMargin = -1;
		public static double GetDefaultMargin(this ShellNavBar navBar)
		{
			if (s_shellNavBarDefaultMargin > 0)
				return s_shellNavBarDefaultMargin;
			return s_shellNavBarDefaultMargin = CalculateDoubleScaledSizeInLargeScreen(10);
		}
		static double s_shellNavBarTitleFontSize = -1;
		public static double GetDefaultTitleFontSize(this ShellNavBar navBar)
		{
			if (s_shellNavBarTitleFontSize > 0)
				return s_shellNavBarTitleFontSize;
			return s_shellNavBarTitleFontSize = CalculateDoubleScaledSizeInLargeScreen(23);
		}
		#endregion

		#region NavigationView

		static double s_navigationViewFlyoutItemHeight = -1;
		public static double GetFlyoutItemHeight(this NavigationView nav)
		{
			if (s_navigationViewFlyoutItemHeight > 0)
				return s_navigationViewFlyoutItemHeight;
			return s_navigationViewFlyoutItemHeight = CalculateDoubleScaledSizeInLargeScreen(60);
		}

		static double s_navigationViewFlyoutIconColumnSize = -1;
		public static double GetFlyoutIconColumnSize(this NavigationView nav)
		{
			if (s_navigationViewFlyoutIconColumnSize > 0)
				return s_navigationViewFlyoutIconColumnSize;
			return s_navigationViewFlyoutIconColumnSize = CalculateDoubleScaledSizeInLargeScreen(40);
		}

		static double s_navigationViewFlyoutIconSize = -1;
		public static double GetFlyoutIconSize(this NavigationView nav)
		{
			if (s_navigationViewFlyoutIconSize > 0)
				return s_navigationViewFlyoutIconSize;
			return s_navigationViewFlyoutIconSize = CalculateDoubleScaledSizeInLargeScreen(25);
		}

		static double s_navigationViewFlyoutMargin = -1;
		public static double GetFlyoutMargin(this NavigationView nav)
		{
			if (s_navigationViewFlyoutMargin > 0)
				return s_navigationViewFlyoutMargin;
			return s_navigationViewFlyoutMargin = CalculateDoubleScaledSizeInLargeScreen(10);
		}

		static double s_navigationViewFlyoutItemFontSize = -1;
		public static double GetFlyoutItemFontSize(this NavigationView nav)
		{
			if (s_navigationViewFlyoutItemFontSize > 0)
				return s_navigationViewFlyoutItemFontSize;
			return s_navigationViewFlyoutItemFontSize = CalculateDoubleScaledSizeInLargeScreen(25);
		}

		#endregion

		#region NavigationDrawer

		static double s_navigationDrawerRatio = -1;
		public static double GetFlyoutRatio(this NavigationDrawer drawer, int width, int height)
		{
			return s_navigationDrawerRatio = (width > height) ? 0.4 : 0.83;
		}
		#endregion

		#region ShellMoreToolbar

		static double s_shellMoreToolBarIconPadding = -1;
		public static double GetIconPadding(this ShellMoreToolbar self)
		{
			if (s_shellMoreToolBarIconPadding > 0)
				return s_shellMoreToolBarIconPadding;
			return s_shellMoreToolBarIconPadding = CalculateDoubleScaledSizeInLargeScreen(15);
		}

		static double s_shellMoreToolBarIconSize = -1;
		public static double GetIconSize(this ShellMoreToolbar self)
		{
			if (s_shellMoreToolBarIconSize > 0)
				return s_shellMoreToolBarIconSize;
			return s_shellMoreToolBarIconSize = CalculateDoubleScaledSizeInLargeScreen(30);
		}

		#endregion

		public static double GetPhysicalPortraitSizeInDP()
		{
			var screenSize = Forms.PhysicalScreenSize;
			return Math.Min(screenSize.Width, screenSize.Height);
		}

		static double CalculateDoubleScaledSizeInLargeScreen(double size)
		{
			if (Forms.DisplayResolutionUnit.UseVP)
				return size;

			if (!Forms.DisplayResolutionUnit.UseDeviceScale && GetPhysicalPortraitSizeInDP() > 1000)
			{
				size *= 2.5;
			}

			if (!Forms.DisplayResolutionUnit.UseDP)
			{
				size = Forms.ConvertToPixel(size);
			}
			return size;
		}

	}
}