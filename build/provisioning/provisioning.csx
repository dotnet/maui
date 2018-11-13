if (!IsMac)
  return;

Item (XreItem.Xcode_10_1_0).XcodeSelect ();

AndroidSdk ()
  .ApiLevel (AndroidApiLevel.JellyBean)
  .ApiLevel (AndroidApiLevel.JellyBean_4_2)
  .ApiLevel (AndroidApiLevel.JellyBean_4_3)
  .ApiLevel (AndroidApiLevel.KitKat)
  .ApiLevel (AndroidApiLevel.Lollipop)
  .ApiLevel (AndroidApiLevel.Lollipop_5_1)
  .ApiLevel (AndroidApiLevel.Lollipop_5_2)
  .ApiLevel (AndroidApiLevel.Marshmallow)
  .ApiLevel (AndroidApiLevel.Nougat)
  .ApiLevel (AndroidApiLevel.Nougat_7_1)
  .ApiLevel (AndroidApiLevel.Oreo)
  .ApiLevel (AndroidApiLevel.Oreo_8_1)
  .SdkManagerPackage ("build-tools;25.0.0")
  .SdkManagerPackage ("build-tools;27.0.0")
  .SdkManagerPackage ("build-tools;28.0.0")
  .SdkManagerPackage ("extras;google;m2repository");


Item ("https://dl.xamarin.com/MonoFrameworkMDK/Macx86/MonoFramework-MDK-5.12.0.309.macos10.xamarin.universal.pkg");
Item ("https://dl.xamarin.com/MonoTouch/Mac/xamarin.ios-12.1.0.15.pkg");
Item ("https://dl.xamarin.com/XamarinforMac/Mac/xamarin.mac-5.0.0.0.pkg");
Item ("https://dl.xamarin.com/MonoforAndroid/Mac/xamarin.android-9.0.0-20.pkg");
Item ("https://dl.xamarin.com/VsMac/VisualStudioForMac-7.6.11.9.dmg");
