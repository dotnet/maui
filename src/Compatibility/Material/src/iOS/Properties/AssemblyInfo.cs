using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls;

[assembly: AssemblyTitle("Microsoft.Maui.Controls.Material")]
[assembly: Preserve]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.ActivityIndicator), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Button), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Entry), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Frame), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.ProgressBar), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Slider), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.TimePicker), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Picker), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialPickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.DatePicker), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialDatePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Stepper), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialStepperRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Editor), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.CheckBox), typeof(Microsoft.Maui.Controls.Compatibility.Material.iOS.MaterialCheckBoxRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
