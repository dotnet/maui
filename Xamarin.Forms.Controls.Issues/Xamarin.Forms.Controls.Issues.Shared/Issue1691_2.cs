using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
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
			_searchBarWithSpellCheck.On<Windows>().SetIsSpellCheckEnabled(true);

			var searchBarWithoutSpellCheckLabel = new Label { Text = "SearchBar with SpellCheck Disabled:" };
			_searchBarWithoutSpellCheck = new SearchBar();
			_searchBarWithoutSpellCheck.On<Windows>().SetIsSpellCheckEnabled(false);

			var searchBarToggleSpellCheck = new Label { Text = "SearchBar with Toggled SpellCheck:" };
			_searchBarToggleSpellCheck = new SearchBar();

			_toggleSpellCheckButton = new Button { Text = "Enable SpellCheck" };
			_toggleSpellCheckButton.Clicked += (object sender, EventArgs e) => {
				if(_searchBarToggleSpellCheck.On<Windows>().IsSpellCheckEnabled())
				{
					_searchBarToggleSpellCheck.On<Windows>().DisableSpellCheck();
					_toggleSpellCheckButton.Text = "Enable SpellCheck";
				}
				else
				{
					_searchBarToggleSpellCheck.On<Windows>().EnableSpellCheck();
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
