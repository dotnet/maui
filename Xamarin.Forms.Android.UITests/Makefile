console:
	calabash-android console ../Xamarin.Forms.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk

testcloud:
	mono xut-console.exe submit --assembly-dir ../Xamarin.Forms.Android.UITests/bin/debug ../Xamarin.Forms.ControlGallery.Android/bin/Release/AndroidControlGallery.AndroidControlGallery-Signed.apk 1a02b2e1ee5a2895e47da095cc895a49 7a47b813

run-phone-android:
	DEVICE_IDIOM=PHONE DEVICE_ID=9adaaf78 DEVICE_IP=192.168.1.144 mono ../packages/NUnit.Runners.2.6.3/tools/nunit-console-x86.exe -result=galaxy-results.xml bin/Debug/Xamarin.Forms.Android.UITests.dll

run-parallel-android:
	DEVICE_IDIOM=TABLET mono ../packages/NUnit.Runners.2.6.3/tools/nunit-console-x86.exe -result=nexus-results.xml bin/Debug/Xamarin.Forms.Android.UITests.dll --include "CITest" & 
	DEVICE_IDIOM=PHONE mono ../packages/NUnit.Runners.2.6.3/tools/nunit-console-x86.exe -result=galaxy-results.xml bin/Debug/Xamarin.Forms.Android.UITests.dll --include "CITest"

clean:
	rm -rf screenshot_* test_servers/ .irb-history nexus-results.xml galaxy-results.xml
