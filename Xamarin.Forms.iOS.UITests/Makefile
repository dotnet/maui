console:
	DEVICE_ENDPOINT=http://10.0.3.253:37265 BUNDLE_ID=com.xamarin.quickui.controlgallery calabash-ios console

run:
	mono ../packages/NUnit.Runners.2.6.3/tools/nunit-console.exe bin/Debug/Xamarin.Forms.iOS.UITests.dll

run-ios-phone:
	DEVICE_IDIOM=PHONE DEVICE_IP=192.168.1.145 DEVICE_ID=795a532d7fb4569fb728a79784eb73bbbca995ba mono ../packages/NUnit.Runners.2.6.3/tools/nunit-console-x86.exe -result=iphone-results.xml bin/Debug/Xamarin.Forms.iOS.UITests.dll

run-ios:
	DEVICE_IDIOM=PHONE mono ../packages/NUnit.Runners.2.6.3/tools/nunit-console-x86.exe -result=iphone-results.xml bin/Debug/Xamarin.Forms.iOS.UITests.dll --include "CITest";
	DEVICE_IDIOM=TABLET mono ../packages/NUnit.Runners.2.6.3/tools/nunit-console-x86.exe -result=ipad-results.xml bin/Debug/Xamarin.Forms.iOS.UITests.dll --include "CITest"
	
clean:
	rm -rf screenshot_* test_servers/ iphone-results.xml ipad-results.xml
