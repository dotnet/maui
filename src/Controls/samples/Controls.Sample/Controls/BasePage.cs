using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Controls
{
	public class BasePage : ContentPage, IPage
	{
		IView IPage.View
		{
			get => Content;
			set => Content = (View)value;
		}
	}
}