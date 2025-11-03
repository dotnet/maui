using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.CollectionView;

[Test(
	id: "I2",
	title: "Spacing.",
	category: Category.CollectionView)]

public partial class I2_Spacing : ContentPage
{
	public ICommand NavigateCommand { get; private set; }
	public I2_Spacing()
	{
		InitializeComponent();

		NavigateCommand = new Command<Type>(
				async ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type pageType) =>
				{
					Page page = (Page)Activator.CreateInstance(pageType);
					await Navigation.PushAsync(page);
				});

		BindingContext = this;
	}
}