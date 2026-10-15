global using Microsoft.Maui.Interop;

[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.Media.ITextToSpeech))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.Controls.Page))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.Controls.Application))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.IWindow))]
[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.Devices.Sensors.IGeolocation))]
[assembly: Microsoft.Maui.Controls.HybridWebApps.HybridJsonContext(typeof(Microsoft.Maui.Devices.Sensors.Location), typeof(Microsoft.Maui.Controls.HybridWebApps.Essentials.NativeLocationJsonContext))]
[assembly: Microsoft.Maui.Controls.HybridWebApps.HybridJsonContext(typeof(Microsoft.Maui.Keyboard), typeof(Microsoft.Maui.Controls.HybridWebApps.UI.NativeKeyboardJsonContext))]
