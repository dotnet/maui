$createSecrets = Test-Path .\Xamarin.Forms.Controls\secrets.txt
$createMapKeys = Test-Path .\Xamarin.Forms.ControlGallery.Android\Properties\MapsKey.cs

if(-not $createSecrets){
    Write-Host "Creating secrets.txt"
    New-Item -ItemType File .\Xamarin.Forms.Controls\secrets.txt
}

if(-not $createMapKeys){
    Write-Host "Creating MapKeys.cs"

    $content = @"
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android.App;

[assembly: Android.App.MetaData("com.google.android.maps.v2.API_KEY", Value = "")]
"@

    Set-Content .\Xamarin.Forms.ControlGallery.Android\Properties\MapsKey.cs $content
}