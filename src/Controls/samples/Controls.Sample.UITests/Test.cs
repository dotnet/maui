using System.Collections.Generic;

namespace Maui.Controls.Sample
{
	public static class Test
	{
		public enum Features
		{
			Binding,
			XAML,
			Maps
		}

		public enum Views
		{
			Label,
			TableView,
			SwitchCell,
			ViewCell,
			Image,
			ListView,
			ScrollView,
			Switch,
			Button,
			TextCell,
			Entry,
			SearchBar,
			ImageCell,
			EntryCell,
			Editor,
			DatePicker,
			CheckBox,
			SwipeView,
			RadioButton
		}

		public enum Layouts
		{
			StackLayout,
			Grid
		}

		public enum Pages
		{
			NavigationPage,
			FlyoutPage,
			TabbedPage,
			ContentPage,
			CarouselPage
		}

		public enum Button
		{
			Clicked,
			Command,
			Text,
			TextColor,
			Font,
			BorderWidth,
			BorderColor,
			BorderRadius,
			Image,
			Padding,
			Pressed,
			LineBreakMode
		}

		public enum VisualElement
		{
			IsEnabled,
			Navigation,
			InputTransparent,
			NotInputTransparent,
			Layout,
			X,
			Y,
			AnchorX,
			AnchorY,
			TranslationX,
			TranslationY,
			Width,
			Height,
			Bounds,
			Rotation,
			RotationX,
			RotationY,
			Scale,
			IsVisible,
			Opacity,
			BackgroundColor,
			Background,
			IsFocused,
			Focus,
			Unfocus,
			Focused,
			Unfocused,
			Default
		}

		public enum Cell
		{
			Tapped,
			Appearing,
			Disappearing,
			IsEnabled,
			RenderHeight,
			Height,
			ContextActions,
		}

		public enum EntryCell
		{
			Completed,
			Text,
			Label,
			Placeholder,
			LabelColor,
			Keyboard,
			HorizontalTextAlignment,
		}

		public enum TextCell
		{
			Command,
			Text,
			Detail,
			TextColor,
			DetailColor,
		}

		public enum ImageCell
		{
			ImageSource,
		}

		public enum GestureRecognizer
		{
			Parent,
		}

		public enum Binding
		{
			Path,
			Converter,
			ConverterParameter,
			Create,
		}

		public enum TapGestureRecognizer
		{
			Tapped,
			Command,
			CommandParameter,
			NumberOfTapsRequired,
			TappedCallBack,
			TappedCallBackParameter
		}

		public enum Device
		{
			OS,
			Idiom,
			OnPlatform,
			OnPlatformGeneric,
			OpenUri,
			BeginInvokeOnMainThread,
			StartTimer,
			Styles
		}

		public enum ValueChangedEventArgs
		{
			OldValue,
			NewValue,
		}

		public enum View
		{
			GestureRecognizers,
		}

		public enum Page
		{
			BackgroundImage,
			ToolbarItems,
			IsBusy,
			DisplayAlert,
			DisplayAlertAccept,
			DisplayActionSheet,
			Title,
			Icon,
			Appearing,
			Disappearing,
		}

		public enum NavigationPage
		{
			GetBackButtonTitle,
			SetBackButtonTitle,
			GetHasNavigationBar,
			SetHasNavigationBar,
			Tint,
			BarBackgroundColor,
			BarTextColor,
			BarGetTitleIcon,
			BarSetTitleIcon,
			Popped,
			PushAsync,
			PopAsync,
			PopToRootAsync,
		}

		public enum MultiPage
		{
			CurrentPageChanged,
			CurrentPagesChanged,
			ItemsSource,
			ItemTemplate,
			SelectedItem,
			CurrentPage,
			Children,
		}

		public enum MenuItem
		{
			Text,
			Command,
			IsDestructive,
			Icon,
			Clicked,
			IsEnabled
		}

		public enum ToolbarItem
		{
			Activated,
			Name,
			Order,
			Priority
		}

