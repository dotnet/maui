using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class CollectionViewDataTypeOverride
{
	public CollectionViewDataTypeOverride() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] 
		public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ControlDataTypeOverridesDataTemplateDataType([Values] XamlInflator inflator)
		{
			var page = new CollectionViewDataTypeOverride(inflator);
			
			// Set up binding context with a MockViewModel
			var viewModel = new MockViewModel
			{
				I = 42,
				Items = new List<MockItemViewModel>
				{
					new MockItemViewModel { Title = "Item 1" },
					new MockItemViewModel { Title = "Item 2" },
				}.ToArray()
			};
			
			page.BindingContext = viewModel;
			
			// Get the item template and create content
			var itemTemplate = page.collectionView.ItemTemplate;
			var templateContent = itemTemplate.CreateContent() as VerticalStackLayout;
			Assert.That(templateContent, Is.Not.Null);
			
			// Find the labels
			var labelFromModel = templateContent.FindByName<Label>("labelFromModel");
			var labelFromViewModel = templateContent.FindByName<Label>("labelFromViewModel");
			
			Assert.That(labelFromModel, Is.Not.Null);
			Assert.That(labelFromViewModel, Is.Not.Null);
			
			// Check that the bindings are compiled (TypedBinding indicates compilation)
			if (inflator == XamlInflator.XamlC || inflator == XamlInflator.SourceGen)
			{
				// labelFromModel should bind to MockItemViewModel.Title
				var bindingFromModel = labelFromModel.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(bindingFromModel, Is.TypeOf<TypedBinding<MockItemViewModel, string>>());
				
				// labelFromViewModel should bind to MockViewModel.I (as int)
				// This is the key test - the Label's x:DataType should override the DataTemplate's x:DataType
				var bindingFromViewModel = labelFromViewModel.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(bindingFromViewModel, Is.TypeOf<TypedBinding<MockViewModel, int>>());
			}
			
			// Set binding context to test runtime behavior
			// The DataTemplate sets the BindingContext to the item (MockItemViewModel)
			templateContent.BindingContext = viewModel.Items[0];
			
			// Test that the first label correctly binds to the item's Title
			Assert.That(labelFromModel.Text, Is.EqualTo("Item 1"));
			
			// THIS IS THE KEY TEST: 
			// The user expectation: x:DataType on the Label should override the DataTemplate's x:DataType
			// and make the binding automatically look for the correct binding context up the visual tree
			// 
			// Currently this FAILS because x:DataType only affects compile-time type checking,
			// not runtime binding source resolution
			Assert.That(labelFromViewModel.Text, Is.EqualTo("42"), 
				"Label with x:DataType override should automatically find the correct binding context");
		}
	}
}
