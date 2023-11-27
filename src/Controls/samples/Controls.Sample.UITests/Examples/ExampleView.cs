using Microsoft.Maui.Controls;
using ExampleFramework.Tooling;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class ExampleView : ContentView
	{
		public ExampleView(UIExample example)
		{
			AutomationId = example.Title;

			object exampleContent = example.Create();
			Content = (View) exampleContent;
		}
	}
}