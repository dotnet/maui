@echo off
PATH="C:\Program Files (x86)\Mono\bin";%PATH%

IF EXIST docs.xml (erase docs.xml)
for /r docs %%i in (*.xml) do type %%i >> docs.xml

echo "Updating Xamarin.Forms.Core"
tools\mdoc\mdoc update --delete Xamarin.Forms.Core\bin\Debug\netstandard2.0\Xamarin.Forms.Core.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Core > nul
IF %ERRORLEVEL% NEQ 0 (goto fail)

echo "Updating Xamarin.Forms.Xaml"
tools\mdoc\mdoc update --delete Xamarin.Forms.Xaml\bin\Debug\netstandard2.0\Xamarin.Forms.Xaml.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Xaml > nul
IF %ERRORLEVEL% NEQ 0 (goto fail)

echo "Updating Xamarin.Forms.Maps"
tools\mdoc\mdoc update --delete Xamarin.Forms.Maps\bin\Debug\netstandard2.0\Xamarin.Forms.Maps.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Maps > nul
IF %ERRORLEVEL% NEQ 0 (goto fail)

echo "Updating Xamarin.Forms.Pages"
tools\mdoc\mdoc update --delete Xamarin.Forms.Pages\bin\Debug\netstandard2.0\Xamarin.Forms.Pages.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Pages > nul
IF %ERRORLEVEL% NEQ 0 (goto fail)

IF EXIST _docs.xml (erase _docs.xml)
for /r docs %%i in (*.xml) do type %%i >> _docs.xml
fc docs.xml _docs.xml > nul 2> nul
IF %ERRORLEVEL% NEQ 0 (goto fail)

erase docs.xml _docs.xml
echo No changes detected.
exit /B 0

:fail
erase docs.xml _docs.xml
echo Changes detected!
exit /B 1

