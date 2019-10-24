using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.WinPhone;
using TableView = Xamarin.Forms.TableView;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Xamarin.Forms.Platform.WP8")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("50e5d817-4ac1-46c0-8e20-260fb6ad16af")]
[assembly: NeutralResourcesLanguage("en-US")]

// View subclasses

[assembly: ExportRenderer(typeof(BoxView), typeof(BoxViewRenderer))]
[assembly: ExportRenderer(typeof(Entry), typeof(EntryRenderer))]
[assembly: ExportRenderer(typeof(Editor), typeof(EditorRenderer))]
[assembly: ExportRenderer(typeof(Label), typeof(LabelRenderer))]
[assembly: ExportRenderer(typeof(Image), typeof(ImageRenderer))]
[assembly: ExportRenderer(typeof(Button), typeof(ButtonRenderer))]
[assembly: ExportRenderer(typeof(Slider), typeof(SliderRenderer))]
[assembly: ExportRenderer(typeof(WebView), typeof(WebViewRenderer))]
[assembly: ExportRenderer(typeof(SearchBar), typeof(SearchBarRenderer))]
[assembly: ExportRenderer(typeof(Switch), typeof(SwitchRenderer))]
[assembly: ExportRenderer(typeof(DatePicker), typeof(DatePickerRenderer))]
[assembly: ExportRenderer(typeof(TimePicker), typeof(TimePickerRenderer))]
[assembly: ExportRenderer(typeof(Picker), typeof(PickerRenderer))]
[assembly: ExportRenderer(typeof(Stepper), typeof(StepperRenderer))]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(ProgressBarRenderer))]
[assembly: ExportRenderer(typeof(ScrollView), typeof(ScrollViewRenderer))]
[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(ActivityIndicatorRenderer))]
[assembly: ExportRenderer(typeof(Frame), typeof(FrameRenderer))]
[assembly: ExportRenderer(typeof(NavigationMenu), typeof(NavigationMenuRenderer))]
[assembly: ExportRenderer(typeof(ListView), typeof(ListViewRenderer))]
[assembly: ExportRenderer(typeof(TableView), typeof(TableViewRenderer))]
[assembly: ExportRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer))]

// Form and subclasses

[assembly: ExportRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer))]
[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer))]
[assembly: ExportRenderer(typeof(CarouselPage), typeof(CarouselPageRenderer))]
[assembly: ExportRenderer(typeof(Page), typeof(PageRenderer))]
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(MasterDetailRenderer))]

//ImageSources

[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(FileImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(StreamImageSource), typeof(StreamImagesourceHandler))]
[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(ImageLoaderSourceHandler))]

// Cells

[assembly: ExportCell(typeof(Cell), typeof(TextCellRenderer))]
[assembly: ExportCell(typeof(ImageCell), typeof(ImageCellRenderer))]
[assembly: ExportCell(typeof(EntryCell), typeof(EntryCellRenderer))]
[assembly: ExportCell(typeof(SwitchCell), typeof(SwitchCellRenderer))]
[assembly: ExportCell(typeof(ViewCell), typeof(ViewCellRenderer))]
[assembly: Dependency(typeof(Deserializer))]
[assembly: Dependency(typeof(ResourcesProvider))]
[assembly: InternalsVisibleTo("Xamarin.Forms.Core.WP8.UnitTests")]