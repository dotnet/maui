rem This is not our official nuget build script.
rem This is used as a quick and dirty way create nuget packages used to test user issue reproductions.
rem This is updated as XF developers use it to test reproductions. As such, it may not always work.
rem This is not ideal, but it's better than nothing, and it usually works fine.

mkdir Xamarin.Forms.Platform.MacOS\bin\%CONFIG%\
mkdir Xamarin.Forms.Platform.Tizen\bin\%CONFIG%\tizen40\
mkdir Xamarin.Forms.Maps.Tizen\bin\%CONFIG%\Tizen40
mkdir Xamarin.Forms.Maps.MacOS\bin\%CONFIG%
mkdir Xamarin.Forms.Platform.UAP\bin\%CONFIG%\
mkdir Xamarin.Forms.Platform.ios\bin\%CONFIG%\
mkdir Stubs\Xamarin.Forms.Platform.iOS\bin\iPhone\%CONFIG%\
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ar
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ca
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\cs
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\da
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\de
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\el
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\es
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\fi
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\fr
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\he
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\hi
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\hr
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\hu
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\id
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\it
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ja
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ko
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ms
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\nb
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\nl
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\pl
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\pt-BR
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\pt
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ro
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ru
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\sk
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\sv
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\th
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\tr
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\uk
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\vi
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\zh-Hans
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\zh-Hant
mkdir Xamarin.Forms.Platform.iOS\bin\%CONFIG%\zh-HK

mkdir Xamarin.Forms.Platform.Android\bin\%CONFIG%
echo foo > Xamarin.Forms.Platform.Android\bin\%CONFIG%\Xamarin.Forms.Platform.Android.dll

mkdir Xamarin.Forms.Platform.Android.FormsViewGroup\bin\%CONFIG%
echo foo > Xamarin.Forms.Platform.Android.FormsViewGroup\bin\%CONFIG%\FormsViewGroup.dll

mkdir Stubs\Xamarin.Forms.Platform.Android\bin\%CONFIG%
echo foo > Stubs\Xamarin.Forms.Platform.Android\bin\%CONFIG%\Xamarin.Forms.Platform.dll

echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ar\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ca\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\cs\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\da\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\de\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\el\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\es\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\fi\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\fr\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\he\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\hi\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\hr\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\hu\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\id\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\it\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ja\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ko\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ms\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\nb\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\nl\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\pl\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\pt-BR\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\pt\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ro\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\ru\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\sk\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\sv\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\th\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\tr\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\uk\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\vi\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\zh-Hans\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\zh-Hant\Xamarin.Forms.Platform.iOS.resources.dll
echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\zh-HK\Xamarin.Forms.Platform.iOS.resources.dll

echo foo > Xamarin.Forms.Platform.iOS\bin\%CONFIG%\Xamarin.Forms.Platform.iOS.dll
echo foo > Stubs\Xamarin.Forms.Platform.iOS\bin\iPhone\%CONFIG%\Xamarin.Forms.Platform.dll

echo foo > Xamarin.Forms.Platform.MacOS\bin\%CONFIG%\Xamarin.forms.Platform.macOS.dll
echo foo > Xamarin.Forms.Platform.MacOS\bin\%CONFIG%\Xamarin.forms.Platform.dll
echo foo > Xamarin.Forms.Maps.MacOS\bin\%CONFIG%\Xamarin.Forms.Maps.macOS.dll

mkdir Stubs\Xamarin.Forms.Platform.Tizen\bin\%CONFIG%\tizen40
echo foo > Stubs\Xamarin.Forms.Platform.Tizen\bin\%CONFIG%\tizen40\Xamarin.Forms.Platform.dll
echo foo > Xamarin.Forms.Maps.Tizen\bin\%CONFIG%\Tizen40\Xamarin.Forms.Maps.Tizen.dll

mkdir Xamarin.Forms.Platform.Tizen\bin\%CONFIG%\tizen40
echo foo > Xamarin.Forms.Platform.Tizen\bin\%CONFIG%\tizen40\Xamarin.forms.Platform.tizen.dll
echo foo > Xamarin.Forms.Platform.Tizen\bin\%CONFIG%\tizen40\Xamarin.forms.Platform.dll

mkdir Xamarin.Forms.Platform.UAP\bin\%CONFIG%\
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\Xamarin.Forms.Platform.UAP.dll
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\Xamarin.Forms.Platform.UAP.pri
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\Xamarin.Forms.Platform.UAP.xr.xml
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\FormsProgressBarStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\FormsFlyout.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\FormsCommandBarStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\Resources.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\FormsTextBoxStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\AutoSuggestStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\SliderStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\MasterDetailControlStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\PageControlStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\TabbedPageStyle.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\FormsEmbeddedPageWrapper.xbf
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\StepperControl.xbf

mkdir Xamarin.Forms.Platform.UAP\bin\%CONFIG%\Items
echo foo > Xamarin.Forms.Platform.UAP\bin\%CONFIG%\Items\ItemsViewStyles.xbf
            