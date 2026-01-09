using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.BugFixes;

[Test(id: "L2", title: "TabBar Switching Test", category: Category.BugFixes)]
public partial class L2_TabBarSwitching : Shell
{
private static CategoryViewModel _savedCategoryViewModel;

public L2_TabBarSwitching()
{
InitializeComponent();

// Save the CategoryPage's ViewModel so we can recreate it
if (Application.Current?.MainPage is Shell shell)
{
var currentPage = shell.CurrentPage;
if (currentPage is CategoryPage categoryPage && categoryPage.BindingContext is CategoryViewModel vm)
{
_savedCategoryViewModel = vm;
}
}
}

private async void OnBackClicked(object sender, EventArgs e)
{
if (Application.Current != null)
{
await MainThread.InvokeOnMainThreadAsync(async () =>
{
// Create a new AppShell
var newAppShell = new AppShell();
Application.Current.MainPage = newAppShell;

// Wait for the shell to initialize
await Task.Delay(150);

// If we had a CategoryPage open, navigate back to it
if (_savedCategoryViewModel != null)
{
var categoryPage = new CategoryPage(_savedCategoryViewModel);
await newAppShell.Navigation.PushAsync(categoryPage, false);

// Clear the saved state
_savedCategoryViewModel = null;
}
});
}
}
}
