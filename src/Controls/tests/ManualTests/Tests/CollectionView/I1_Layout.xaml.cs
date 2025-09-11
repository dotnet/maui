using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.CollectionView;

[Test(
	id: "I1",
	title: "Layout.",
	category: Category.CollectionView)]

public partial class I1_Layout : ContentPage
{
	public ICommand NavigateCommand { get; private set; }
	public I1_Layout()
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