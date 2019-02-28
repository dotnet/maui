using System.Reflection;
using Xamarin.Forms.Internals;
using System.Runtime.InteropServices;
using Xamarin.Forms;

[assembly: AssemblyTitle("Xamarin.Forms.Material")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("7cdc1f47-073c-45ee-aee0-540abfb11baa")]
[assembly: Preserve]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]

[assembly: ExportRenderer(typeof(Xamarin.Forms.ActivityIndicator), typeof(Xamarin.Forms.Material.iOS.MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Button), typeof(Xamarin.Forms.Material.iOS.MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Entry), typeof(Xamarin.Forms.Material.iOS.MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Frame), typeof(Xamarin.Forms.Material.iOS.MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.ProgressBar), typeof(Xamarin.Forms.Material.iOS.MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Slider), typeof(Xamarin.Forms.Material.iOS.MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.TimePicker), typeof(Xamarin.Forms.Material.iOS.MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Picker), typeof(Xamarin.Forms.Material.iOS.MaterialPickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.DatePicker), typeof(Xamarin.Forms.Material.iOS.MaterialDatePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Stepper), typeof(Xamarin.Forms.Material.iOS.MaterialStepperRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Editor), typeof(Xamarin.Forms.Material.iOS.MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
