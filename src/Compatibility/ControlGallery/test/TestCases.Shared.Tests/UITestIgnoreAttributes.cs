using NUnit.Framework;

namespace UITests
{
	public class IgnoredDuringMoveToAppium : IgnoreAttribute
	{
		public IgnoredDuringMoveToAppium() : base(nameof(IgnoredDuringMoveToAppium))
		{
		}
		public IgnoredDuringMoveToAppium(string reason) : base(reason)
		{
		}
	}

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
		public FailsOnAndroid(string name) : base(name)
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
		public FailsOnIOS(string name) : base(name)
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
		public FailsOnMac(string name) : base(name)
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
		public FailsOnWindows(string name) : base(name)
		{
		}
	}
#endif
}
