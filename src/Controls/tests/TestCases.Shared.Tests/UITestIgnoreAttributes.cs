using NUnit.Framework;

namespace Microsoft.Maui.TestCases.Tests
{
	public class FailsOnAllPlatforms : IgnoreAttribute
	{
		public FailsOnAllPlatforms(string reason) : base(reason)
		{
		}
	}

#if ANDROID
	public class FailsOnAndroid : IgnoreAttribute
	{
		public FailsOnAndroid(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnAndroid : CategoryAttribute
	{
		public FailsOnAndroid(string name) : base(name)
		{
		}
	}
#endif

#if IOS
	public class FailsOnIOS : IgnoreAttribute
	{
		public FailsOnIOS(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnIOS : CategoryAttribute
	{
		public FailsOnIOS(string name) : base(name)
		{
		}
	}
#endif

#if MACCATALYST
	public class FailsOnMac : IgnoreAttribute
	{
		public FailsOnMac(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnMac : CategoryAttribute
	{
		public FailsOnMac(string name) : base(name)
		{
		}
	}
#endif

#if WINDOWS
	public class FailsOnWindows : IgnoreAttribute
	{
		public FailsOnWindows(string reason) : base(reason)
		{
		}
	}
#else
	public class FailsOnWindows : CategoryAttribute
	{
		public FailsOnWindows(string description) : base(description)
		{
		}
	}
#endif
}
