using System.Collections.Generic;

namespace Xamarin.Forms.UITest.TestCloud
{
	internal static class DeviceSets
	{
		public static readonly DeviceSet AndroidMassiveSet =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.Android }, "ddaf5646", new List<string>
			{
				"150+ devices!"
			});

		public static readonly DeviceSet IOsMassiveSet =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "cc20a257",
				new List<string>
				{
					"60+ devices!"
				});

		// Android
		public static readonly DeviceSet AndroidFastParallel =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.Android }, "7e376fe0", new List<string>
			{
				"LG Nexus 5 - Android 4.4.4"
			});

		public static readonly DeviceSet Android5 = new DeviceSet(
			new List<DeviceSet.Platform> { DeviceSet.Platform.Android }, "f2d90195", new List<string>
			{
				"LG Nexus 5- Android 5.1.1"
			});

		public static readonly DeviceSet Android6 = new DeviceSet(
			new List<DeviceSet.Platform> { DeviceSet.Platform.Android }, "012c8f06", new List<string>
			{
				"LG Nexus 5 - Android 6.0.1"
			});

		public static readonly DeviceSet SethLocalDeviceSetAndroid =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.Android }, "9e76018b", new List<string>
			{
				"Google Nexus 7 - 4.4.2",
				"Samsung Galaxy S4 (Google Play Edition GT-I9505G) - Seth -> 4.4, Test Cloud -> 4.4.2 (closest match)"
			});

		public static readonly DeviceSet IcsSmallSet =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.Android }, "fc61dbe3", new List<string>
			{
				"Amazon Kindle Fire (2nd Gen) - 4.0.3",
				"HTC Desire - 4.0.3",
				"Samsung Galaxy Tab 2 - 4.0.4",
				"Sony Xperia neo L - 4.0.3"
			});

		public static readonly DeviceSet AndroidAllApiSmallSet =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.Android }, "4f912d99", new List<string>
			{
				"HTC Sensation XL - 4.0.3",
				"Amazon Kindle Fire (2nd Generation) - 4.0.3",
				"Samsung Galaxy Tab 2 - 4.0.4",
				"Motorola ATRIX 2 - 4.0.4",
				"Samsung Galaxy Note II - 4.1.1",
				"HP Slate 7 - 4.1.1",
				"Sony Xperia Z - 4.1.2",
				"Samsung Galaxy Tab 3 7.0 - 4.1.2",
				"Google Nexus 7 - 4.2",
				"Oppo R819 - 4.2.1",
				"Samsung Galaxy S4 Zoom - 4.2.2",
				"Acer Iconia Tab A1 - 4.2.2",
				"Samsung Galaxy S4 (Octo-core) - 4.3",
				"Oppo N1 - 4.3",
				"LG Nexus 5 - 4.4",
				"Samsung Google Nexus 10 - 4.4",
				"Samsung Galaxy S5 - 4.4.2",
				"Samsung Galaxy Note - 3 (Octo-Core) - 4.4.2"
			});

		public static readonly DeviceSet IOsFastParallel =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "7f2fa3ae",
				new List<string>
				{
					"Apple iPad Mini Retina - iOS 7.1.2"
				});

		// iOS
		public static readonly DeviceSet SethLocalDeviceSetiOs =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "b417f165",
				new List<string>
				{
					"Apple iPhone 5 - iOS 7.1.1",
					"Apple iPad Air - iOS 7.1.1"
				});

		public static readonly DeviceSet IOsAllApiSmallSet =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "d2d6440e",
				new List<string>
				{
					"Apple iPod Touch 5th Gen - iOS 6.0.1",
					"Apple iPad 2 - iOS 6.1.3",
					"Apple iPhone 4 - iOS 6.1.3",
					"Apple iPhone 5 - iOS 6.1.4",
					"Apple iPhone 3GS - iOS 6.1.6",
					"Apple iPod Touch 4th Gen - iOS 6.1.6",
					"Apple iPhone 5C - iOS 7.0.2",
					"Apple iPad Mini - iOS 7.0.3",
					"Apple iPad 4 - iOS 7.0.4",
					"Apple iPhone 5S - iOS 7.0.4",
					"Apple iPad Air - iOS 7.1",
					"Apple iPhone 5C - iOS 7.1",
					"Apple iPad Air - iOS 7.1.1",
					"Apple iPhone 4S - iOS 7.1.1",
					"Apple iPhone 5 - iOS 7.1.1"
				});

		public static readonly DeviceSet IOs7 =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "ad7b679b",
				new List<string>
				{
					"Apple Ipad Retina Mini - iOS 7.1.2"
				});


		public static readonly DeviceSet IOs8 =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "b859db6b",
				new List<string>
				{
					"Apple Iphone 6 plus- iOS 8.4.1"
				});

		public static readonly DeviceSet IOs7AndiOs8 =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "5190fc68",
				new List<string>
				{
					"Apple Ipad Retina Mini - iOS 7.1.2",
					"Apple Iphone 6 Plus - 8.1.3"
				});

		public static readonly DeviceSet IOs6PhoneTablet =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "212c4682",
				new List<string>
				{
					"Apple Ipad 4 - iOS 6.1.3",
					"Apple Iphone 4 - iOS 6.1.3"
				});

		public static readonly DeviceSet IOs9 =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "dc3140fd",
				new List<string>
				{
					"Apple Iphone 6 - iOS 9.0",
					"Apple Ipad Air - iOS 9.0"
				});

		public static readonly DeviceSet IOs91 =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "7f4b13f8",
				new List<string>
				{
					"Apple Iphone 6S - iOS 9.1",
					"Apple Ipad Air 2 - iOS 9.1"
				});

		public static readonly DeviceSet IOs93 =
			new DeviceSet(new List<DeviceSet.Platform> { DeviceSet.Platform.IOs, DeviceSet.Platform.IOsClassic }, "3bd6076f",
				new List<string>
				{
					"Apple Iphone 5S - iOS 9.3.1"
				});
	}
}