using System;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	class AndroidRunnerOptions
	{
		public AndroidRunnerOptions(Type type)
		{
			ActivityType = type;
		}

		public Type ActivityType { get; }
	}
}