using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp._1.Data;
using MauiApp._1.Models;
using MauiApp._1.Services;

namespace MauiApp._1.PageModels;

public partial class ManageMetaPageModel : ObservableObject
{
	private readonly CategoryRepository _categoryRepository;
	private readonly TagRepository _tagRepository;
    private readonly SeedDataService _seedDataService;

	[ObservableProperty]
	private ObservableCollection<Category> _categories = [];

	[ObservableProperty]
	private ObservableCollection<Tag> _tags = [];

	public ManageMetaPageModel(CategoryRepository categoryRepository, TagRepository tagRepository, SeedDataService seedDataService)
	{
		_categoryRepository = categoryRepository;
		_tagRepository = tagRepository;
        _seedDataService = seedDataService;
    }

	private async Task LoadData()
	{
		var categoriesList = await _categoryRepository.ListAsync();
		Categories = new ObservableCollection<Category>(categoriesList);
		var tagsList = await _tagRepository.ListAsync();
		Tags = new ObservableCollection<Tag>(tagsList);
	}

	[RelayCommand]
	private Task Appearing()
		=> LoadData();

	[RelayCommand]
	private async Task SaveCategories()
	{
		foreach (var category in Categories)
		{
			await _categoryRepository.SaveItemAsync(category);
		}

		await AppShell.DisplayToastAsync("Categories saved");
		SemanticScreenReader.Announce("Categories saved");
	}

	[RelayCommand]
	private async Task DeleteCategory(Category category)
	{
		Categories.Remove(category);
		await _categoryRepository.DeleteItemAsync(category);
		await AppShell.DisplayToastAsync("Category deleted");
		SemanticScreenReader.Announce("Category deleted");
	}

	[RelayCommand]
	private async Task AddCategory()
	{
		var category = new Category();
		Categories.Add(category);
		await _categoryRepository.SaveItemAsync(category);
		await AppShell.DisplayToastAsync("Category added");
		SemanticScreenReader.Announce("Category added");
	}

	[RelayCommand]
	private async Task SaveTags()
	{
		foreach (var tag in Tags)
		{
			await _tagRepository.SaveItemAsync(tag);
		}

		await AppShell.DisplayToastAsync("Tags saved");
		SemanticScreenReader.Announce("Tags saved");
	}

	[RelayCommand]
	private async Task DeleteTag(Tag tag)
	{
		Tags.Remove(tag);
		await _tagRepository.DeleteItemAsync(tag);
		await AppShell.DisplayToastAsync("Tag deleted");
		SemanticScreenReader.Announce("Tags deleted");
	}

	[RelayCommand]
	private async Task AddTag()
	{
		var tag = new Tag();
		Tags.Add(tag);
		await _tagRepository.SaveItemAsync(tag);
		await AppShell.DisplayToastAsync("Tag added");
		SemanticScreenReader.Announce("Tags added");
	}

	[RelayCommand]
	private async Task Reset()
	{
		Preferences.Default.Remove("is_seeded");
        await _seedDataService.LoadSeedDataAsync();
        Preferences.Default.Set("is_seeded", true);
        await Shell.Current.GoToAsync("//main");
	}
}