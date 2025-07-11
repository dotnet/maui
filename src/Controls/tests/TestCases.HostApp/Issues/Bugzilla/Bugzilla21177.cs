#if IOS
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 21177, "Using a UICollectionView in a ViewRenderer results in issues with selection")]
public class Bugzilla21177 : TestContentPage
{
	public class CollectionView : View
	{
		public event EventHandler<int> ItemSelected;

		public void InvokeItemSelected(int index)
		{
			ItemSelected?.Invoke(this, index);
		}
	}

	protected override void Init()
	{
		var view = new CollectionView() { AutomationId = "view" };
		view.ItemSelected += View_ItemSelected;
		Content = view;
	}

	private void View_ItemSelected(object sender, int e)
	{
		DisplayAlert("Success", "Success", "Cancel");
	}
}
#endif