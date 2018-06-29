using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Cells;
using Xamarin.Forms.Platform.GTK.Renderers;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Xamarin.Forms.Platform.GTK")]
[assembly: AssemblyDescription("GTK# Backend for Xamarin.Forms")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(FileImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(StreamImageSource), typeof(StreamImagesourceHandler))]
[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(UriImageSourceHandler))]

[assembly: Dependency(typeof(ResourcesProvider))]
[assembly: Dependency(typeof(GtkSerializer))]

[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(ActivityIndicatorRenderer))]
[assembly: ExportRenderer(typeof(BoxView), typeof(BoxViewRenderer))]
[assembly: ExportRenderer(typeof(Button), typeof(ButtonRenderer))]
[assembly: ExportRenderer(typeof(CarouselPage), typeof(CarouselPageRenderer))]
[assembly: ExportRenderer(typeof(DatePicker), typeof(DatePickerRenderer))]
[assembly: ExportRenderer(typeof(Editor), typeof(EditorRenderer))]
[assembly: ExportRenderer(typeof(Entry), typeof(EntryRenderer))]
[assembly: ExportRenderer(typeof(Frame), typeof(FrameRenderer))]
[assembly: ExportRenderer(typeof(Image), typeof(ImageRenderer))]
[assembly: ExportRenderer(typeof(Label), typeof(LabelRenderer))]
[assembly: ExportRenderer(typeof(Layout), typeof(LayoutRenderer))]
[assembly: ExportRenderer(typeof(ListView), typeof(ListViewRenderer))]
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(MasterDetailPageRenderer))]
[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer))]
[assembly: ExportRenderer(typeof(OpenGLView), typeof(OpenGLViewRenderer))]
[assembly: ExportRenderer(typeof(Page), typeof(PageRenderer))]
[assembly: ExportRenderer(typeof(Picker), typeof(PickerRenderer))]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(ProgressBarRenderer))]
[assembly: ExportRenderer(typeof(ScrollView), typeof(ScrollViewRenderer))]
[assembly: ExportRenderer(typeof(SearchBar), typeof(SearchBarRenderer))]
[assembly: ExportRenderer(typeof(Slider), typeof(SliderRenderer))]
[assembly: ExportRenderer(typeof(Stepper), typeof(StepperRenderer))]
[assembly: ExportRenderer(typeof(Switch), typeof(SwitchRenderer))]
[assembly: ExportRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer))]
[assembly: ExportRenderer(typeof(TableView), typeof(TableViewRenderer))]
[assembly: ExportRenderer(typeof(TimePicker), typeof(TimePickerRenderer))]
[assembly: ExportRenderer(typeof(WebView), typeof(WebViewRenderer))]

[assembly: ExportCell(typeof(Cell), typeof(CellRenderer))]
[assembly: ExportCell(typeof(Xamarin.Forms.EntryCell), typeof(EntryCellRenderer))]
[assembly: ExportCell(typeof(Xamarin.Forms.TextCell), typeof(TextCellRenderer))]
[assembly: ExportCell(typeof(Xamarin.Forms.ImageCell), typeof(ImageCellRenderer))]
[assembly: ExportCell(typeof(Xamarin.Forms.SwitchCell), typeof(SwitchCellRenderer))]
[assembly: ExportCell(typeof(Xamarin.Forms.ViewCell), typeof(ViewCellRenderer))]

[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]