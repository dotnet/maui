using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.CollectionView;

[Test(
	id: "I3",
	title: "Sizing.",
	category: Category.CollectionView)]

public partial class I3_Sizing : ContentPage
{
	public ICommand NavigateCommand { get; private set; }
	public I3_Sizing()
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