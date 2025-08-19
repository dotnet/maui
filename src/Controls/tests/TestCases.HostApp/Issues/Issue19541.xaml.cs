using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19541, "[iOS] - Swipeview with collectionview issue", PlatformAffected.iOS)]
public partial class Issue19541 : ContentPage
{
    SwipeView _swipeView;
	public Issue19541()
	{
		InitializeComponent();
		LoadInitialList();
	}

	void LoadInitialList()
	{
		var list = new List<Issue19541Model>();
		for (int i = 0; i < 3; i++)
		{
			list.Add(new Issue19541Model
			{
				Name = $"Name {i}",
			});
		}

		collectionView.ItemsSource = list;
	}

	void OnRefreshClicked(object sender, EventArgs e)
	{
		var list = new List<Issue19541Model>();
		for (int i = 0; i < 3; i++)
		{
			list.Add(new Issue19541Model
			{
				Name = $"Name {i}",
			});
		}

		collectionView.ItemsSource = list;
	}

	void swipeView_Loaded(object sender, EventArgs e)
	{
		if (_swipeView is null)
			_swipeView = (SwipeView)sender;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		if (_swipeView == null)
			return;
		
		_swipeView.Open(OpenSwipeItem.RightItems);
	}
}


public class Issue19541Model
{
    public string Name { get; set; }
}

