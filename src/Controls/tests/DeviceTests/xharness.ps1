for ($x = 0; $x -lt $args.Length - 1; $x++) {
    if ($args[$x] -eq "--client-port") {
        $clientPort = $args[$x+1]
    }
}

$ADB_EXE = 'C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe'

& $ADB_EXE forward --remove-all
& $ADB_EXE reverse --remove-all
& $ADB_EXE reverse tcp:6000 tcp:$clientPort

& $ADB_EXE wait-for-device

& $ADB_EXE uninstall com.microsoft.maui.controls.devicetests
& $ADB_EXE install "Q:\repos\dotnet\maui\artifacts\bin\Controls.DeviceTests\Debug\net10.0-android\com.microsoft.maui.controls.devicetests-Signed.apk"

& $ADB_EXE shell am instrument --user 0 -w com.microsoft.maui.controls.devicetests