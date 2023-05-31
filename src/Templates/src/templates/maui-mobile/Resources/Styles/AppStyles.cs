namespace MauiApp._1.Resources.Styles;

// TODO VisualStateManager for: Button, CheckBox, DatePicker, Editor, Entry, ImageButton, Label, Picker, ProgressBar, RadioButton, SearchBar, SearchHandler, Slider, Switch, TimePicker
public class AppStyles : ResourceDictionary
{
    public static Style ActivityIndicatorStyle = new Style<ActivityIndicator>()
        .AddAppThemeBinding(ActivityIndicator.ColorProperty, AppColors.Primary, AppColors.White);

    public static Style IndicatorViewStyle = new Style<IndicatorView>()
        .AddAppThemeBinding(IndicatorView.IndicatorColorProperty, AppColors.Gray200, AppColors.Gray500)
        .AddAppThemeBinding(IndicatorView.SelectedIndicatorColorProperty, AppColors.Gray950, AppColors.Gray100);

    public static Style BorderStyle = new Style<Border>()
        .AddAppThemeBinding(Border.StrokeProperty, AppColors.Gray200, AppColors.Gray500)
        .Add(Border.StrokeShapeProperty, new Rectangle())
        .Add(Border.StrokeThicknessProperty, 1);

    public static Style BoxViewStyle = new Style<BoxView>()
        .AddAppThemeBinding(BoxView.ColorProperty, AppColors.Gray950, AppColors.Gray200);

    public static Style ButtonStyle = new Style<Button>()
        .AddAppThemeBinding(Button.TextColorProperty, AppColors.White, AppColors.Primary)
        .AddAppThemeBinding(Button.BackgroundColorProperty, AppColors.Primary, AppColors.White)
        .Add(Button.FontFamilyProperty, "OpenSansRegular")
        .Add(Button.FontSizeProperty, 14)
        .Add(Button.CornerRadiusProperty, 8)
        .Add(Button.PaddingProperty, new Thickness(14, 10))
        .Add(Button.MinimumHeightRequestProperty, 44)
        .Add(Button.MinimumWidthRequestProperty, 44);

    public static Style CheckBoxStyle = new Style<CheckBox>()
        .AddAppThemeBinding(CheckBox.ColorProperty, AppColors.Primary, AppColors.White)
        .Add(CheckBox.MinimumHeightRequestProperty, 44)
        .Add(CheckBox.MinimumWidthRequestProperty, 44);

    public static Style DatePickerStyle = new Style<DatePicker>()
        .AddAppThemeBinding(DatePicker.TextColorProperty, AppColors.Gray900, AppColors.White)
        .Add(DatePicker.BackgroundProperty, Colors.Transparent)
        .Add(DatePicker.FontFamilyProperty, "OpenSansRegular")
        .Add(DatePicker.FontSizeProperty, 14)
        .Add(DatePicker.MinimumHeightRequestProperty, 44)
        .Add(DatePicker.MinimumWidthRequestProperty, 44);

    public static Style EditorStyle = new Style<Editor>()
        .AddAppThemeBinding(Editor.TextColorProperty, AppColors.Black, AppColors.White)
        .Add(Editor.BackgroundProperty, Colors.Transparent)
        .Add(Editor.FontFamilyProperty, "OpenSansRegular")
        .Add(Editor.FontSizeProperty, 14)
        .AddAppThemeBinding(Editor.PlaceholderColorProperty, AppColors.Gray200, AppColors.Gray500)
        .Add(Editor.MinimumHeightRequestProperty, 44)
        .Add(Editor.MinimumWidthRequestProperty, 44);

    public static Style EntryStyle = new Style<Entry>()
        .AddAppThemeBinding(Entry.TextColorProperty, AppColors.Black, AppColors.White)
        .Add(Entry.BackgroundProperty, Colors.Transparent)
        .Add(Entry.FontFamilyProperty, "OpenSansRegular")
        .Add(Entry.FontSizeProperty, 14)
        .AddAppThemeBinding(Entry.PlaceholderColorProperty, AppColors.Gray200, AppColors.Gray500)
        .Add(Entry.MinimumHeightRequestProperty, 44)
        .Add(Entry.MinimumWidthRequestProperty, 44);

    public static Style FrameStyle = new Style<Frame>()
        .Add(Frame.ShadowProperty, false)
        .AddAppThemeBinding(Frame.BorderColorProperty, AppColors.Gray200, AppColors.Gray950)
        .Add(Frame.CornerRadiusProperty, 8);

