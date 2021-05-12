using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Controls
{
	public class RedButton : Button
	{
		static RedButton()
		{
			RedServiceBuilder.RegisterHandler<RedButton, RedButtonHandler>();
		}
	}
}