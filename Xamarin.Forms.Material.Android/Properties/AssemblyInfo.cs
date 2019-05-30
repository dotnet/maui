using System.Reflection;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Material.Android;

[assembly: Preserve]
[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Button), typeof(MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(DatePicker), typeof(MaterialDatePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Editor), typeof(MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Entry), typeof(MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Frame), typeof(MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Picker), typeof(MaterialPickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Slider), typeof(MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(Stepper), typeof(MaterialStepperRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(TimePicker), typeof(MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
[assembly: ExportRenderer(typeof(CheckBox), typeof(MaterialCheckBoxRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]