		public enum SwitchCell
		{
			OnChanged,
			On,
			Text,
		}

		public enum ViewCell
		{
			View,
		}

		public enum ListView
		{
			ItemAppearing,
			ItemDisappearing,
			ItemSelected,
			ItemTapped,
			SelectedItem,
			HasUnevenRows,
			RowHeight,
			GroupHeaderTemplate,
			IsGroupingEnabled,
			GroupDisplayBinding,
			GroupShortNameBinding,
			ScrollTo,
			Scrolled,
			FastScroll,
			RefreshControlColor,
			ScrollBarVisibility
		}

		public enum TableView
		{
			Root,
			Intent,
			RowHeight,
			HasUnevenRows,
			TableSection
		}

		public enum TableSectionBase
		{
			Title,
		}

		public enum Layout
		{
			IsClippedToBounds,
			Padding,
			RaiseChild,
			LowerChild,
			GenericChildren,
		}

		public enum AbsoluteLayout
		{
			Children,
			SetLayoutFlags,
			SetLayoutBounds,
		}

		public enum ActivityIndicator
		{
			IsRunning,
			Color,
		}

		public enum ContentView
		{
			View,
		}

		public enum DatePicker
		{
			DateSelected,
			Format,
			Date,
			MinimumDate,
			MaximumDate,
			Focus,
			IsVisible,
			TextColor,
			FontAttributes,
			FontFamily,
			FontSize
		}

		public enum InputView
		{
			Keyboard,
			MaxLength,
		}

		public enum Editor
		{
			Completed,
			TextChanged,
			Placeholder,
			PlaceholderColor,
			Text,
			TextColor,
			FontAttributes,
			FontFamily,
			FontSize,
			MaxLength,
			IsReadOnly
		}

		public enum Entry
		{
			Completed,
			TextChanged,
			Placeholder,
			IsPassword,
			Text,
			TextColor,
			HorizontalTextAlignmentStart,
			HorizontalTextAlignmentCenter,
			HorizontalTextAlignmentEnd,
			HorizontalTextAlignmentPlaceholderStart,
			HorizontalTextAlignmentPlaceholderCenter,
			HorizontalTextAlignmentPlaceholderEnd,
			VerticalTextAlignmentStart,
			VerticalTextAlignmentCenter,
			VerticalTextAlignmentEnd,
			VerticalTextAlignmentPlaceholderStart,
			VerticalTextAlignmentPlaceholderCenter,
			VerticalTextAlignmentPlaceholderEnd,
			FontAttributes,
			FontFamily,
			FontSize,
			PlaceholderColor,
			TextDisabledColor,
			PlaceholderDisabledColor,
			PasswordColor,
			MaxLength,
			IsReadOnly,
			IsPasswordNumeric,
			ClearButtonVisibility
		}

		public enum Frame
		{
			OutlineColor,
			HasShadow,
			Content,
			CornerRadius
		}

		public enum Image
		{
			Source,
			Aspect,
			IsOpaque,
			IsLoading,
			AspectFill,
			AspectFit,
			Fill
		}

		public enum ImageButton
		{
			Source,
			Aspect,
			IsOpaque,
			IsLoading,
			AspectFill,
			AspectFit,
			Fill,
			BorderColor,
			CornerRadius,
			BorderWidth,
			Clicked,
			Command,
			Image,
			Pressed,
			Padding
		}

		public enum ImageSource
		{
			FromFile,
			FromStream,
			FromResource,
			FromUri,
			Cancel,
		}

		public enum UriImageSource
		{
			Uri,
			CachingEnabled,
			CacheValidity,
		}

		public enum Keyboard
		{
			Create,
			Default,
			Email,
			Text,
			Url,
			Numeric,
			Telephone,
			Chat,
		}

