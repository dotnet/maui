using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("Xamarin.Forms.Platform.Android")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
[assembly: InternalsVisibleTo("Xamarin.Forms.Platform")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]


// These renderers are now registered via the RenderWithAttribute in the Android Forwarders project.
// Note that AppCompat and FastRenderers are also registered conditionally in FormsAppCompatActivity.LoadApplication
#if ROOT_RENDERERS
[assembly: ExportRenderer (typeof (BoxView), typeof (BoxRenderer))]
[assembly: ExportRenderer (typeof (Entry), typeof (EntryRenderer))]
[assembly: ExportRenderer (typeof (Editor), typeof (EditorRenderer))]
[assembly: ExportRenderer (typeof (Label), typeof (LabelRenderer))]
[assembly: ExportRenderer (typeof (Image), typeof (ImageRenderer))]
[assembly: ExportRenderer (typeof (Button), typeof (ButtonRenderer))]
[assembly: ExportRenderer (typeof (TableView), typeof (TableViewRenderer))]
[assembly: ExportRenderer (typeof (ListView), typeof (ListViewRenderer))]
[assembly: ExportRenderer (typeof (Slider), typeof (SliderRenderer))]
[assembly: ExportRenderer (typeof (WebView), typeof (WebViewRenderer))]
[assembly: ExportRenderer (typeof (SearchBar), typeof (SearchBarRenderer))]
[assembly: ExportRenderer (typeof (Switch), typeof (SwitchRenderer))]
[assembly: ExportRenderer (typeof (DatePicker), typeof (DatePickerRenderer))]
[assembly: ExportRenderer (typeof (TimePicker), typeof (TimePickerRenderer))]
[assembly: ExportRenderer (typeof (Picker), typeof (PickerRenderer))]
[assembly: ExportRenderer (typeof (Stepper), typeof (StepperRenderer))]
[assembly: ExportRenderer (typeof (ProgressBar), typeof (ProgressBarRenderer))]
[assembly: ExportRenderer (typeof (ScrollView), typeof (ScrollViewRenderer))]
[assembly: ExportRenderer (typeof (ActivityIndicator), typeof (ActivityIndicatorRenderer))]
[assembly: ExportRenderer (typeof (Frame), typeof (FrameRenderer))]
[assembly: ExportRenderer (typeof (NavigationMenu), typeof (NavigationMenuRenderer))]
[assembly: ExportRenderer (typeof (OpenGLView), typeof (OpenGLViewRenderer))]

[assembly: ExportRenderer (typeof (TabbedPage), typeof (TabbedRenderer))]
[assembly: ExportRenderer (typeof (NavigationPage), typeof (NavigationRenderer))]
[assembly: ExportRenderer (typeof (CarouselPage), typeof (CarouselPageRenderer))]
[assembly: ExportRenderer (typeof (Page), typeof (PageRenderer))]
[assembly: ExportRenderer (typeof (MasterDetailPage), typeof (MasterDetailRenderer))]
#endif

[assembly: ExportRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer))]
[assembly: ExportCell(typeof(Cell), typeof(CellRenderer))]
[assembly: ExportCell(typeof(EntryCell), typeof(EntryCellRenderer))]
[assembly: ExportCell(typeof(SwitchCell), typeof(SwitchCellRenderer))]
[assembly: ExportCell(typeof(TextCell), typeof(TextCellRenderer))]
[assembly: ExportCell(typeof(ImageCell), typeof(ImageCellRenderer))]
[assembly: ExportCell(typeof(ViewCell), typeof(ViewCellRenderer))]
[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(FileImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(StreamImageSource), typeof(StreamImagesourceHandler))]
[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(ImageLoaderSourceHandler))]
[assembly: Xamarin.Forms.Dependency(typeof(Deserializer))]
[assembly: Xamarin.Forms.Dependency(typeof(ResourcesProvider))]
[assembly: Preserve]