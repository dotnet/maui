namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30961, "Setting IsTextPredictionEnabled to false for a SearchBar is not working", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue30961 : ContentPage
{
	public Issue30961()
	{
		Content = new StackLayout()
		{
			Children =
				{
					new SearchBar()
					{
						AutomationId = "SearchBar",
						Placeholder = "Type here...",
						IsTextPredictionEnabled=false,
						IsSpellCheckEnabled=false,
					}
				}
		};
	}
}