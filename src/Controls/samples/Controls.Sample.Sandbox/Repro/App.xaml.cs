using System.Diagnostics;
using AllTheLists.Models;
using AllTheLists.Models.Learning;
using Fonts;

namespace AllTheLists;

public partial class App : Application
{
    

    public App()
	{
		InitializeComponent();

		MainPage = new NavigationPage(new MainPage());
	}
}
