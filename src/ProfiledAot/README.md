# Profiled AOT support for Android

This is based on the NuGet package found here:

https://github.com/jonathanpeppers/Mono.Profiler.Android#usage-of-the-aot-profiler

## Updating Profiles

Build MAUI following the instructions at [DEVELOPMENT.md][0]. Make
sure to build with `--configuration=Release`.

Run the `Record` target on each "type" of project template:

```bash
./.dotnet/dotnet build src/ProfiledAot/build.proj -bl -p:App=maui
./.dotnet/dotnet build src/ProfiledAot/build.proj -bl -p:App=maui-sc
./.dotnet/dotnet build src/ProfiledAot/build.proj -bl -p:App=maui-blazor
```

* `maui` is `dotnet new maui`
* `maui-sc` is `dotnet new maui -sc` or (sample content)
* `maui-blazor` is `dotnet new maui-blazor`

You can also use `-r android-x64`, if you'd prefer an x86_64 emulator.

`maui.aotprofile.txt` and `maui-blazor.aotprofile.txt` are a list of
method names contained within the profiles. We don't ship these, but
we can use them to track changes over time. Note that they are not
always in order, so I opened the files in VS Code and did Ctrl+Shift+P
to get the command palette. Then `Sort lines ascending` to get them in
alphabetical order. If the text files don't change, it is likely not
necessary to update the `.aotprofile` files.

## Testing Profiles

Once you've updated the profile, always test to make sure the times
you get are either "the same" or slightly better than before.

Build MAUI again with `--configuration=Release` (see
[DEVELOPMENT.md][0]).

Create a new project and run it:

```bash
mkdir foo && cd foo
../.dotnet/dotnet new maui
../.dotnet/dotnet build -c Release -t:Run -f net7.0-android
```

Run the app a few times and make sure you get good launch times:

```bash
$ adb logcat -d | grep Displayed
02-22 15:50:50.502  1802  1962 I ActivityTaskManager: Displayed com.companyname.foo/crc64808a40cc7e533249.MainActivity: +477ms
02-22 15:50:51.703  1802  1962 I ActivityTaskManager: Displayed com.companyname.foo/crc64808a40cc7e533249.MainActivity: +477ms
02-22 15:50:52.926  1802  1962 I ActivityTaskManager: Displayed com.companyname.foo/crc64808a40cc7e533249.MainActivity: +477ms
```

You can also use [profile-android.ps1][1] in this repo, or [profile.ps1][2].

### Troubleshooting

If the resulting `*.aotprofile.txt` file is empty, review the `.binlog`
file to see if there is a message like:

```log
Task Exec 85ms
CommandLineArguments = "D:\.nuget\packages\mono.aotprofiler.android\9.0.0-preview1\tools\aprofutil"  -s -v -p 9999 -o "custom.aprof"
Reading from '127.0.0.1:9999'...
Read 19 bytes...
Read total 19 bytes...
Summary:
	Modules:          0
	Types:            0
	Methods:          0
Going to write the profile to 'custom.aprof'
```

Mono will print out more information to `adb logcat` if you set:

```bash
adb shell setprop debug.mono.log default,assembly,mono_log_level=debug,mono_log_mask=all
```

Be sure to comment out the line in
`src\ProfiledAot\src\Directory.Build.targets` that sets
`debug.mono.log` to empty.

A *working* example of Mono's logging would say:

```log
05-21 11:38:30.032 28555 28555 W monodroid: Initializing profiler with options: aot:port=9999,output=/data/user/0/com.companyname.maui/files/.__override__/arm64-v8a/profile.aotprofile
05-21 11:38:30.032 28555 28555 I monodroid-assembly: Trying to load shared library '/data/app/~~aLsIB6f0cwe4kpqTjGW6KA==/com.companyname.maui-8LChNbDvTnxGHnFnUt1vBw==/lib/arm64/libmono-profiler-aot.so'
05-21 11:38:30.033 28555 28555 W monodroid: Looking for profiler init symbol 'mono_profiler_init_aot'? 0x7826470468
...
05-21 11:38:35.531 28555 28586 I mono-prof: AOT profiler data written to 'socket'
05-21 11:38:35.534 28555 28586 E mono-prof: aot profiler data saved to the socket
```

Which could fail when loading the `libmono-profiler-aot.so` library,
finding the `mono_profiler_init_aot` symbol, or just not return any
data...

To fix this, you will need a new build of `libmono-profiler-aot.so`.
We usually have to ship a new version of `Mono.Profiler.Android` with
each major .NET version.

See some past examples at:

* https://github.com/jonathanpeppers/Mono.Profiler.Android/pull/17
* https://github.com/jonathanpeppers/Mono.Profiler.Android/pull/23

### Notes about NuGet caches

To get a clear picture of before & after, you can copy over top of the
previous profile with a command such as:

```powershell
cp -Verbose src\Controls\src\Build.Tasks\nuget\buildTransitive\netstandard2.0\*.aotprofile ~\.nuget\packages\microsoft.maui.controls.build.tasks\9.0.100-preview.2-dev\buildTransitive\netstandard2.0\
```

Note that sometimes stale builds of MAUI can exist in your
`%NUGET_PACKAGES%` directory, sometimes I even run a command to
manually clear it completely of all `*-dev` directories:

```powershell
rm -r ~\.nuget\packages\*\*-dev\
```

### How to verify specific methods are AOT'd

To verify what methods are AOT'd, clear the log and enable AOT logging:

```bash
adb logcat -c
adb shell setprop debug.mono.log default,timing=bare,assembly,mono_log_level=debug,mono_log_mask=aot
```

Restart the app, and you should be able to see messages like:

```bash
$ adb logcat -d | grep AOT
02-23 09:03:46.327 10401 10401 D Mono    : AOT: FOUND method Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView:.ctor () [0x6f9efd0150 - 0x6f9efd0340 0x6f9efd260c]
```

Look for any suspicious `AOT NOT FOUND` messages.

Note that it is expected that some methods will say `(wrapper)`:

```bash
02-23 09:03:46.327 10401 10401 D Mono    : AOT NOT FOUND: (wrapper runtime-invoke) object:runtime_invoke_void (object,intptr,intptr,intptr).
02-23 09:03:46.334 10401 10401 D Mono    : AOT NOT FOUND: (wrapper managed-to-native) System.Diagnostics.Debugger:IsAttached_internal ().
02-23 09:03:46.367 10401 10401 D Mono    : AOT NOT FOUND: (wrapper native-to-managed) Android.Runtime.JNINativeWrapper:Wrap_JniMarshal_PPL_V (intptr,intptr,intptr).
```

[0]: ../../.github/DEVELOPMENT.md#compile-using-a-local-bindotnet
[1]: ../../eng/scripts/profile-android.ps1
[2]: https://github.com/jonathanpeppers/maui-profiling/blob/main/scripts/profile.ps1
