using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.CollectionView;

[Test(
	id: "I7",
	title: "Pull To Refresh.",
	category: Category.CollectionView)]

public partial class I7_PullToRefresh : ContentPage
{
	public ICommand NavigateCommand { get; private set; }
	public I7_PullToRefresh()
	{
		InitializeComponent();

		NavigateCommand = new Command<Type>(
				async (Type pageType) =>
				{
					Page page = (Page)Activator.CreateInstance(pageType);
					await Navigation.PushAsync(page);
				});

		BindingContext = this;
	}
}