		public enum Label
		{
			TextColor,
			Text,
			Padding,
			FormattedText,
			FontAttibutesBold,
			FontAttributesItalic,
			TextDecorationUnderline,
			TextDecorationStrike,
			FontNamedSizeMicro,
			FontNamedSizeSmall,
			FontNamedSizeMedium,
			FontNamedSizeLarge,
			LineBreakModeNoWrap,
			LineBreakModeWordWrap,
			LineBreakModeCharacterWrap,
			LineBreakModeHeadTruncation,
			LineBreakModeTailTruncation,
			LineBreakModeMiddleTruncation,
			HorizontalTextAlignmentStart,
			HorizontalTextAlignmentCenter,
			HorizontalTextAlignmentEnd,
			VerticalTextAlignmentStart,
			VerticalTextAlignmentCenter,
			VerticalTextAlignmentEnd,
			MaxLines,
			HtmlTextType,
			BrokenHtmlTextType,
			HtmlTextTypeMultipleLines,
			HtmlTextLabelProperties,
			TextTypeToggle,
		}

		public enum ProgressBar
		{
			Progress,
			ProgressColor
		}

		public enum RefreshView
		{
			RefreshColor
		}

		public enum RelativeLayout
		{
			Children,
			SetBoundsConstraint
		}

		public enum ScrollView
		{
			ContentSize,
			Orientation,
			Content
		}

		public enum SearchBar
		{
			SearchButtonPressed,
			TextChanged,
			SearchCommand,
			Text,
			PlaceHolder,
			CancelButtonColor,
			FontAttributes,
			FontFamily,
			FontSize,
			TextAlignmentStart,
			TextAlignmentCenter,
			TextAlignmentEnd,
			TextVerticalAlignmentStart,
			TextVerticalAlignmentCenter,
			TextVerticalAlignmentEnd,
			PlaceholderAlignmentStart,
			PlaceholderAlignmentCenter,
			PlaceholderAlignmentEnd,
			PlaceholderVerticalAlignmentStart,
			PlaceholderVerticalAlignmentCenter,
			PlaceholderVerticalAlignmentEnd,
			TextColor,
			PlaceholderColor
		}

		public enum Slider
		{
			Minimum,
			Maximum,
			Value,
			MinimumTrackColor,
			MaximumTrackColor,
			ThumbColor,
			ThumbImage,
			DragStarted,
			DragCompleted
		}

		public enum StackLayout
		{
			Orientation,
			Spacing
		}

		public enum Stepper
		{
			Maximum,
			Minimum,
			Value,
			Increment
		}

		public enum Switch
		{
			IsToggled,
			OnColor,
			ThumbColor
		}

		public enum SwipeView
		{
			RightItems,
			TopItems,
			BottomItems
		}

		public enum CheckBox
		{
			IsChecked,
			CheckedColor,
			UncheckedColor
		}

		public enum RadioButton
		{
			IsChecked,
			ButtonSource,
		}

		public enum TimePicker
		{
			Format,
			Time,
			Focus,
			TextColor,
			FontAttributes,
			FontFamily,
			FontSize
		}

		public enum WebView
		{
			UrlWebViewSource,
			HtmlWebViewSource,
			LoadHtml,
			MixedContentDisallowed,
			MixedContentAllowed,
			JavaScriptAlert,
			EvaluateJavaScript,
			EnableZoomControls,
			DisplayZoomControls
		}

		public enum UrlWebViewSource
		{
			Url
		}

		public enum HtmlWebViewSource
		{
			BaseUrl,
			Html
		}

		public enum Grid
		{
			Children,
			SetRow,
			SetRowSpan,
			SetColumn,
			SetColumnSpan,
			RowSpacing,
			ColumnSpacing,
			ColumnDefinitions,
			RowDefinitions
		}

		public enum ContentPage
		{
			Content
		}

		public enum Picker
		{
			Title,
			TitleColor,
			Items,
			SelectedIndex,
			Focus,
			HorizontalTextAlignment,
			VerticalTextAlignment,
			TextColor,
			FontAttributes,
			FontFamily,
			FontSize
		}

		public enum FileImageSource
		{
			File,
			Cancel
		}

