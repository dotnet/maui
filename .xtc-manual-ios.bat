REM e61c8dbb is the device set used by CI for iOS testing
REM XTC_KEY is your XTC key
REM XTC_USER is you XTC login user name
REM If UITest version is updated in CI then it'll need to updated here
REM You'll also need to add [Category("manual")] to the test you want to run manually

dir Xamarin.Forms.ControlGallery.iOS\bin\iPhone\Debug\XamarinFormsControlGalleryiOS.ipa
packages\Xamarin.UITest.2.2.0\tools\test-cloud.exe ^
submit Xamarin.Forms.ControlGallery.iOS\bin\iPhone\Debug\XamarinFormsControlGalleryiOS.ipa %XTC_KEY% ^
--devices e61c8dbb ^
--series "manual" ^
--locale "en_US" ^
--user %XTC_USER% ^
--assembly-dir Xamarin.Forms.Core.iOS.UITests\bin\Debug\ ^
--category manual