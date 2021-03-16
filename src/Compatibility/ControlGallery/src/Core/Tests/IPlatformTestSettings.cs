using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests
{
	public interface IPlatformTestSettings
	{
		Assembly Assembly { get; }
		Dictionary<string, object> TestRunSettings { get; }
	}
}
