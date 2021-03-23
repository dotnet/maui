using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using UIKit;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls;

[assembly: ExportRenderer(typeof(BoxView), typeof(BoxRenderer))]
[assembly: ExportRenderer(typeof(Entry), typeof(EntryRenderer))]
[assembly: ExportRenderer(typeof(Editor), typeof(EditorRenderer))]
[assembly: ExportRenderer(typeof(Label), typeof(LabelRenderer))]
[assembly: ExportRenderer(typeof(Image), typeof(ImageRenderer))]
[assembly: ExportRenderer(typeof(Button), typeof(ButtonRenderer))]
[assembly: ExportRenderer(typeof(TableView), typeof(TableViewRenderer))]
[assembly: ExportRenderer(typeof(Slider), typeof(SliderRenderer))]
[assembly: ExportRenderer(typeof(WebView), typeof(WkWebViewRenderer))]
[assembly: ExportRenderer(typeof(SearchBar), typeof(SearchBarRenderer))]
[assembly: ExportRenderer(typeof(Switch), typeof(SwitchRenderer))]
[assembly: ExportRenderer(typeof(SwipeView), typeof(SwipeViewRenderer))]
[assembly: ExportRenderer(typeof(DatePicker), typeof(DatePickerRenderer))]
[assembly: ExportRenderer(typeof(TimePicker), typeof(TimePickerRenderer))]
[assembly: ExportRenderer(typeof(Picker), typeof(PickerRenderer))]
[assembly: ExportRenderer(typeof(Stepper), typeof(StepperRenderer))]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(ProgressBarRenderer))]
[assembly: ExportRenderer(typeof(ScrollView), typeof(ScrollViewRenderer))]
[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(ActivityIndicatorRenderer))]
[assembly: ExportRenderer(typeof(Frame), typeof(FrameRenderer))]
[assembly: ExportRenderer(typeof(ListView), typeof(ListViewRenderer))]
[assembly: ExportRenderer (typeof (CollectionView), typeof (CollectionViewRenderer))]
[assembly: ExportRenderer(typeof(CarouselView), typeof(CarouselViewRenderer))]
[assembly: ExportRenderer(typeof(IndicatorView), typeof(IndicatorViewRenderer))]
[assembly: ExportRenderer(typeof(OpenGLView), typeof(OpenGLViewRenderer))]
[assembly: ExportRenderer (typeof (CheckBox), typeof (CheckBoxRenderer))]
[assembly: ExportRenderer(typeof(Path), typeof(PathRenderer))]
[assembly: ExportRenderer(typeof(Ellipse), typeof(EllipseRenderer))]
[assembly: ExportRenderer(typeof(Line), typeof(LineRenderer))]
[assembly: ExportRenderer(typeof(Polyline), typeof(PolylineRenderer))]
[assembly: ExportRenderer(typeof(Polygon), typeof(PolygonRenderer))]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Shapes.Rectangle), typeof(RectangleRenderer))]

[assembly: ExportRenderer(typeof(TabbedPage), typeof(TabbedRenderer))]
[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationRenderer))]
[assembly: ExportRenderer(typeof(CarouselPage), typeof(CarouselPageRenderer))]
[assembly: ExportRenderer(typeof(Page), typeof(PageRenderer))]
[assembly: ExportRenderer(typeof(FlyoutPage), typeof(PhoneFlyoutPageRenderer), UIUserInterfaceIdiom.Phone)]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(PhoneMasterDetailRenderer), UIUserInterfaceIdiom.Phone)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: ExportRenderer(typeof(RefreshView), typeof(RefreshViewRenderer))]

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(TabletMasterDetailRenderer), UIUserInterfaceIdiom.Pad)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: ExportRenderer(typeof(FlyoutPage), typeof(TabletFlyoutPageRenderer), UIUserInterfaceIdiom.Pad)]
[assembly: ExportRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer))]
[assembly: ExportRenderer(typeof(Shell), typeof(ShellRenderer))]
[assembly: ExportCell(typeof(Cell), typeof(CellRenderer))]
[assembly: ExportCell(typeof(ImageCell), typeof(ImageCellRenderer))]
[assembly: ExportCell(typeof(EntryCell), typeof(EntryCellRenderer))]
[assembly: ExportCell(typeof(TextCell), typeof(TextCellRenderer))]
[assembly: ExportCell(typeof(ViewCell), typeof(ViewCellRenderer))]
[assembly: ExportCell(typeof(SwitchCell), typeof(SwitchCellRenderer))]
[assembly: ExportRenderer(typeof(Microsoft.Maui.EmbeddedFont), typeof(Microsoft.Maui.EmbeddedFontLoader))]
[assembly: ExportEffect(typeof(ShadowEffect), "ShadowEffect")]
[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(FileImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(StreamImageSource), typeof(StreamImagesourceHandler))]
[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(ImageLoaderSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(FontImageSource), typeof(FontImageSourceHandler))]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Compatibility.Platform")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Compatibility.Material")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Compatibility.iOS.UnitTests")]
[assembly: Microsoft.Maui.Controls.Dependency(typeof(Deserializer))]
[assembly: Microsoft.Maui.Controls.Dependency(typeof(ResourcesProvider))]
[assembly: ResolutionGroupName("Xamarin")]
[assembly: LinkerSafe]