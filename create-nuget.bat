if "%DEBUG_VERSION%"=="" set DEBUG_VERSION=0
set /a DEBUG_VERSION=%DEBUG_VERSION%+1
pushd docs
..\tools\mdoc\mdoc.exe export-msxdoc -o Xamarin.Forms.Core.xml Xamarin.Forms.Core
..\tools\mdoc\mdoc.exe export-msxdoc -o Xamarin.Forms.Xaml.xml Xamarin.Forms.Xaml
..\tools\mdoc\mdoc.exe export-msxdoc -o Xamarin.Forms.Maps.xml Xamarin.Forms.Maps
popd
pushd .nuspec
..\.nuget\NuGet.exe pack Xamarin.Forms.nuspec -properties configuration=debug;platform=anycpu -Version 9.9.%DEBUG_VERSION%
popd
