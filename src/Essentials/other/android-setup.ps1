$androidSdkHome = $ENV:ANDROID_HOME

if ([string]::IsNullOrWhiteSpace($androidSdkHome))
{
    if ($ENV:OS -eq "Windows_NT")
    {
        $androidSdkHome = "C:\Program Files (x86)\Android\android-sdk\"
    }
    else
    {
        $androidSdkHome = Resolve-Path "~/Library/Developer/Xamarin/android-sdk-macosx"
    }

    Write-Host "Couldn't locate ANDROID_HOME, using: $androidSdkHome"
}

Write-Host "Using Android SDK Home: $androidSdkHome"

Write-Host "Installing Global dotnet android tool..."
dotnet tool install --global --add-source https://www.myget.org/F/xam-dotnet-tools/api/v3/index.json AndroidSdk.Tool

Write-Host "Installing and/or Updating Android SDK..."
Write-Host "This may take awhile..."
android --home="$androidSdkHome" sdk download 

Write-Host "Installing API 23..."
android --home="$androidSdkHome" sdk --install="platforms;android-23"

Write-Host "Installing API 24..."
android --home="$androidSdkHome" sdk --install="platforms;android-24"

Write-Host "Installing API 25..."
android --home="$androidSdkHome" sdk --install="platforms;android-25"

Write-Host "Installing API 26..."
android --home="$androidSdkHome" sdk --install="platforms;android-26"

Write-Host "Installing API 27..."
android --home="$androidSdkHome" sdk --install="platforms;android-27"

Write-Host "Installing API 28..."
android --home="$androidSdkHome" sdk --install="platforms;android-28"

Write-Host "Installing API 29..."
android --home="$androidSdkHome" sdk --install="platforms;android-29"

Write-Host "Installing API 29 Emulator Image (x86_64 - google_apis)..."
android --home="$androidSdkHome" sdk --install="system-images;android-29;google_apis;x86_64"

Write-Host "Creating XamarinEmulator virtual device..."
android --home="$androidSdkHome" emulator create XamarinEmulator "system-images;android-29;google_apis;x86_64" --device=Pixel --force

Write-Host "Done!"