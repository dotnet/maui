using System.Collections.Generic;
using System.Reflection;

namespace Xamarin.Forms.Controls.Tests
{
	public interface IPlatformTestSettings
	{
		Assembly Assembly { get; }
		Dictionary<string, object> TestRunSettings { get; }
	}
}
