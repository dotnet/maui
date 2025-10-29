using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		BindingContext = new MainViewModel();
	}

	private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is CategoryPageInfo issue)
		{
			var vm = (CategoryViewModel)Activator.CreateInstance(issue.Type);
			Navigation.PushAsync(new CategoryPage(vm));
		}

		var cv = sender as CollectionView;
		cv?.SelectedItem = null;
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		Uri uri = new("https://devdiv.visualstudio.com/DevDiv/_testPlans/define?planId=1802913&suiteId=1837197");
		await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
	}
}

public class MainViewModel
{
	[RequiresUnreferencedCode()]
	public MainViewModel()
	{
		var a = GetType().Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(CategoryViewModel)));
		Categories = GetType().Assembly.GetTypes()
			.Where(t => t.IsAssignableTo(typeof(CategoryViewModel)))
			.Select(t => new CategoryPageInfo(t, t.GetCustomAttribute<CategoryAttribute>()))
			.Where(t => t.Category is not null).ToList();
	}

	public List<CategoryPageInfo> Categories { get; set; } = new List<CategoryPageInfo>();
}

public record CategoryPageInfo(Type Type, CategoryAttribute Category);