    public static Style ImageButtonStyle = new Style<ImageButton>()
        .Add(ImageButton.OpacityProperty, 1)
        .Add(ImageButton.BorderColorProperty, Colors.Transparent)
        .Add(ImageButton.BorderWidthProperty, 0)
        .Add(ImageButton.CornerRadiusProperty, 0)
        .Add(ImageButton.MinimumHeightRequestProperty, 44)
        .Add(ImageButton.MinimumWidthRequestProperty, 44);

    public static Style LabelStyle = new Style<Label>()
        .AddAppThemeBinding(Label.TextColorProperty, AppColors.Gray900, AppColors.White)
        .Add(Label.BackgroundProperty, Colors.Transparent)
        .Add(Label.FontFamilyProperty, "OpenSansRegular")
        .Add(Label.FontSizeProperty, 14);

    public static Style ListViewStyle = new Style<ListView>()
        .AddAppThemeBinding(ListView.SeparatorColorProperty, AppColors.Gray200, AppColors.Gray500)
        .AddAppThemeBinding(ListView.RefreshControlColorProperty, AppColors.Gray900, AppColors.Gray200);

    public static Style PickerStyle = new Style<Picker>()
        .AddAppThemeBinding(Picker.TextColorProperty, AppColors.Gray900, AppColors.White)
        .AddAppThemeBinding(Picker.TitleColorProperty, AppColors.Gray900, AppColors.Gray200)
        .Add(Picker.BackgroundProperty, Colors.Transparent)
        .Add(Picker.FontFamilyProperty, "OpenSansRegular")
        .Add(Picker.FontSizeProperty, 14)
        .Add(Picker.MinimumHeightRequestProperty, 44)
        .Add(Picker.MinimumWidthRequestProperty, 44);

    public static Style ProgressBarStyle = new Style<ProgressBar>()
        .AddAppThemeBinding(ProgressBar.ProgressColorProperty, AppColors.Primary, AppColors.White);

    public static Style RadioButtonStyle = new Style<RadioButton>()
        .Add(RadioButton.BackgroundProperty, Colors.Transparent)
        .AddAppThemeBinding(RadioButton.TextColorProperty, AppColors.Black, AppColors.White)
        .Add(RadioButton.FontFamilyProperty, "OpenSansRegular")
        .Add(RadioButton.FontSizeProperty, 14)
        .Add(RadioButton.MinimumHeightRequestProperty, 44)
        .Add(RadioButton.MinimumWidthRequestProperty, 44);

    public static Style RefreshViewStyle = new Style<RefreshView>()
        .AddAppThemeBinding(RefreshView.RefreshColorProperty, AppColors.Gray900, AppColors.Gray200);

    public static Style SearchBarStyle = new Style<SearchBar>()
        .AddAppThemeBinding(SearchBar.TextColorProperty, AppColors.Gray900, AppColors.White)
        .Add(SearchBar.PlaceholderColorProperty, AppColors.Gray500)
        .Add(SearchBar.CancelButtonColorProperty, AppColors.Gray500)
        .Add(SearchBar.BackgroundProperty, Colors.Transparent)
        .Add(SearchBar.FontFamilyProperty, "OpenSansRegular")
        .Add(SearchBar.FontSizeProperty, 14)
        .Add(SearchBar.MinimumHeightRequestProperty, 44)
        .Add(SearchBar.MinimumWidthRequestProperty, 44);

    public static Style SearchHandlerStyle = new Style<SearchHandler>()
        .AddAppThemeBinding(SearchHandler.TextColorProperty, AppColors.Gray900, AppColors.White)
        .Add(SearchHandler.PlaceholderColorProperty, AppColors.Gray500)
        .Add(SearchHandler.FontFamilyProperty, "OpenSansRegular")
        .Add(SearchHandler.FontSizeProperty, 14);

    public static Style ShadowStyle = new Style<Shadow>()
        .Add(Shadow.RadiusProperty, 15)
        .Add(Shadow.OpacityProperty, 0.5)
        .AddAppThemeBinding(Shadow.BrushProperty, AppColors.White, AppColors.White)
        .Add(Shadow.OffsetProperty, new Point(10, 10));

    public static Style SliderStyle = new Style<Slider>()
        .AddAppThemeBinding(Slider.MinimumTrackColorProperty, AppColors.Primary, AppColors.White)
        .AddAppThemeBinding(Slider.MaximumTrackColorProperty, AppColors.Gray200, AppColors.Gray600)
        .AddAppThemeBinding(Slider.ThumbColorProperty, AppColors.Primary, AppColors.White);

