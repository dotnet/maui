using NUnit.Framework;

namespace Microsoft.Maui.TestCases.Tests
{
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