
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery
{
	[Preserve(AllMembers = true)]
	internal class PerformanceScenario
	{
		public View View { get; set; }
		public string Name { get; private set; }

		public PerformanceScenario() { }
		public PerformanceScenario(string name)
		{
			Name = name;
		}
	}
}