    public static Style SwipeItemStyle = new Style<SwipeItem>()
        .AddAppThemeBinding(SwipeItem.BackgroundColorProperty, AppColors.White, AppColors.Black);

    public static Style SwitchStyle = new Style<Switch>()
        .AddAppThemeBinding(Switch.OnColorProperty, AppColors.Primary, AppColors.White)
        .Add(Switch.ThumbColorProperty, AppColors.White);

    public static Style TimePickerStyle = new Style<TimePicker>()
        .AddAppThemeBinding(TimePicker.TextColorProperty, AppColors.Gray900, AppColors.White)
        .Add(TimePicker.BackgroundProperty, Colors.Transparent)
        .Add(TimePicker.FontFamilyProperty, "OpenSansRegular")
        .Add(TimePicker.FontSizeProperty, 14)
        .Add(TimePicker.MinimumHeightRequestProperty, 44)
        .Add(TimePicker.MinimumWidthRequestProperty, 44);

    public static Style PageStyle = new Style<Page>()
        .Add(Page.PaddingProperty, 0)
        .AddAppThemeBinding(Page.BackgroundColorProperty, AppColors.White, AppColors.Black)
        .ApplyToDerivedTypes(true);

    public static Style ShellStyle = new Style<Shell>()
        .AddAppThemeBinding(Shell.BackgroundColorProperty, AppColors.Primary, AppColors.Gray950)
        .Add(Shell.ForegroundColorProperty, DeviceInfo.Platform == DevicePlatform.WinUI ? AppColors.Primary : AppColors.White)
        .AddAppThemeBinding(Shell.TitleColorProperty, AppColors.White, AppColors.White)
        .AddAppThemeBinding(Shell.DisabledColorProperty, AppColors.Gray200, AppColors.Gray950)
        .AddAppThemeBinding(Shell.UnselectedColorProperty, AppColors.Gray200, AppColors.Gray200)
        .Add(Shell.NavBarHasShadowProperty, false)
        .AddAppThemeBinding(Shell.TabBarBackgroundColorProperty, AppColors.White, AppColors.Black)
        .AddAppThemeBinding(Shell.TabBarForegroundColorProperty, AppColors.Primary, AppColors.White)
        .AddAppThemeBinding(Shell.TabBarTitleColorProperty, AppColors.Primary, AppColors.White)
        .AddAppThemeBinding(Shell.TabBarUnselectedColorProperty, AppColors.Gray900, AppColors.Gray200)
        .ApplyToDerivedTypes(true);

    public static Style NavigationPageStyle = new Style<NavigationPage>()
        .AddAppThemeBinding(NavigationPage.BarBackgroundColorProperty, AppColors.Primary, AppColors.Gray950)
        .AddAppThemeBinding(NavigationPage.BarTextColorProperty, AppColors.Gray200, AppColors.White)
        .AddAppThemeBinding(NavigationPage.IconColorProperty, AppColors.Gray200, AppColors.White);

    public static Style TabbedPageStyle = new Style<TabbedPage>()
        .AddAppThemeBinding(TabbedPage.BarBackgroundColorProperty, AppColors.White, AppColors.Gray950)
        .AddAppThemeBinding(TabbedPage.BarTextColorProperty, AppColors.Primary, AppColors.White)
        .AddAppThemeBinding(TabbedPage.UnselectedTabColorProperty, AppColors.Gray200, AppColors.Gray950)
        .AddAppThemeBinding(TabbedPage.SelectedItemProperty, AppColors.Gray950, AppColors.Gray200);

    public AppStyles()
    {
        Add(ActivityIndicatorStyle);
        Add(IndicatorViewStyle);
        Add(BorderStyle);
        Add(BoxViewStyle);
        Add(ButtonStyle);
        Add(CheckBoxStyle);
        Add(DatePickerStyle);
        Add(EditorStyle);
        Add(EntryStyle);
        Add(FrameStyle);
        Add(ImageButtonStyle);
        Add(LabelStyle);
        Add(ListViewStyle);
        Add(PickerStyle);
        Add(ProgressBarStyle);
        Add(RadioButtonStyle);
        Add(RefreshViewStyle);
        Add(SearchBarStyle);
        Add(ShadowStyle);
        Add(SliderStyle);
        Add(SwipeItemStyle);
        Add(SwitchStyle);
        Add(TimePickerStyle);
        Add(PageStyle);
        Add(ShellStyle);
        Add(NavigationPageStyle);
        Add(TabbedPageStyle);
    }
}
