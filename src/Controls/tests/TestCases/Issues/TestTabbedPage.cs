using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	public abstract class TestTabbedPage : TabbedPage
	{
		protected TestTabbedPage()
		{
			Init();
		}

		protected abstract void Init();
	}
}