using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Controls
{
	public class RedButton : Button
	{
		static RedButton()
		{
			RedServiceBuilder.TryAddHandler<RedButton, RedButtonHandler>();
		}
	}
}