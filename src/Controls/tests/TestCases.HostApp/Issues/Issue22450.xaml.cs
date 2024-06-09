using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22450, "[Android] Custom shell back button is always white in color", PlatformAffected.All)]
	public partial class Issue22450 : Shell
	{
		public Issue22450()
		{
			InitializeComponent();
			Routing.RegisterRoute("detail",typeof(Issue22460Detail));
			GoToAsync("detail");
		}
	}

	public class Issue22460Detail : ContentPage
	{
		public Issue22460Detail()
		{
			Shell.SetForegroundColor(this, Colors.Blue);

            var backButtonBehavior = new BackButtonBehavior
            {
                IconOverride = "groceries.png"
            };

            Shell.SetBackButtonBehavior(this, backButtonBehavior);
			Content = new StackLayout()
			{
				new Label()
				{
					AutomationId="label",
					Text="Hello MAUI!"
				}
			};
		}
	}
}