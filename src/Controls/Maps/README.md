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

## 🧭 Features at a glance

- **Pins**: Labels, addresses, pin types, custom images, and tap handling via `PinClicked`.
- **Shapes**: Draw **polygons**, **polylines**, and **circles** to outline areas or routes.
- **Map interactions**: Handle `MapClicked`, move the camera/region, and programmatically add or remove map elements at runtime.
- **Map display**: Switch map types (e.g., Street, Satellite, Hybrid) and control gesture support (scroll, zoom, rotate).
- **User location**: Show the user’s current location with `IsShowingUser` (platform location permission required).

### Samples: pins and shapes

```xml
<maps:Map MapType="Hybrid" MapClicked="OnMapClicked">
    <maps:Map.Pins>
        <maps:Pin Label="Office" Type="Place">
            <maps:Pin.Location>
                <essentials:Location Latitude="47.6424" Longitude="-122.3219" />
            </maps:Pin.Location>
        </maps:Pin>
    </maps:Map.Pins>

    <maps:Map.MapElements>
        <maps:Polygon StrokeColor="Red" FillColor="#40FF0000" StrokeWidth="4">
            <maps:Polygon.Geopath>
                <maps:LocationCollection>
                    <essentials:Location Latitude="47.642" Longitude="-122.323" />
                    <essentials:Location Latitude="47.643" Longitude="-122.326" />
                    <essentials:Location Latitude="47.640" Longitude="-122.327" />
                </maps:LocationCollection>
            </maps:Polygon.Geopath>
        </maps:Polygon>

        <maps:Polyline StrokeColor="DodgerBlue" StrokeWidth="6">
            <maps:Polyline.Geopath>
                <maps:LocationCollection>
                    <essentials:Location Latitude="47.639" Longitude="-122.330" />
                    <essentials:Location Latitude="47.640" Longitude="-122.335" />
                </maps:LocationCollection>
            </maps:Polyline.Geopath>
        </maps:Polyline>

        <maps:Circle StrokeColor="Green" FillColor="#40008000" StrokeWidth="3">
            <maps:Circle.Center>
                <essentials:Location Latitude="47.641" Longitude="-122.329" />
            </maps:Circle.Center>
            <maps:Circle.Radius>
                <maps:Distance>
                    <x:Arguments>
                        <x:Double>200</x:Double>
                    </x:Arguments>
                </maps:Distance>
            </maps:Circle.Radius>
        </maps:Circle>
    </maps:Map.MapElements>
</maps:Map>
```

### Samples: interactions and map modes

```csharp
// In code-behind
void OnMapClicked(object sender, MapClickedEventArgs e)
{
    // Drop a pin where the user tapped
    var map = (Map)sender;
    map.Pins.Add(new Pin
    {
        Label = "Dropped pin",
        Type = PinType.Place,
        Location = e.Location
    });

    // Move/zoom to the tapped location
    map.MoveToRegion(MapSpan.FromCenterAndRadius(e.Location, Distance.FromMeters(500)));
}

// Toggle map display mode
void ToggleMapType(Map map) =>
    map.MapType = map.MapType == MapType.Street ? MapType.Satellite : MapType.Street;

// Common runtime toggles
void EnableLocationAndTraffic(Map map)
{
    map.IsShowingUser = true;      // requires location permission on the device
    map.IsTrafficEnabled = true;   // show live traffic where supported
}
```

## 📚 Learn more

- [.NET MAUI Map control docs](https://learn.microsoft.com/dotnet/maui/user-interface/controls/map) – capabilities, events, and platform guidance
- [.NET MAUI documentation](https://learn.microsoft.com/dotnet/maui/) – build and ship cross-platform apps
