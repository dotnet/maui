using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
public class Maui33293Product
{
	public string Name { get; set; } = string.Empty;
	public string Price { get; set; } = string.Empty;
	public bool IsSelected { get; set; }
}

public partial class Maui33293
{
	public Maui33293() => InitializeComponent();

	[Collection("Issue")]
	public class Maui33293Tests : IDisposable
	{
		bool bindingFailureReported = false;

		public Maui33293Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			RuntimeFeature.EnableMauiDiagnostics = true;
			BindingDiagnostics.BindingFailed += BindingFailed;
		}

		public void Dispose()
		{
			BindingDiagnostics.BindingFailed -= BindingFailed;
			DispatcherProvider.SetCurrent(null);
			AppInfo.SetCurrent(null);
		}

		void BindingFailed(object sender, BindingBaseErrorEventArgs args) =>
			bindingFailureReported = true;

		[Theory]
		[XamlInflatorData]
		internal void RadioButtonXReferenceBindingInDataTemplateShouldResolve(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				// SourceGen is out of scope for this runtime fix
				return;
			}

			var page = new Maui33293(inflator);

			// Provide one product so the DataTemplate instantiates a RadioButton
			var product = new Maui33293Product { Name = "Option 1", Price = "$0.99", IsSelected = true };
			BindableLayout.SetItemsSource(page.ProductStack, new[] { product });

			// Walk the visual tree: ProductStack → RadioButton → Content (HorizontalStackLayout) → Labels
			var radioButton = (RadioButton)page.ProductStack.Children[0];
			var hsl = (HorizontalStackLayout)radioButton.Content;
			var priceLabel = (Label)hsl.Children[0];
			var nameLabel = (Label)hsl.Children[1];

			// Labels must show product data — binding resolved successfully through x:Reference
			Assert.Equal("$0.99", priceLabel.Text);
			Assert.Equal("Option 1", nameLabel.Text);

			// No false-positive type-mismatch diagnostic must be raised
			Assert.False(bindingFailureReported);
		}
	}
}
