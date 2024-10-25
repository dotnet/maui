using NUnit.Framework;

namespace Microsoft.Maui.TestCases.Tests
{
	/*
	 * Use these attributes to ignore UITests that fail on all platforms or on a specific one. 
	 * Remember to leave a comment explaining the cause whenever possible.
	 * 
	 * .NET MAUI has UITests ported from Xamarin.UITests. If the test was already failing in Xamarin.Forms, 
	 * the WhenRunningOnXamarinUITest attribute is used.
	 */
	public class FailsOnAllPlatforms : IgnoreAttribute
	{
		public FailsOnAllPlatforms() : base(nameof(FailsOnAllPlatforms))
		{
		}
		public FailsOnAllPlatforms(string reason) : base(reason)
		{
		}
	}

#if ANDROID
	public class FailsOnAndroid : IgnoreAttribute
	{
		public FailsOnAndroid() : base(nameof(FailsOnAndroid))
		{
		}
		public FailsOnAndroid(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnAndroid : CategoryAttribute
	{
		public FailsOnAndroid() : base(nameof(FailsOnAndroid))
		{
		}
		public FailsOnAndroid(string description) : base(description)
		{
		}
	}
#endif

#if IOS
	public class FailsOnIOS : IgnoreAttribute
	{
		public FailsOnIOS() : base(nameof(FailsOnIOS))
		{
		}
		public FailsOnIOS(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnIOS : CategoryAttribute
	{
		public FailsOnIOS() : base(nameof(FailsOnIOS))
		{
		}
		public FailsOnIOS(string description) : base(description)
		{
		}
	}
#endif

#if MACCATALYST
	public class FailsOnMac : IgnoreAttribute
	{
		public FailsOnMac() : base(nameof(FailsOnMac))
		{
		}
		public FailsOnMac(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnMac : CategoryAttribute
	{
		public FailsOnMac() : base(nameof(FailsOnMac))
		{
		}
		public FailsOnMac(string description) : base(description)
		{
		}
	}
#endif

#if IOS || MACCATALYST
	public class FailsOnApple : IgnoreAttribute
	{
		public FailsOnApple() : base(nameof(FailsOnApple))
		{
		}
		public FailsOnApple(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnApple : CategoryAttribute
	{
		public FailsOnApple() : base(nameof(FailsOnApple))
		{
		}
		public FailsOnApple(string description) : base(description)
		{
		}
	}
#endif

#if WINDOWS
	public class FailsOnWindows : IgnoreAttribute
	{
		public FailsOnWindows() : base(nameof(FailsOnWindows))
		{
		}
		public FailsOnWindows(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnWindows : CategoryAttribute
	{
		public FailsOnWindows() : base(nameof(FailsOnWindows))
		{
		}
		public FailsOnWindows(string description) : base(description)
		{
		}
	}
#endif

	public class FailsOnAllPlatformsWhenRunningOnXamarinUITest : IgnoreAttribute
	{
		public FailsOnAllPlatformsWhenRunningOnXamarinUITest() : base(nameof(FailsOnAllPlatformsWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnAllPlatformsWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}

#if ANDROID
	public class FailsOnAndroidWhenRunningOnXamarinUITest : IgnoreAttribute
	{
		public FailsOnAndroidWhenRunningOnXamarinUITest() : base(nameof(FailsOnAndroidWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnAndroidWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnAndroidWhenRunningOnXamarinUITest : CategoryAttribute
	{
		public FailsOnAndroidWhenRunningOnXamarinUITest() : base(nameof(FailsOnAndroidWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnAndroidWhenRunningOnXamarinUITest(string description) : base(description)
		{
		}
	}
#endif

#if IOS
	public class FailsOnIOSWhenRunningOnXamarinUITest : IgnoreAttribute
	{
		public FailsOnIOSWhenRunningOnXamarinUITest() : base(nameof(FailsOnIOSWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnIOSWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnIOSWhenRunningOnXamarinUITest : CategoryAttribute
	{
		public FailsOnIOSWhenRunningOnXamarinUITest() : base(nameof(FailsOnIOSWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnIOSWhenRunningOnXamarinUITest(string description) : base(description)
		{
		}
	}
#endif

#if MACCATALYST
	public class FailsOnMacWhenRunningOnXamarinUITest : IgnoreAttribute
	{
		public FailsOnMacWhenRunningOnXamarinUITest() : base(nameof(FailsOnMacWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnMacWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnMacWhenRunningOnXamarinUITest : CategoryAttribute
	{
		public FailsOnMacWhenRunningOnXamarinUITest() : base(nameof(FailsOnMacWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnMacWhenRunningOnXamarinUITest(string description) : base(description)
		{
		}
	}
#endif

#if WINDOWS
	public class FailsOnWindowsWhenRunningOnXamarinUITest : IgnoreAttribute
	{
		public FailsOnWindowsWhenRunningOnXamarinUITest() : base(nameof(FailsOnWindowsWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnWindowsWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnWindowsWhenRunningOnXamarinUITest : CategoryAttribute
	{
		public FailsOnWindowsWhenRunningOnXamarinUITest() : base(nameof(FailsOnWindowsWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnWindowsWhenRunningOnXamarinUITest(string description) : base(description)
		{
		}
	}
#endif
}