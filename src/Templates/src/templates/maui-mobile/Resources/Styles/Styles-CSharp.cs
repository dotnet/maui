namespace MauiApp._1.Resources.Styles;

using System.Reflection;
using Microsoft.Maui.Controls.Shapes;

public class AppStyles : ResourceDictionary
{
	// AppThemeBinding is internal; reflect once to construct it from code.
	private static readonly Type s_appThemeBindingType =
		typeof(Application).Assembly.GetType("Microsoft.Maui.Controls.AppThemeBinding", throwOnError: true)!;
	private static readonly PropertyInfo s_lightProperty = s_appThemeBindingType.GetProperty("Light")!;
	private static readonly PropertyInfo s_darkProperty = s_appThemeBindingType.GetProperty("Dark")!;

	public AppStyles() : this(new AppColors())
	{
	}

	public AppStyles(AppColors colors)
	{
		var primary = (Color)colors["Primary"];
		var primaryDark = (Color)colors["PrimaryDark"];
		var primaryDarkText = (Color)colors["PrimaryDarkText"];
		var secondary = (Color)colors["Secondary"];
		var secondaryDarkText = (Color)colors["SecondaryDarkText"];
		var white = (Color)colors["White"];
		var black = (Color)colors["Black"];
		var magenta = (Color)colors["Magenta"];
		var midnightBlue = (Color)colors["MidnightBlue"];
		var offBlack = (Color)colors["OffBlack"];
		var gray100 = (Color)colors["Gray100"];
		var gray200 = (Color)colors["Gray200"];
		var gray300 = (Color)colors["Gray300"];
		var gray400 = (Color)colors["Gray400"];
		var gray500 = (Color)colors["Gray500"];
		var gray600 = (Color)colors["Gray600"];
		var gray900 = (Color)colors["Gray900"];
		var gray950 = (Color)colors["Gray950"];

		static BindingBase Theme(object light, object dark)
		{
			var binding = (BindingBase)Activator.CreateInstance(s_appThemeBindingType, nonPublic: true)!;
			s_lightProperty.SetValue(binding, light);
			s_darkProperty.SetValue(binding, dark);
			return binding;
		}

		static Setter Set(BindableProperty property, object value) => new() { Property = property, Value = value };

		static VisualState State(string name, params Setter[] setters)
		{
			var state = new VisualState { Name = name };
			foreach (var s in setters)
				state.Setters.Add(s);
			return state;
		}

		static Setter CommonStates(params VisualState[] states)
		{
			var group = new VisualStateGroup { Name = "CommonStates" };
			foreach (var s in states)
				group.States.Add(s);
			var list = new VisualStateGroupList { group };
			return new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = list };
		}

		static Style Style(Type target, params Setter[] setters)
		{
			var style = new Style(target);
			foreach (var s in setters)
				style.Setters.Add(s);
			return style;
		}

		// ActivityIndicator
		Add(Style(typeof(ActivityIndicator),
			Set(ActivityIndicator.ColorProperty, Theme(primary, white))));

		// IndicatorView
		Add(Style(typeof(IndicatorView),
			Set(IndicatorView.IndicatorColorProperty, Theme(gray200, gray500)),
			Set(IndicatorView.SelectedIndicatorColorProperty, Theme(gray950, gray100))));

		// Border
		Add(Style(typeof(Border),
			Set(Border.StrokeProperty, Theme(gray200, gray500)),
			Set(Border.StrokeShapeProperty, new Rectangle()),
			Set(Border.StrokeThicknessProperty, 1.0)));

		// BoxView
		Add(Style(typeof(BoxView),
			Set(BoxView.BackgroundColorProperty, Theme(gray950, gray200))));

