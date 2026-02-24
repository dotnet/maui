using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.CollectionView;

[Test(
	id: "I8",
	title: "Headers and Footers.",
	category: Category.CollectionView)]

public partial class I8_HeaderAndFooters : ContentPage
{
	public ICommand NavigateCommand { get; private set; }
	public I8_HeaderAndFooters()
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