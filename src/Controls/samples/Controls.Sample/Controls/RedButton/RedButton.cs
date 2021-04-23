using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Controls
{
	public class RedButton : Button
	{
		static RedButton()
		{
			var handlers = AppHost.Current.Handlers.GetCollection();

			handlers.AddHandler<RedButton, RedButtonHandler>();
		}
	}
}