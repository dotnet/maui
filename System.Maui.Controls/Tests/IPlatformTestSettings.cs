using System.Collections.Generic;
using System.Reflection;

namespace System.Maui.Controls.Tests
{
	public interface IPlatformTestSettings
	{
		Assembly Assembly { get; }
		Dictionary<string, object> TestRunSettings { get; }
	}
}