		// Button
		Add(Style(typeof(Button),
			Set(Button.TextColorProperty, Theme(white, primaryDarkText)),
			Set(Button.BackgroundColorProperty, Theme(primary, primaryDark)),
			Set(Button.FontFamilyProperty, "OpenSansRegular"),
			Set(Button.FontSizeProperty, 14.0),
			Set(Button.BorderWidthProperty, 0.0),
			Set(Button.CornerRadiusProperty, 8),
			Set(Button.PaddingProperty, new Thickness(14, 10)),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(Button.TextColorProperty, Theme(gray950, gray200)),
					Set(Button.BackgroundColorProperty, Theme(gray200, gray600))),
				State("PointerOver"))));

		// CheckBox
		Add(Style(typeof(CheckBox),
			Set(CheckBox.ColorProperty, Theme(primary, white)),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(CheckBox.ColorProperty, Theme(gray300, gray600))))));

		// DatePicker
		Add(Style(typeof(DatePicker),
			Set(DatePicker.TextColorProperty, Theme(gray900, white)),
			Set(DatePicker.BackgroundColorProperty, Colors.Transparent),
			Set(DatePicker.FontFamilyProperty, "OpenSansRegular"),
			Set(DatePicker.FontSizeProperty, 14.0),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(DatePicker.TextColorProperty, Theme(gray200, gray500))))));

		// Editor
		Add(Style(typeof(Editor),
			Set(Editor.TextColorProperty, Theme(black, white)),
			Set(Editor.BackgroundColorProperty, Colors.Transparent),
			Set(Editor.FontFamilyProperty, "OpenSansRegular"),
			Set(Editor.FontSizeProperty, 14.0),
			Set(Editor.PlaceholderColorProperty, Theme(gray200, gray500)),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(Editor.TextColorProperty, Theme(gray300, gray600))))));

		// Entry
		Add(Style(typeof(Entry),
			Set(Entry.TextColorProperty, Theme(black, white)),
			Set(Entry.BackgroundColorProperty, Colors.Transparent),
			Set(Entry.FontFamilyProperty, "OpenSansRegular"),
			Set(Entry.FontSizeProperty, 14.0),
			Set(Entry.PlaceholderColorProperty, Theme(gray200, gray500)),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(Entry.TextColorProperty, Theme(gray300, gray600))))));

		// ImageButton
		Add(Style(typeof(ImageButton),
			Set(VisualElement.OpacityProperty, 1.0),
			Set(ImageButton.BorderColorProperty, Colors.Transparent),
			Set(ImageButton.BorderWidthProperty, 0.0),
			Set(ImageButton.CornerRadiusProperty, 0),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(VisualElement.OpacityProperty, 0.5)),
				State("PointerOver"))));

		// Label
		Add(Style(typeof(Label),
			Set(Label.TextColorProperty, Theme(black, white)),
			Set(Label.BackgroundColorProperty, Colors.Transparent),
			Set(Label.FontFamilyProperty, "OpenSansRegular"),
			Set(Label.FontSizeProperty, 14.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(Label.TextColorProperty, Theme(gray300, gray600))))));

		// Label - Headline
		Add("Headline", Style(typeof(Label),
			Set(Label.TextColorProperty, Theme(midnightBlue, white)),
			Set(Label.FontSizeProperty, 32.0),
			Set(View.HorizontalOptionsProperty, LayoutOptions.Center),
			Set(Label.HorizontalTextAlignmentProperty, TextAlignment.Center)));

		// Label - SubHeadline
		Add("SubHeadline", Style(typeof(Label),
			Set(Label.TextColorProperty, Theme(midnightBlue, white)),
			Set(Label.FontSizeProperty, 24.0),
			Set(View.HorizontalOptionsProperty, LayoutOptions.Center),
			Set(Label.HorizontalTextAlignmentProperty, TextAlignment.Center)));

		// Picker
		Add(Style(typeof(Picker),
			Set(Picker.TextColorProperty, Theme(gray900, white)),
			Set(Picker.TitleColorProperty, Theme(gray900, gray200)),
			Set(Picker.BackgroundColorProperty, Colors.Transparent),
			Set(Picker.FontFamilyProperty, "OpenSansRegular"),
			Set(Picker.FontSizeProperty, 14.0),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(Picker.TextColorProperty, Theme(gray300, gray600)),
					Set(Picker.TitleColorProperty, Theme(gray300, gray600))))));

		// ProgressBar
		Add(Style(typeof(ProgressBar),
			Set(ProgressBar.ProgressColorProperty, Theme(primary, white)),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(ProgressBar.ProgressColorProperty, Theme(gray300, gray600))))));

		// RadioButton
		Add(Style(typeof(RadioButton),
			Set(RadioButton.BackgroundColorProperty, Colors.Transparent),
			Set(RadioButton.TextColorProperty, Theme(black, white)),
			Set(RadioButton.FontFamilyProperty, "OpenSansRegular"),
			Set(RadioButton.FontSizeProperty, 14.0),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(RadioButton.TextColorProperty, Theme(gray300, gray600))))));

		// RefreshView
		Add(Style(typeof(RefreshView),
			Set(RefreshView.RefreshColorProperty, Theme(gray900, gray200))));

		// SearchBar
		Add(Style(typeof(SearchBar),
			Set(SearchBar.TextColorProperty, Theme(gray900, white)),
			Set(SearchBar.PlaceholderColorProperty, gray500),
			Set(SearchBar.CancelButtonColorProperty, gray500),
			Set(SearchBar.BackgroundColorProperty, Colors.Transparent),
			Set(SearchBar.FontFamilyProperty, "OpenSansRegular"),
			Set(SearchBar.FontSizeProperty, 14.0),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(SearchBar.TextColorProperty, Theme(gray300, gray600)),
					Set(SearchBar.PlaceholderColorProperty, Theme(gray300, gray600))))));

		// SearchHandler
		Add(Style(typeof(SearchHandler),
			Set(SearchHandler.TextColorProperty, Theme(gray900, white)),
			Set(SearchHandler.PlaceholderColorProperty, gray500),
			Set(SearchHandler.BackgroundColorProperty, Colors.Transparent),
			Set(SearchHandler.FontFamilyProperty, "OpenSansRegular"),
			Set(SearchHandler.FontSizeProperty, 14.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(SearchHandler.TextColorProperty, Theme(gray300, gray600)),
					Set(SearchHandler.PlaceholderColorProperty, Theme(gray300, gray600))))));

		// Shadow
		Add(Style(typeof(Shadow),
			Set(Shadow.RadiusProperty, 15.0f),
			Set(Shadow.OpacityProperty, 0.5f),
			Set(Shadow.BrushProperty, Theme(new SolidColorBrush(white), new SolidColorBrush(white))),
			Set(Shadow.OffsetProperty, new Point(10, 10))));

		// Slider
		Add(Style(typeof(Slider),
			Set(Slider.MinimumTrackColorProperty, Theme(primary, white)),
			Set(Slider.MaximumTrackColorProperty, Theme(gray200, gray600)),
			Set(Slider.ThumbColorProperty, Theme(primary, white)),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(Slider.MinimumTrackColorProperty, Theme(gray300, gray600)),
					Set(Slider.MaximumTrackColorProperty, Theme(gray300, gray600)),
					Set(Slider.ThumbColorProperty, Theme(gray300, gray600))))));

		// SwipeItem
		Add(Style(typeof(SwipeItem),
			Set(SwipeItem.BackgroundColorProperty, Theme(white, black))));

		// Switch
		Add(Style(typeof(Switch),
			Set(Switch.OnColorProperty, Theme(primary, white)),
			Set(Switch.ThumbColorProperty, white),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(Switch.OnColorProperty, Theme(gray300, gray600)),
					Set(Switch.ThumbColorProperty, Theme(gray300, gray600))),
				State("On",
					Set(Switch.OnColorProperty, Theme(secondary, gray200)),
					Set(Switch.ThumbColorProperty, Theme(primary, white))),
				State("Off",
					Set(Switch.ThumbColorProperty, Theme(gray400, gray500))))));

		// TimePicker
		Add(Style(typeof(TimePicker),
			Set(TimePicker.TextColorProperty, Theme(gray900, white)),
			Set(TimePicker.BackgroundColorProperty, Colors.Transparent),
			Set(TimePicker.FontFamilyProperty, "OpenSansRegular"),
			Set(TimePicker.FontSizeProperty, 14.0),
			Set(VisualElement.MinimumHeightRequestProperty, 44.0),
			Set(VisualElement.MinimumWidthRequestProperty, 44.0),
			CommonStates(
				State("Normal"),
				State("Disabled",
					Set(TimePicker.TextColorProperty, Theme(gray300, gray600))))));

		// Page
		var pageStyle = new Style(typeof(Page)) { ApplyToDerivedTypes = true };
		pageStyle.Setters.Add(Set(Page.PaddingProperty, new Thickness(0)));
		pageStyle.Setters.Add(Set(VisualElement.BackgroundColorProperty, Theme(white, offBlack)));
		Add(pageStyle);

		// Shell
		var shellStyle = new Style(typeof(Shell)) { ApplyToDerivedTypes = true };
		shellStyle.Setters.Add(Set(Shell.BackgroundColorProperty, Theme(white, offBlack)));
		shellStyle.Setters.Add(Set(Shell.ForegroundColorProperty, Theme(black, secondaryDarkText)));
		shellStyle.Setters.Add(Set(Shell.TitleColorProperty, Theme(black, secondaryDarkText)));
		shellStyle.Setters.Add(Set(Shell.DisabledColorProperty, Theme(gray200, gray950)));
		shellStyle.Setters.Add(Set(Shell.UnselectedColorProperty, Theme(gray200, gray200)));
		shellStyle.Setters.Add(Set(Shell.NavBarHasShadowProperty, false));
		shellStyle.Setters.Add(Set(Shell.TabBarBackgroundColorProperty, Theme(white, black)));
		shellStyle.Setters.Add(Set(Shell.TabBarForegroundColorProperty, Theme(magenta, white)));
		shellStyle.Setters.Add(Set(Shell.TabBarTitleColorProperty, Theme(magenta, white)));
		shellStyle.Setters.Add(Set(Shell.TabBarUnselectedColorProperty, Theme(gray900, gray200)));
		Add(shellStyle);

		// NavigationPage
		Add(Style(typeof(NavigationPage),
			Set(NavigationPage.BarBackgroundColorProperty, Theme(white, offBlack)),
			Set(NavigationPage.BarTextColorProperty, Theme(gray200, white)),
			Set(NavigationPage.IconColorProperty, Theme(gray200, white))));

		// TabbedPage
		Add(Style(typeof(TabbedPage),
			Set(TabbedPage.BarBackgroundColorProperty, Theme(white, gray950)),
			Set(TabbedPage.BarTextColorProperty, Theme(magenta, white)),
			Set(TabbedPage.UnselectedTabColorProperty, Theme(gray200, gray950)),
			Set(TabbedPage.SelectedTabColorProperty, Theme(gray950, gray200))));
	}
}
