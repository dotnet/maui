using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34499 : ContentView
{
	public Maui34499() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		// Diagnostic managed-tree coverage; this does not verify Mac Catalyst rendering.
		[Theory]
		[XamlInflatorData]
		internal void DelayedBindingContextPopulatesBindableLayoutAndObservesAdditions(XamlInflator inflator)
		{
			var view = new Maui34499(inflator);
			var model = new Maui34499ViewModel();

			Assert.Null(view.BindingContext);
			Assert.Null(view.ItemsLayout.BindingContext);
			Assert.Null(BindableLayout.GetItemsSource(view.ItemsLayout));
			Assert.Empty(view.ItemsLayout.Children);

			// Match the repro: assign on the ContentView only after InitializeComponent.
			view.BindingContext = model;
			AssertItems(view, model);

			model.Items.Add("Item 6 - Fig");
			AssertItems(view, model);
		}

		static void AssertItems(Maui34499 view, Maui34499ViewModel model)
		{
			Assert.Same(model, view.BindingContext);
			Assert.Same(model, view.ItemsLayout.BindingContext);
			Assert.Same(model.Items, BindableLayout.GetItemsSource(view.ItemsLayout));
			Assert.Equal(model.Items.Count, view.ItemsLayout.Children.Count);
			Assert.Same(model, view.CountLabel.BindingContext);
			Assert.Equal($"Total items: {model.Items.Count}", view.CountLabel.Text);

			for (var i = 0; i < model.Items.Count; i++)
			{
				var border = Assert.IsType<Microsoft.Maui.Controls.Border>(view.ItemsLayout.Children[i]);
				var label = Assert.IsType<Label>(border.Content);
				Assert.Same(model.Items[i], border.BindingContext);
				Assert.Same(model.Items[i], label.BindingContext);
				Assert.Equal(model.Items[i], label.Text);
			}
		}
	}
}

public class Maui34499ViewModel
{
	public ObservableCollection<string> Items { get; } = new()
	{
		"Item 1 - Apple",
		"Item 2 - Banana",
		"Item 3 - Cherry",
		"Item 4 - Date",
		"Item 5 - Elderberry"
	};
}
