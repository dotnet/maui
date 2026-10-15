global using Microsoft.Maui.Interop;

[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.Media.ITextToSpeech))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.Controls.Page))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.IWindow))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.Devices.Sensors.IGeolocation))]
[assembly: Maui.HybridApp.HybridJsonContext(typeof(Microsoft.Maui.Devices.Sensors.Location), typeof(Maui.HybridApp.Essentials.NativeLocationJsonContext))]
[assembly: Maui.HybridApp.HybridJsonContext(typeof(Microsoft.Maui.Keyboard), typeof(Maui.HybridApp.UI.NativeKeyboardJsonContext))]
