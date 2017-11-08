REM e61c8dbb 
dir Xamarin.Forms.ControlGallery.iOS\bin\iPhone\Debug\XamarinFormsControlGalleryiOS.ipa
packages\Xamarin.UITest.2.1.4\tools\test-cloud.exe ^
submit Xamarin.Forms.ControlGallery.iOS\bin\iPhone\Debug\XamarinFormsControlGalleryiOS.ipa 13d622b3fbb487d8f5d0791278f0908d ^
--devices e61c8dbb ^
--series "refs/pull/1226/merge" ^
--locale "en_US" ^
--user kingces95@gmail.com ^
--assembly-dir Xamarin.Forms.Core.iOS.UITests\bin\Debug\ ^
--category manual