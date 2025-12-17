# Microsoft.Maui.Controls.Maps

`Microsoft.Maui.Controls.Maps` brings an easy-to-use map control to .NET MAUI apps so you can display maps, drop pins, draw shapes, and react to user interactions on Android and iOS from one shared codebase. (Windows implementation is not available yet.)

## 🚀 Get started

1. **Install the package**
   ```bash
   dotnet add package Microsoft.Maui.Controls.Maps
   ```
2. **Enable Maps in `MauiProgram.cs`**
   ```csharp
   var builder = MauiApp.CreateBuilder();
   builder
       .UseMauiApp<App>()
       .UseMauiMaps(); // registers map handlers
   ```
3. **Add platform credentials**
   - **Android**: Add your Google Maps API key to `Platforms/Android/AndroidManifest.xml`
     ```xml
     <application>
         <meta-data android:name="com.google.android.geo.API_KEY"
                    android:value="YOUR_API_KEY_HERE" />
     </application>
     ```
   - **iOS**: No additional keys required for the built-in map provider.
   - **Windows**: The Maps control is currently not implemented on Windows. For updates, see [this tracking link](https://aka.ms/maui-maps-no-windows).

4. **Place a map in your page**
   ```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:maps="clr-namespace:Microsoft.Maui.Controls.Maps"
             xmlns:essentials="clr-namespace:Microsoft.Maui.Devices.Sensors;assembly=Microsoft.Maui">
    <maps:Map>
        <maps:Map.Pins>
            <maps:Pin Label="Downtown"
                      Address="Main Street"
                      Type="Place">
                <maps:Pin.Location>
                    <essentials:Location Latitude="47.6205"
                                         Longitude="-122.3493" />
                </maps:Pin.Location>
            </maps:Pin>
        </maps:Map.Pins>
    </maps:Map>
</ContentPage>
   ```

## 📚 Learn more

- [.NET MAUI Map control docs](https://learn.microsoft.com/dotnet/maui/user-interface/controls/map?view=net-maui-10.0) – capabilities, events, and platform guidance
- [.NET MAUI documentation](https://learn.microsoft.com/dotnet/maui/) – build and ship cross-platform apps
