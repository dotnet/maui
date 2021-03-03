using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Shapes;
using Rectangle = Microsoft.Maui.Controls.Shapes.Rectangle;

[assembly: Dependency(typeof(WindowsSerializer))]

// Views

[assembly: ExportRenderer(typeof(Layout), typeof(LayoutRenderer))]
[assembly: ExportRenderer(typeof(BoxView), typeof(BoxViewBorderRenderer))]
[assembly: ExportRenderer(typeof(Image), typeof(ImageRenderer))]
[assembly: ExportRenderer(typeof(ImageButton), typeof(ImageButtonRenderer))]
[assembly: ExportRenderer(typeof(Label), typeof(LabelRenderer))]
[assembly: ExportRenderer(typeof(Button), typeof(ButtonRenderer))]
[assembly: ExportRenderer(typeof(RadioButton), typeof(RadioButtonRenderer))]
[assembly: ExportRenderer(typeof(ListView), typeof(ListViewRenderer))]
[assembly: ExportRenderer(typeof(CarouselView), typeof(CarouselViewRenderer))]
[assembly: ExportRenderer(typeof(IndicatorView), typeof(IndicatorViewRenderer))]
[assembly: ExportRenderer(typeof(CollectionView), typeof(CollectionViewRenderer))]
[assembly: ExportRenderer(typeof(ScrollView), typeof(ScrollViewRenderer))]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(ProgressBarRenderer))]
[assembly: ExportRenderer(typeof(Slider), typeof(SliderRenderer))]
[assembly: ExportRenderer(typeof(Switch), typeof(SwitchRenderer))]
[assembly: ExportRenderer(typeof(SwipeView), typeof(SwipeViewRenderer))]
[assembly: ExportRenderer(typeof(WebView), typeof(WebViewRenderer))]
[assembly: ExportRenderer(typeof(Frame), typeof(FrameRenderer))]
[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(ActivityIndicatorRenderer))]
[assembly: ExportRenderer(typeof(Editor), typeof(EditorRenderer))]
[assembly: ExportRenderer(typeof(Picker), typeof(PickerRenderer))]
[assembly: ExportRenderer(typeof(TimePicker), typeof(TimePickerRenderer))]
[assembly: ExportRenderer(typeof(DatePicker), typeof(DatePickerRenderer))]
[assembly: ExportRenderer(typeof(Stepper), typeof(StepperRenderer))]
[assembly: ExportRenderer(typeof(Entry), typeof(EntryRenderer))]
[assembly: ExportRenderer(typeof(CheckBox), typeof(CheckBoxRenderer))]
[assembly: ExportRenderer(typeof(TableView), typeof(TableViewRenderer))]
[assembly: ExportRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer))]
[assembly: ExportRenderer(typeof(RefreshView), typeof(RefreshViewRenderer))]
[assembly: ExportRenderer(typeof(Shell), typeof(ShellRenderer))]
[assembly: ExportRenderer(typeof(Path), typeof(PathRenderer))]
[assembly: ExportRenderer(typeof(Ellipse), typeof(EllipseRenderer))]
[assembly: ExportRenderer(typeof(Line), typeof(LineRenderer))]
[assembly: ExportRenderer(typeof(Polygon), typeof(PolygonRenderer))]
[assembly: ExportRenderer(typeof(Polyline), typeof(PolylineRenderer))]
[assembly: ExportRenderer(typeof(Rectangle), typeof(RectangleRenderer))]
[assembly: ExportRenderer(typeof(IndicatorView), typeof(IndicatorViewRenderer))]
[assembly: ExportRenderer(typeof(RadioButton), typeof(RadioButtonRenderer))]

//ImageSources

[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(FileImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(StreamImageSource), typeof(StreamImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(UriImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(FontImageSource), typeof(FontImageSourceHandler))]

// Pages

[assembly: ExportRenderer(typeof(Page), typeof(PageRenderer))]
[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer))]
[assembly: ExportRenderer(typeof(FlyoutPage), typeof(FlyoutPageRenderer))]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(MasterDetailPageRenderer))]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: ExportRenderer(typeof(CarouselPage), typeof(CarouselPageRenderer))]

// Cells

[assembly: ExportCell(typeof(Cell), typeof(TextCellRenderer))]
[assembly: ExportCell(typeof(ImageCell), typeof(ImageCellRenderer))]
[assembly: ExportCell(typeof(EntryCell), typeof(EntryCellRenderer))]
[assembly: ExportCell(typeof(SwitchCell), typeof(SwitchCellRenderer))]
[assembly: ExportCell(typeof(ViewCell), typeof(ViewCellRenderer))]
[assembly: Dependency(typeof(WindowsResourcesProvider))]
[assembly: ExportRenderer(typeof(SearchBar), typeof(SearchBarRenderer))]
[assembly: ExportRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer))]

//Fonts
[assembly: ExportRenderer(typeof(EmbeddedFont), typeof(EmbeddedFontLoader))]

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Maui.Controls.DualScreen")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests")]
