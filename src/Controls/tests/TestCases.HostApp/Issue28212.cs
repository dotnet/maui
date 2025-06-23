using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28212, "Using CollectionView.EmptyView results in an Exception on Windows", PlatformAffected.WinPhone)]
public class Issue28212 : NavigationPage
{
	Issue28212_Page1 _issue28212_Page1;
	public Issue28212()
	{
		_issue28212_Page1 = new Issue28212_Page1();
		this.PushAsync(_issue28212_Page1);
	}
}

public class Issue28212_Page1 : ContentPage
{
	VerticalStackLayout verticalStackLayout;
	Issue28212_Page2 _issue28212_Page2;
	Button button;
	public Issue28212_Page1()
	{
		_issue28212_Page2 = new Issue28212_Page2();
		button = new Button();
		button.Text = "Click to Navigate";
		button.AutomationId = "Button";
		button.Clicked += Button_Clicked;
		button.HeightRequest = 100;
		verticalStackLayout = new VerticalStackLayout();
		verticalStackLayout.Children.Add(button);
		this.Content = verticalStackLayout;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(_issue28212_Page2);
	}
}

public class Issue28212_Page2 : ContentPage
{
	public ObservableCollection<string> Items { get; } = [];
	VerticalStackLayout verticalStackLayout;
	Button backButton;
	Button addButton;
	CollectionView _collectionView;
	Label emptyLabel;
	public Issue28212_Page2()
	{
		_collectionView = new CollectionView();
		_collectionView.ItemsSource = Items;
		emptyLabel = new Label();
		emptyLabel.Text = "Empty";
		_collectionView.EmptyView = emptyLabel;
		addButton = new Button();
		addButton.Text = "Add";
		addButton.Clicked += Button_Clicked;
		backButton = new Button();
		backButton.Text = "Back";
		backButton.Clicked += BackButton_Clicked;
		backButton.AutomationId = "BackButton";
		verticalStackLayout = new VerticalStackLayout();
		verticalStackLayout.Children.Add(addButton);
		verticalStackLayout.Children.Add(backButton);
		verticalStackLayout.Children.Add(_collectionView);
		this.Content = verticalStackLayout;
	}

	private void BackButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		Items.Add("Item " + (Items.Count + 1));
	}
}
