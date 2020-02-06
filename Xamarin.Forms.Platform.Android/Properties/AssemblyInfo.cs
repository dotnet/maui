using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

// These renderers are now registered via the RenderWithAttribute in the Android Forwarders project.
// Note that AppCompat and FastRenderers are also registered conditionally in FormsAppCompatActivity.LoadApplication
#if ROOT_RENDERERS
[assembly: ExportRenderer (typeof (BoxView), typeof (BoxRenderer))]
[assembly: ExportRenderer (typeof (Entry), typeof (EntryRenderer))]
[assembly: ExportRenderer (typeof (Editor), typeof (EditorRenderer))]
[assembly: ExportRenderer (typeof (Label), typeof (LabelRenderer))]
[assembly: ExportRenderer (typeof (Image), typeof (ImageRenderer))]
[assembly: ExportRenderer (typeof (Button), typeof (ButtonRenderer))]
[assembly: ExportRenderer (typeof (ImageButton), typeof (ImageButtonRenderer))]
[assembly: ExportRenderer (typeof (TableView), typeof (TableViewRenderer))]
[assembly: ExportRenderer (typeof (ListView), typeof (ListViewRenderer))]
[assembly: ExportRenderer (typeof (CollectionView), typeof (CollectionViewRenderer))]
[assembly: ExportRenderer (typeof (CarouselView), typeof (CarouselViewRenderer))]
[assembly: ExportRenderer (typeof (Slider), typeof (SliderRenderer))]
[assembly: ExportRenderer (typeof (WebView), typeof (WebViewRenderer))]
[assembly: ExportRenderer (typeof (SearchBar), typeof (SearchBarRenderer))]
[assembly: ExportRenderer (typeof (Switch), typeof (SwitchRenderer))]
[assembly: ExportRenderer(typeof(SwipeView), typeof(SwipeViewRenderer))]
[assembly: ExportRenderer (typeof (DatePicker), typeof (DatePickerRenderer))]
[assembly: ExportRenderer (typeof (TimePicker), typeof (TimePickerRenderer))]
[assembly: ExportRenderer (typeof (Picker), typeof (PickerRenderer))]
[assembly: ExportRenderer (typeof (Stepper), typeof (StepperRenderer))]
[assembly: ExportRenderer (typeof (ProgressBar), typeof (ProgressBarRenderer))]
[assembly: ExportRenderer (typeof (ScrollView), typeof (ScrollViewRenderer))]
[assembly: ExportRenderer (typeof (ActivityIndicator), typeof (ActivityIndicatorRenderer))]
[assembly: ExportRenderer (typeof (Frame), typeof (FrameRenderer))]
[assembly: ExportRenderer (typeof (OpenGLView), typeof (OpenGLViewRenderer))]
[assembly: ExportRenderer (typeof (CheckBox), typeof (CheckBoxRenderer))]

[assembly: ExportRenderer (typeof (TabbedPage), typeof (TabbedRenderer))]
[assembly: ExportRenderer (typeof (NavigationPage), typeof (NavigationRenderer))]
[assembly: ExportRenderer (typeof (CarouselPage), typeof (CarouselPageRenderer))]
[assembly: ExportRenderer (typeof (Page), typeof (PageRenderer))]
[assembly: ExportRenderer (typeof (MasterDetailPage), typeof (MasterDetailRenderer))]
[assembly: ExportRenderer (typeof (MediaElement), typeof(MediaElementRenderer))]
[assembly: ExportRenderer (typeof (RefreshView), typeof (RefreshViewRenderer))]
#endif

[assembly: ExportRenderer(typeof(Shell), typeof(ShellRenderer))]

[assembly: ExportRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer))]
[assembly: ExportCell(typeof(Cell), typeof(CellRenderer))]
[assembly: ExportCell(typeof(EntryCell), typeof(EntryCellRenderer))]
[assembly: ExportCell(typeof(SwitchCell), typeof(SwitchCellRenderer))]
[assembly: ExportCell(typeof(TextCell), typeof(TextCellRenderer))]
[assembly: ExportCell(typeof(ImageCell), typeof(ImageCellRenderer))]
[assembly: ExportCell(typeof(ViewCell), typeof(ViewCellRenderer))]
[assembly: ExportRenderer(typeof(EmbeddedFont), typeof(EmbeddedFontLoader))]
[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(FileImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(StreamImageSource), typeof(StreamImagesourceHandler))]
[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(ImageLoaderSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(FontImageSource), typeof(FontImageSourceHandler))]
[assembly: Xamarin.Forms.Dependency(typeof(Deserializer))]
[assembly: Xamarin.Forms.Dependency(typeof(ResourcesProvider))]
[assembly: Preserve]
[assembly: InternalsVisibleTo("Xamarin.Forms.Platform")]
[assembly: InternalsVisibleTo("Xamarin.Forms.Material")]
[assembly: InternalsVisibleTo("Xamarin.Forms.DualScreen")]