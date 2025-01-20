using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22570, "[iOS] Cross TabBar navigation broken", PlatformAffected.iOS)]
	public partial class Issue22570 : Shell
	{
		public Issue22570()
		{
			InitializeComponent();
			Routing.RegisterRoute("Bar22570", typeof(Issue22570Page));
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
			_ = GoToAsync("//main/Foo/Bar22570");
		}

		public class Issue22570Page : ContentPage
		{
			public Issue22570Page()
			{
				Content = new VerticalStackLayout()
				{
					new Label()
					{
						Text = "Hello, World!",
						AutomationId = "label"
					}
				};
			}
		}
	}
}