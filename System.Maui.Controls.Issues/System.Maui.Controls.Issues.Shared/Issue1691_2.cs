using System;
using System.Collections.Generic;
using System.Text;

using System.Maui;
using System.Maui.Internals;
using System.Maui.CustomAttributes;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.WindowsSpecific;

using WindowsOS = System.Maui.PlatformConfiguration.Windows;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1691, "[Enhancement] Add SearchBar platform specific for IsSpellCheckEnabled",
		PlatformAffected.UWP, issueTestNumber: 2)]
	public class Issue1691_2 : TestContentPage
	{
		SearchBar _searchBarWithSpellCheck;
		SearchBar _searchBarWithoutSpellCheck;
		SearchBar _searchBarToggleSpellCheck;
		Button _toggleSpellCheckButton;

		protected override void Init()
		{
			var searchBarWithSpellCheckLabel = new Label { Text = "SearchBar with SpellCheck Enabled:" };
			_searchBarWithSpellCheck = new SearchBar();
			_searchBarWithSpellCheck.On<WindowsOS>().SetIsSpellCheckEnabled(true);

			var searchBarWithoutSpellCheckLabel = new Label { Text = "SearchBar with SpellCheck Disabled:" };
			_searchBarWithoutSpellCheck = new SearchBar();
			_searchBarWithoutSpellCheck.On<WindowsOS>().SetIsSpellCheckEnabled(false);

			var searchBarToggleSpellCheck = new Label { Text = "SearchBar with Toggled SpellCheck:" };
			_searchBarToggleSpellCheck = new SearchBar();

			_toggleSpellCheckButton = new Button { Text = "Enable SpellCheck" };
			_toggleSpellCheckButton.Clicked += (object sender, EventArgs e) => {
				if(_searchBarToggleSpellCheck.On<WindowsOS>().IsSpellCheckEnabled())
				{
					_searchBarToggleSpellCheck.On<WindowsOS>().DisableSpellCheck();
					_toggleSpellCheckButton.Text = "Enable SpellCheck";
				}
				else
				{
					_searchBarToggleSpellCheck.On<WindowsOS>().EnableSpellCheck();
					_toggleSpellCheckButton.Text = "Disable SpellCheck";
				}
			};

			Content = new StackLayout {
				Children = {
					searchBarWithSpellCheckLabel,
					_searchBarWithSpellCheck,
					searchBarWithoutSpellCheckLabel,
					_searchBarWithoutSpellCheck,
					searchBarToggleSpellCheck,
					_searchBarToggleSpellCheck,
					_toggleSpellCheckButton
				}
			};
		}
	}
}
