using Tizen.Common;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class ThemeConstants
	{
		#region Common
		public class Common
		{
			public class Parts
			{
				public const string Text = "elm.text";
				public const string Content = "elm.swallow.content";
			}

			public class ColorClass
			{
				public const string BackGround = "bg";
				public const string Opacity = "opacity";
			}

			public class Resource
			{
				public class Mobile
				{
					public const double BaseScale = 2.6;
				}
				public class TV
				{
					public const double BaseScale = 2.0;
				}
				public class Watch
				{
					public const double BaseScale = 1.3;
				}
				public class Refrigerator
				{
					public const double BaseScale = 1.0;
				}
				public class Iot
				{
					public const double BaseScale = 1.8;
				}
			}
		}
		#endregion

		#region Layout
		public class Layout
		{
			public class Parts
			{
				public const string Text = Common.Parts.Text;
				public const string Content = Common.Parts.Content;
				public const string Background = "elm.swallow.background";
				public const string Overlay = "elm.swallow.overlay";
			}
		}
		#endregion

		#region Button
		public class Button
		{
			public class Styles
			{
				public const string Default = "default";
				public const string Circle = "circle";
				public const string Bottom = "bottom";
				public const string Text = "textbutton";
				public const string SelectMode = "select_mode";
				public const string EditFieldClear = "editfield_clear";
				public const string Transparent = "transparent";
				public const string Popup = "popup";
				public const string NavigationTitleLeft = "naviframe/title_left";
				public const string NavigationTitleRight = "naviframe/title_right";
				public const string NavigationBack = "naviframe/back_btn/default";
				public const string NavigationDrawers = "naviframe/drawers";

				public class Watch
				{
					public const string PopupLeft = "popup/circle/left_delete";
					public const string PopupRight = "popup/circle/right_check";
					public const string Text = "textbutton";
				}
			}

			public class Parts
			{
				public const string Icon = "elm.swallow.content";
				public const string Button = "elm.swallow.button";
			}

			public class ColorClass
			{
				public const string Icon = "icon";
				public const string IconPressed = "icon_pressed";
				public const string Effect = "effect";
				public const string EffectPressed = "effect_pressed";
			}

			public class Signals
			{
				public const string ElementaryCode = "elm";
				public const string TextVisibleState = "elm,state,text,visible";
				public const string TextHiddenState = "elm,state,text,hidden";
			}
		}
		#endregion

		#region Entry
		public class Entry
		{
			public class Parts
			{
				public const string PlaceHolderText = "elm.guide";
			}

			public class Signals
			{
				public const string SelectionChanged = "selection,changed";
				public const string SelectionCleared = "selection,cleared";
			}
		}
		#endregion

		#region GenListItem
		public class GenListItem
		{
			public class ColorClass
			{
				public const string BottomLine = "bottomline";
				public const string Background = Common.ColorClass.BackGround;
			}

			public class Signals
			{
				public class TV
				{
					public const string SinglelineIconTextTheme = "theme,singleline,icon,text";
					public const string SinglelineTextIconTheme = "theme,singleline,text,1icon";
				}
			}
		}
		#endregion

		#region NaviItem
		public class NaviItem
		{
			public class Styles
			{
				public const string Default = "default";
				public const string NavigationBar = "navigationbar";

				public class TV
				{
					public const string TabBar = "tabbar";
				}
			}

			public class Parts
			{
				public const string Title = "default";
				public const string BackButton = "elm.swallow.prev_btn";
				public const string LeftToolbarButton = "title_left_btn";
				public const string RightToolbarButton = "title_right_btn";
				public const string NavigationBar = "navigationbar";
			}

			public class ColorClass
			{
				public const string TextUnderIconPressed = "text_under_icon_pressed";
				public const string TextUnderIconSelected = "text_under_icon_selected";
			}
		}
		#endregion

		#region GenList
		public static class GenList
		{
			public class Styles
			{
				public const string Solid = "solid/default";
			}
		}
		#endregion

		#region GenItemClass
		public static class GenItemClass
		{
			public class Styles
			{
				public const string Default = "default";
				public const string Full = "full";
				public const string DoubleLabel = "double_label";
				public const string GroupIndex = "group_index";

				public class Watch
				{
					public const string Padding = "padding";
					public const string SingleText = "1text";
					public const string Text1Icon1 = "1text.1icon.1";
					public const string TwoText1Icon1 = "2text.1icon.1";
					public const string Icon2Text = "2text.1icon";//"1icon_2text"
					public const string FullEffectOff = "full_effect_off";
					public const string FullOff = "full_off";
				}
			}

			public class Parts
			{
				public const string Text = Common.Parts.Text;
				public const string Content = Common.Parts.Content;
				public const string Icon = "elm.swallow.icon";
				public const string End = "elm.swallow.end";
				public const string SubText = "elm.text.sub";
				public const string EndText = "elm.text.end";
				public const string Ignore = "null";

				public class Watch
				{
					public const string Icon = "elm.icon";
					public const string Text = "elm.text.1";
				}
			}
		}
		#endregion

		#region Popup
		public class Popup
		{
			public class Styles
			{
				public class Watch
				{
					public const string Circle = "circle";
					public const string ToastCircle = "toast/circle";
				}
			}

			public class Parts
			{
				public const string Title = "title,text";
				public const string Button1 = "button1";
				public const string Button2 = "button2";
				public const string Button3 = "button3";

				public class Watch
				{
					public const string ToastIcon = "toast,icon";
				}
			}

			public class ColorClass
			{
				public const string Title = "text_maintitle";
				public const string TitleBackground = "bg_title";
				public const string ContentBackground = "bg_content";

				public class TV
				{
					public const string Title = "text_title";
				}
			}
		}
		#endregion

		#region ContextPopup
		public class ContextPopup
		{
			public class Styles
			{
				public const string SelectMode = "select_mode";
			}
		}
		#endregion

		#region DateTimeSelector
		public class DateTimeSelector
		{
			public class Styles
			{
				public const string TimeLayout = "time_layout";
				public const string DateLayout = "date_layout";
			}
		}
		#endregion

		#region CircleDateTimeSelector
		public class CircleDateTimeSelector
		{
			public class Styles
			{
				public const string CircleDatePicker = "datepicker/circle";
				public const string CircleTimePicker = "timepicker/circle";
			}
		}
		#endregion

		#region CircleSpinner
		public class CircleSpinner
		{
			public class Styles
			{
				public const string Circle = "circle";
			}

			public class Signals
			{
				public static readonly string ShowList = DotnetUtil.TizenAPIVersion == 4 ? "genlist,show" : "list,show";
				public static readonly string HideList = DotnetUtil.TizenAPIVersion == 4 ? "genlist,hide" : "list,hide";
			}
		}
		#endregion

		#region Toolbar
		public class Toolbar
		{
			public class Styles
			{
				public const string Default = "default";
				public const string NavigationBar = "navigationbar";
				public const string Tabbar = "tabbar";
				public const string Material = "material";

				public class TV
				{
					public const string TabbarWithTitle = "tabbar_with_title";
				}
			}
		}
		#endregion

		#region ToolbarItem
		public class ToolbarItem
		{
			public class Parts
			{
				public const string Icon = "elm.swallow.icon";
			}

			public class ColorClass
			{
				public const string Background = Common.ColorClass.BackGround;
				public const string Icon = "icon";
				public const string IconPressed = "icon_pressed";
				public const string IconSelected = "icon_selected";
				public const string Text = "text";
				public const string TextPressed = "text_pressed";
				public const string TextSelected = "text_selected";
				public const string TextUnderIcon = "text_under_icon";
				public const string TextUnderIconPressed = "text_under_icon_pressed";
				public const string TextUnderIconSelected = "text_under_icon_selected";
				public const string Underline = "underline";
			}
		}
		#endregion

		#region ProgressBar
		public class ProgressBar
		{
			public class Styles
			{
				public const string Default = "default";
				public const string Pending = "pending";
				public const string Small = "process_small";
				public const string Large = "process_large";

				public class Watch
				{
					public const string PopupSmall = "process/popup/small";
				}
			}

			public class ColorClass
			{
				public static readonly EColor Default = new EColor(129, 198, 255);
			}
		}
		#endregion

		#region Frame
		public class Frame
		{
			public class ColorClass
			{
				public static readonly EColor DefaultBorderColor = Device.Idiom == TargetIdiom.TV || Device.Idiom == TargetIdiom.Watch ? EColor.Gray : EColor.Black;
				public static readonly EColor DefaultShadowColor = EColor.FromRgba(80, 80, 80, 50);
			}
		}
		#endregion

		#region Panes
		public class Panes
		{
			public class Parts
			{
				public const string Left = "left";
				public const string Right = "right";
			}
		}
		#endregion

		#region Scroller
		public class Scroller
		{
			public class Signals
			{
				public const string StartScrollAnimation = "scroll,anim,start";
				public const string StopScrollAnimation = "scroll,anim,stop";
			}
		}
		#endregion

		#region Background
		public class Background
		{
			public class Parts
			{
				public const string Overlay = "overlay";
			}
		}
		#endregion

		#region Check
		public class Check
		{
			public class Styles
			{
				public const string Default = "default";
				public const string Toggle = "toggle";
				public const string Favorite = "favorite";
				public const string OnOff = "on&off";
				public const string Small = "small";

				public class Watch
				{
					public const string ListSelectMode = "genlist/select_mode";
				}
			}

			public class ColorClass
			{
				public const string BackgroundOn = "bg_on";
				public const string Stroke = "stroke";

				public class TV
				{
					public const string SliderOn = "slider_on";
					public const string SliderFocusedOn = "slider_focused_on";
				}

				public class Watch
				{
					public const string OuterBackgroundOn = "outer_bg_on";
					public const string OuterBackgroundOnPressed = "outer_bg_on_pressed";
					public const string CheckOn = "check_on";
					public const string CheckOnPressed = "check_on_pressed";
				}
			}
		}
		#endregion

		#region Radio
		public class Radio
		{
			public class Signals
			{
				public const string ElementaryCode = "elm";
				public const string TextVisibleState = "elm,state,text,visible";
				public const string TextHiddenState = "elm,state,text,hidden";
			}
		}
		#endregion

		#region Index
		public class Index
		{
			public class Styles
			{
				public const string PageControl = "pagecontrol";
				public const string Circle = "circle";
				public const string Thumbnail = "thumbnail";
			}
		}
		#endregion

		#region IndexItem
		public class IndexItem
		{
			public class Styles
			{
				public const string EvenItemPrefix = "item/even_";
				public const string OddItemPrefix = "item/odd_";
			}
		}
		#endregion

		#region Slider
		public class Slider
		{
			public class ColorClass
			{
				public const string Bar = "bar";
				public const string Background = "bg";
				public const string Handler = "handler";
				public const string BarPressed = "bar_pressed";
				public const string HandlerPressed = "handler_pressed";
			}
		}
		#endregion

		#region RefreshView
		public class RefreshView
		{
			public class Resources
			{
				public const int IconSize = 48;
				public const string IconPath = "Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Resource.refresh_48dp.png";
			}

			public class ColorClass
			{
				public static readonly Color DefaultColor = Color.FromArgb("#6200EE");
			}
		}
		#endregion

		#region Span
		public class Span
		{
			public class ColorClass
			{
				public static readonly EColor DefaultUnderLineColor = EColor.Black;
			}
		}
		#endregion

		#region Cell
		public class EntryCell
		{
			public class Resources
			{
				public const double DefaultHeight = 65;
			}

			public class ColorClass
			{
				public static readonly EColor DefaultLabelColor = EColor.Black;
			}
		}

		public class ImageCell
		{
			public class Resources
			{
				public const double DefaultHeight = 55;
			}
		}
		#endregion

		#region Shell
		public class Shell
		{
			public class Resources
			{
				// The source of icon resources is https://materialdesignicons.com/
				public const string MenuIcon = "Resource.menu.png";
				public const string BackIcon = "Resource.arrow_left.png";
				public const string DotsIcon = "Resource.dots_horizontal.png";

				public class Watch
				{
					public const int DefaultNavigationViewIconSize = 60;
					public const int DefaultDrawerTouchWidth = 50;
					public const int DefaultDrawerIconSize = 40;
					public const string DefaultDrawerIcon = "Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Resource.wc_visual_cue.png";
				}

				public class TV
				{
					public const string MenuIconCode = "\u2630";
					public const string BackIconCode = "\u2190";
					public const string DotsIconCode = "\u2026";
				}
			}

			public class ColorClass
			{
				public static readonly Color DefaultBackgroundColor = Color.FromRgb(33, 150, 243);
				public static readonly Color DefaultForegroundColor = Color.White;
				public static readonly Color DefaultTitleColor = Color.White;
				public static readonly EColor DefaultNavigationViewBackgroundColor = EColor.White;
				public static readonly EColor DefaultDrawerDimBackgroundColor = new EColor(0, 0, 0, 82);

				public class Watch
				{
					public static readonly EColor DefaultNavigationViewForegroundColor = EColor.Default;
					public static readonly EColor DefaultNavigationViewBackgroundColor = EColor.Black;
				}
			}
		}
		#endregion

		#region CarouselView
		public class CarouselView
		{
			public class ColorClass
			{
				public static readonly EColor DefaultFocusedColor = EColor.Transparent;
				public static readonly EColor DefaultSelectedColor = EColor.Transparent;
			}
		}
		#endregion

		#region MediaPlayer
		public class MediaPlayer
		{
			public class Resources
			{
				public const string PlayImagePath = "Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Resource.img_button_play.png";
				public const string PauseImagePath = "Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Resource.img_button_pause.png";
			}

			public class ColorClass
			{
				public static readonly Color DefaultProgressLabelColor = Color.FromArgb("#eeeeeeee");
				public static readonly Color DefaultProgressBarColor = Color.FromArgb($"#4286f4");
				public static readonly Color DefaultProgressAreaColor = Color.FromArgb("#80000000");
				public static readonly Color DefaultProgressAreaBackgroundColor = Color.FromArgb("#50000000");
			}
		}
		#endregion
	}
}
