using NUnit.Framework;

namespace Microsoft.Maui.TestCases.Tests
{
	public class FailsOnAllPlatforms : IgnoreAttribute
	{
		public FailsOnAllPlatforms() : base(nameof(FailsOnAndroid))
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
}