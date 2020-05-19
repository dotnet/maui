rem This is not our official nuget build script.
rem This is used as a quick and dirty way create nuget packages used to test user issue reproductions.
rem This is updated as XF developers use it to test reproductions. As such, it may not always work.
rem This is not ideal, but it's better than nothing, and it usually works fine.

mkdir System.Maui.Platform.MacOS\bin\%CONFIG%\
mkdir System.Maui.Platform.Tizen\bin\%CONFIG%\tizen40\
mkdir System.Maui.Maps.Tizen\bin\%CONFIG%\Tizen40
mkdir System.Maui.Maps.MacOS\bin\%CONFIG%
mkdir System.Maui.Platform.UAP\bin\%CONFIG%\
mkdir System.Maui.Platform.ios\bin\%CONFIG%\
mkdir Stubs\System.Maui.Platform.iOS\bin\iPhone\%CONFIG%\
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\ar
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\ca
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\cs
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\da
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\de
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\el
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\es
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\fi
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\fr
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\he
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\hi
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\hr
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\hu
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\id
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\it
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\ja
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\ko
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\ms
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\nb
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\nl
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\pl
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\pt-BR
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\pt
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\ro
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\ru
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\sk
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\sv
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\th
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\tr
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\uk
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\vi
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\zh-Hans
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\zh-Hant
mkdir System.Maui.Platform.iOS\bin\%CONFIG%\zh-HK



mkdir System.Maui.Platform.Android\bin\%CONFIG%
echo foo > System.Maui.Platform.Android\bin\%CONFIG%\System.Maui.Platform.Android.dll

mkdir System.Maui.Platform.Android.FormsViewGroup\bin\%CONFIG%
echo foo > System.Maui.Platform.Android.FormsViewGroup\bin\%CONFIG%\FormsViewGroup.dll

mkdir Stubs\System.Maui.Platform.Android\bin\%CONFIG%
echo foo > Stubs\System.Maui.Platform.Android\bin\%CONFIG%\System.Maui.Platform.dll



mkdir System.Maui.Platform.Android\bin\%CONFIG%\MonoAndroid90
echo foo > System.Maui.Platform.Android\bin\%CONFIG%\MonoAndroid90\System.Maui.Platform.Android.dll

mkdir System.Maui.Platform.Android.FormsViewGroup\bin\%CONFIG%\MonoAndroid90
echo foo > System.Maui.Platform.Android.FormsViewGroup\bin\%CONFIG%\MonoAndroid90\FormsViewGroup.dll

mkdir Stubs\System.Maui.Platform.Android\bin\%CONFIG%\MonoAndroid90
echo foo > Stubs\System.Maui.Platform.Android\bin\%CONFIG%\MonoAndroid90\System.Maui.Platform.dll


echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\ar\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\ca\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\cs\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\da\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\de\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\el\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\es\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\fi\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\fr\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\he\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\hi\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\hr\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\hu\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\id\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\it\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\ja\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\ko\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\ms\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\nb\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\nl\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\pl\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\pt-BR\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\pt\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\ro\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\ru\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\sk\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\sv\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\th\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\tr\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\uk\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\vi\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\zh-Hans\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\zh-Hant\System.Maui.Platform.iOS.resources.dll
echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\zh-HK\System.Maui.Platform.iOS.resources.dll

echo foo > System.Maui.Platform.iOS\bin\%CONFIG%\System.Maui.Platform.iOS.dll
echo foo > Stubs\System.Maui.Platform.iOS\bin\iPhone\%CONFIG%\System.Maui.Platform.dll

echo foo > System.Maui.Platform.MacOS\bin\%CONFIG%\system.maui.Platform.macOS.dll
echo foo > System.Maui.Platform.MacOS\bin\%CONFIG%\system.maui.Platform.dll
echo foo > System.Maui.Maps.MacOS\bin\%CONFIG%\System.Maui.Maps.macOS.dll

mkdir Stubs\System.Maui.Platform.Tizen\bin\%CONFIG%\tizen40
echo foo > Stubs\System.Maui.Platform.Tizen\bin\%CONFIG%\tizen40\System.Maui.Platform.dll
echo foo > System.Maui.Maps.Tizen\bin\%CONFIG%\Tizen40\System.Maui.Maps.Tizen.dll

mkdir System.Maui.Platform.Tizen\bin\%CONFIG%\tizen40
echo foo > System.Maui.Platform.Tizen\bin\%CONFIG%\tizen40\system.maui.Platform.tizen.dll
echo foo > System.Maui.Platform.Tizen\bin\%CONFIG%\tizen40\system.maui.Platform.dll

mkdir System.Maui.Platform.UAP\bin\%CONFIG%\
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\System.Maui.Platform.UAP.dll
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\System.Maui.Platform.UAP.pri
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\System.Maui.Platform.UAP.xr.xml
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\FormsProgressBarStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\FormsFlyout.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\FormsCommandBarStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\Resources.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\FormsTextBoxStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\AutoSuggestStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\SliderStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\MasterDetailControlStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\PageControlStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\TabbedPageStyle.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\FormsEmbeddedPageWrapper.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\StepperControl.xbf
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\FormsCheckBoxStyle.xbf

mkdir System.Maui.Platform.UAP\bin\%CONFIG%\Microsoft.UI.Xaml
mkdir System.Maui.Platform.UAP\bin\%CONFIG%\Microsoft.UI.Xaml\DensityStyles
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\Microsoft.UI.Xaml\DensityStyles\Compact.xbf

mkdir System.Maui.Platform.UAP\bin\%CONFIG%\Shell
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\Shell\ShellStyles.xbf

mkdir System.Maui.Platform.UAP\bin\%CONFIG%\CollectionView\
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\CollectionView\ItemsViewStyles.xbf

mkdir System.Maui.Platform.UAP\bin\%CONFIG%\Items
echo foo > System.Maui.Platform.UAP\bin\%CONFIG%\Items\ItemsViewStyles.xbf
