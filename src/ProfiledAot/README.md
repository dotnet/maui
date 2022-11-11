# Profiled AOT support for Android

This is based on the NuGet package found here:

https://github.com/jonathanpeppers/Mono.Profiler.Android#usage-of-the-aot-profiler

## Updating Profiles

Build MAUI following the instructions at [DEVELOPMENT.md][0]. Make
sure to build with `--configuration=Release`.

Run the `Record` target on each project:

```bash
$ ./bin/dotnet/dotnet build src/ProfiledAot/build.proj -p:App=maui
$ ./bin/dotnet/dotnet build src/ProfiledAot/build.proj -p:App=maui-blazor
```

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
$ mkdir foo && cd foo
$ ../bin/dotnet/dotnet new maui
$ ../bin/dotnet/dotnet build -c Release -t:Run -f net7.0-android
```

Run the app a few times and make sure you get good launch times:

```bash
$ adb logcat -d | grep Displayed
02-22 15:50:50.502  1802  1962 I ActivityTaskManager: Displayed com.companyname.foo/crc64808a40cc7e533249.MainActivity: +477ms
02-22 15:50:51.703  1802  1962 I ActivityTaskManager: Displayed com.companyname.foo/crc64808a40cc7e533249.MainActivity: +477ms
02-22 15:50:52.926  1802  1962 I ActivityTaskManager: Displayed com.companyname.foo/crc64808a40cc7e533249.MainActivity: +477ms
```

You can also use [profile-android.ps1][1] in this repo, or [profile.ps1][2].

To verify what methods are AOT'd, clear the log and enable AOT logging:

```bash
$ adb logcat -c
$ adb shell setprop debug.mono.log default,timing=bare,assembly,mono_log_level=debug,mono_log_mask=aot
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
