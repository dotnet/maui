using System.Reflection;
using System.Runtime.InteropServices;
using System.Maui;
using System.Maui.Internals;
using System.Maui.Material.Android;

[assembly: Preserve]
[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Button), typeof(MaterialButtonRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(DatePicker), typeof(MaterialDatePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Editor), typeof(MaterialEditorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Entry), typeof(MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Frame), typeof(MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Picker), typeof(MaterialPickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(ProgressBar), typeof(MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Slider), typeof(MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(Stepper), typeof(MaterialStepperRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(TimePicker), typeof(MaterialTimePickerRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
[assembly: ExportRenderer(typeof(CheckBox), typeof(MaterialCheckBoxRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]