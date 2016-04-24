@echo off
PATH="C:\Program Files (x86)\Mono\bin";%PATH%

echo "Updating Xamarin.Forms.Core"
tools\mdoc\mdoc update --delete Xamarin.Forms.Core\bin\Debug\Xamarin.Forms.Core.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Core | findstr /r "^Members.Added:..," > tmpFile
set /p RESULT= < tmpFile
echo "%RESULT%"
del tmpFile

IF NOT "%RESULT%" == "Members Added: 0, Members Deleted: 0" (exit 1)

echo "Updating Xamarin.Forms.Xaml"
tools\mdoc\mdoc update --delete Xamarin.Forms.Xaml\bin\Debug\Xamarin.Forms.Xaml.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Xaml | findstr /r "^Members.Added:..," > tmpFile
set /p RESULT= < tmpFile
echo "%RESULT%"
del tmpFile

IF NOT "%RESULT%" == "Members Added: 0, Members Deleted: 0" (exit 1)

echo "Updating Xamarin.Forms.Maps"
tools\mdoc\mdoc update --delete Xamarin.Forms.Maps\bin\Debug\Xamarin.Forms.Maps.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Maps | findstr /r "^Members.Added:..," > tmpFile
set /p RESULT= < tmpFile
echo "%RESULT%"
del tmpFile

IF NOT "%RESULT%" == "Members Added: 0, Members Deleted: 0" (exit 1)

echo "Updating Xamarin.Forms.Pages"
tools\mdoc\mdoc update --delete Xamarin.Forms.Pages\bin\Debug\Xamarin.Forms.Pages.dll -L "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259" --out docs\Xamarin.Forms.Pages | findstr /r "^Members.Added:..," > tmpFile
set /p RESULT= < tmpFile
echo "%RESULT%"
del tmpFile

IF NOT "%RESULT%" == "Members Added: 0, Members Deleted: 0" (exit 1)

exit 0

