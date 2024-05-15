using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	public abstract class TestContentPage : ContentPage
	{
		protected TestContentPage()
		{
			Init();
		}

		protected abstract void Init();
	}
}
