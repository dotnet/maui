rem This is not our official nuget build script.
rem This is used as a quick and dirty way create nuget packages used to test user issue reproductions.
rem This is updated as XF developers use it to test reproductions. As such, it may not always work.
rem This is not ideal, but it's better than nothing, and it usually works fine.

@echo off
rem stub uncommon targets
set NUGET_EXE=%NUGET_DIR%NuGet.exe

if "%1" == "droid" (
   set CONFIG=debug
   call .create-stubs.bat
   %NUGET_EXE% restore .xamarin.forms.android.nuget.sln
   msbuild /v:m /p:platform="any cpu" /p:WarningLevel=0 /p:CreateAllAndroidTargets=true .xamarin.forms.android.nuget.sln
)
if "%1" == "rdroid" (
   set CONFIG=release
   call .create-stubs.bat
   %NUGET_EXE% restore .xamarin.forms.android.nuget.sln
   msbuild /v:m /p:configuration=release /p:platform="any cpu" /p:WarningLevel=0 /p:CreateAllAndroidTargets=true .xamarin.forms.android.nuget.sln
)
if "%1" == "pdroid" (
   set CONFIG=release
   msbuild /v:m /p:configuration=release /p:platform="anyCpu" /p:WarningLevel=0 /p:CreateAllAndroidTargets=true Xamarin.Forms.Platform.Android\Xamarin.Forms.Platform.Android.csproj
)
if "%1" == "ios" (
   set CONFIG=debug
   call .create-stubs.bat
   %NUGET_EXE% restore .xamarin.forms.ios.nuget.sln
   msbuild /v:m /p:platform="any cpu" .xamarin.forms.ios.nuget.sln
)
if "%1" == "droidios" (
   set CONFIG=debug
   call .create-stubs.bat
   %NUGET_EXE% restore .xamarin.forms.android.nuget.sln
   %NUGET_EXE% restore .xamarin.forms.ios.nuget.sln
   msbuild /v:m /p:platform="any cpu" /p:WarningLevel=0 .xamarin.forms.android.nuget.sln
   msbuild /v:m /p:platform="any cpu" .xamarin.forms.ios.nuget.sln
)
if "%1" == "uap" (
   set CONFIG=debug
   call .create-stubs.bat
   %NUGET_EXE% restore .xamarin.forms.uap.nuget.sln
   msbuild /v:m /p:platform="any cpu" .xamarin.forms.uap.nuget.sln /t:restore
   msbuild /v:m /p:platform="any cpu" .xamarin.forms.uap.nuget.sln
)
if "%1" == "all" (
   set CONFIG=debug
   call .create-stubs.bat
   %NUGET_EXE% restore .xamarin.forms.sln
   msbuild /v:m /p:platform="any cpu" .xamarin.forms.uap.nuget.sln /t:restore
   msbuild /v:m /p:platform="any cpu" /p:WarningLevel=0 /p:CreateAllAndroidTargets=true .xamarin.forms.nuget.sln
)

if "%DEBUG_VERSION%"=="" set DEBUG_VERSION=0
set /a DEBUG_VERSION=%DEBUG_VERSION%+1
pushd .nuspec
%NUGET_EXE% pack Xamarin.Forms.nuspec -properties configuration=%CONFIG%;platform=anycpu -Version 9.9.%DEBUG_VERSION%
if "%CREATE_MAP_NUGET%" NEQ "" (
REM Requires building x86, x64, AMD
	%NUGET_EXE% pack Xamarin.Forms.Maps.nuspec -properties configuration=%CONFIG%;platform=anycpu -Version 9.9.%DEBUG_VERSION%
)
popd
