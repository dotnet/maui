using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

public partial class Issue21630_navPage : ContentPage
{
	Page _page;
	List<Page> _modalStack;

	public Issue21630_navPage()
	{
		InitializeComponent();
		_page = Shell.Current.BindingContext as Page;
	}

	public Issue21630_navPage(Page page, List<Page> modalStack)
	{
		InitializeComponent();
		_page = page;
		_modalStack = modalStack;
	}

	void FocusNavBarEntryNav (object sender, EventArgs e)
	{
		NavBarEntryNav.Focus();
	}

	void FocusNavBarEntryShell (object sender, EventArgs e)
	{
		NavBarEntryShell.Focus();
	}

	async void RestoreMainPage (object sender, EventArgs e)
	{
		Application.Current.MainPage = _page;
		await Task.Yield();

		foreach(var page in _modalStack)
		{
			await _page.Navigation.PushModalAsync(page);
		}
	}
}
