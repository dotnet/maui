using Xunit;

namespace Microsoft.Maui.TestCases.Tests
{
	public class FailsOnAllPlatformsWhenRunningOnXamarinUITest : SkipAttribute
	{
		public FailsOnAllPlatformsWhenRunningOnXamarinUITest() : base(nameof(FailsOnAllPlatformsWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnAllPlatformsWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}

#if ANDROID
	public class FailsOnAndroidWhenRunningOnXamarinUITest : SkipAttribute
	{
		public FailsOnAndroidWhenRunningOnXamarinUITest() : base(nameof(FailsOnAndroidWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnAndroidWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnAndroidWhenRunningOnXamarinUITest : TraitAttribute
	{
		public FailsOnAndroidWhenRunningOnXamarinUITest() : base("FailsOnPlatform", nameof(FailsOnAndroidWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnAndroidWhenRunningOnXamarinUITest(string description) : base("FailsOnPlatform", description)
		{
		}
	}
#endif

#if IOS
	public class FailsOnIOSWhenRunningOnXamarinUITest : SkipAttribute
	{
		public FailsOnIOSWhenRunningOnXamarinUITest() : base(nameof(FailsOnIOSWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnIOSWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnIOSWhenRunningOnXamarinUITest : TraitAttribute
	{
		public FailsOnIOSWhenRunningOnXamarinUITest() : base("FailsOnPlatform", nameof(FailsOnIOSWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnIOSWhenRunningOnXamarinUITest(string description) : base(description)
		{
		}
	}
#endif

#if MACCATALYST
	public class FailsOnMacWhenRunningOnXamarinUITest : SkipAttribute
	{
		public FailsOnMacWhenRunningOnXamarinUITest() : base(nameof(FailsOnMacWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnMacWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnMacWhenRunningOnXamarinUITest : TraitAttribute
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
	public class FailsOnWindowsWhenRunningOnXamarinUITest : SkipAttribute
	{
		public FailsOnWindowsWhenRunningOnXamarinUITest() : base(nameof(FailsOnWindowsWhenRunningOnXamarinUITest))
		{
		}
		public FailsOnWindowsWhenRunningOnXamarinUITest(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnWindowsWhenRunningOnXamarinUITest : TraitAttribute
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