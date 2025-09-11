using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.CollectionView;

[Test(
	id: "I9",
	title: "Scrolling.",
	category: Category.CollectionView)]
public partial class I9_Scrolling : ContentPage
{
	public ICommand NavigateCommand { get; private set; }
	public I9_Scrolling()
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