		public enum StreamImageSource
		{
			Stream
		}

		public enum OnPlatform
		{
			WinPhone,
			Android,
			iOS
		}

		public enum OnIdiom
		{
			Phone,
			Tablet
		}

		public enum Span
		{
			Text,
			ForeGroundColor,
			BackgroundColor,
			Font,
			PropertyChanged
		}

		public enum FormattedString
		{
			ToStringOverride,
			Spans,
			SpanTapped,
			PropertyChanged
		}

		public enum BoxView
		{
			Color,
			CornerRadius
		}

		public enum CarouselView
		{
			CurrentItem,
			IsSwipeEnabled,
			IsScrollAnimated,
			NumberOfSideItems,
			PeekAreaInsets,
			Position,
			IsBounceEnabled
		}

		public enum ImageLoading
		{
			FromBundleSvg,
			FromBundlePng,
			FromBundleJpg,
			FromBundleGif,
		}

		public enum InputTransparency
		{
			Default,
			IsFalse,
			IsTrue,
			TransLayoutOverlay,
			TransLayoutOverlayWithButton,
			CascadeTransLayoutOverlay,
			CascadeTransLayoutOverlayWithButton,
		}

		public static class InputTransparencyMatrix
		{
			// this is both for color diff and cols
			const bool truee = true;

			public static readonly IReadOnlyDictionary<(bool RT, bool RC, bool NT, bool NC, bool T), (bool Clickable, bool PassThru)> States =
				new Dictionary<(bool, bool, bool, bool, bool), (bool, bool)>
				{
					[(truee, truee, truee, truee, truee)] = (false, truee),
					[(truee, truee, truee, truee, false)] = (false, truee),
					[(truee, truee, truee, false, truee)] = (false, truee),
					[(truee, truee, truee, false, false)] = (false, truee),
					[(truee, truee, false, truee, truee)] = (false, truee),
					[(truee, truee, false, truee, false)] = (false, truee),
					[(truee, truee, false, false, truee)] = (false, truee),
					[(truee, truee, false, false, false)] = (false, truee),
					[(truee, false, truee, truee, truee)] = (false, truee),
					[(truee, false, truee, truee, false)] = (false, truee),
					[(truee, false, truee, false, truee)] = (false, truee),
					[(truee, false, truee, false, false)] = (truee, false),
					[(truee, false, false, truee, truee)] = (false, false),
					[(truee, false, false, truee, false)] = (truee, false),
					[(truee, false, false, false, truee)] = (false, false),
					[(truee, false, false, false, false)] = (truee, false),
					[(false, truee, truee, truee, truee)] = (false, false),
					[(false, truee, truee, truee, false)] = (false, false),
					[(false, truee, truee, false, truee)] = (false, false),
					[(false, truee, truee, false, false)] = (truee, false),
					[(false, truee, false, truee, truee)] = (false, false),
					[(false, truee, false, truee, false)] = (truee, false),
					[(false, truee, false, false, truee)] = (false, false),
					[(false, truee, false, false, false)] = (truee, false),
					[(false, false, truee, truee, truee)] = (false, false),
					[(false, false, truee, truee, false)] = (false, false),
					[(false, false, truee, false, truee)] = (false, false),
					[(false, false, truee, false, false)] = (truee, false),
					[(false, false, false, truee, truee)] = (false, false),
					[(false, false, false, truee, false)] = (truee, false),
					[(false, false, false, false, truee)] = (false, false),
					[(false, false, false, false, false)] = (truee, false),
				};

			public static string GetKey(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans, bool isClickable, bool isPassThru) =>
				$"Root{(rootTrans ? "Trans" : "")}{(rootCascade ? "Cascade" : "")}Nested{(nestedTrans ? "Trans" : "")}{(nestedCascade ? "Cascade" : "")}Control{(trans ? "Trans" : "")}Is{(isClickable ? "" : "Not")}ClickableIs{(isPassThru ? "" : "Not")}PassThru";
		}
	}
}