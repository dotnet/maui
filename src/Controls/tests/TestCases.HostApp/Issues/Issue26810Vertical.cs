namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26810, "Scroll To first item in CollectionView with vertical orientation when updating the collection with KeepItemsInView", PlatformAffected.Android, issueTestNumber: 1)]
public class Issue26810Vertical : Issue26810Page
{
	public Issue26810Vertical() : base(ItemsLayoutOrientation.Vertical)
	{
	}
}
