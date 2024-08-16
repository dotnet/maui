using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	public abstract class TestNavigationPage : NavigationPage
	{
		protected TestNavigationPage()
		{
			Init();
		}

		protected abstract void Init();
	}
}