using System.Reflection;
using System.Maui.Internals;
using System.Runtime.InteropServices;
using System.Maui;

[assembly: AssemblyTitle("System.Maui.Material")]
[assembly: Preserve]
[assembly: ExportRenderer(typeof(System.Maui.ActivityIndicator), typeof(System.Maui.Material.iOS.MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.Button), typeof(System.Maui.Material.iOS.MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.Entry), typeof(System.Maui.Material.iOS.MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.Frame), typeof(System.Maui.Material.iOS.MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.ProgressBar), typeof(System.Maui.Material.iOS.MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.Slider), typeof(System.Maui.Material.iOS.MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.TimePicker), typeof(System.Maui.Material.iOS.MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.Picker), typeof(System.Maui.Material.iOS.MaterialPickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.DatePicker), typeof(System.Maui.Material.iOS.MaterialDatePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.Stepper), typeof(System.Maui.Material.iOS.MaterialStepperRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.Editor), typeof(System.Maui.Material.iOS.MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(System.Maui.CheckBox), typeof(System.Maui.Material.iOS.MaterialCheckBoxRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
