using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21630, "Entries in NavBar don't trigger keyboard scroll", PlatformAffected.iOS)]
public partial class Issue21630 : ContentPage
{
	Page _page;
	List<Page> _modalStack;

	public Issue21630()
	{
		InitializeComponent();
		Loaded += OnLoaded;
	}

	private void OnLoaded(object sender, EventArgs e)
	{
		_page = Application.Current.MainPage;
		_modalStack = Navigation.ModalStack.ToList();
	}

	void SwapMainPageNav (object sender, EventArgs e)
	{
		Application.Current.MainPage = new NavigationPage(new Issue21630_navPage(_page, _modalStack));
	}

	void SwapMainPageShell (object sender, EventArgs e)
	{
		Application.Current.MainPage = new Issue21630_shellPage(_page, _modalStack);
	}
}
