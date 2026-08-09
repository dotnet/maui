using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Regression test for https://github.com/dotnet/maui/issues/33291
///
/// Scenario:
///   A TapGestureRecognizer inside a CollectionView DataTemplate binds its Command
///   to the *page* BindingContext using Source={x:Reference thisPage}, while the
///   CommandParameter binds to the current DataTemplate item.
///
///   The DataTemplate has x:DataType="local:Maui33291Item" (the item model type).
///
/// Bug (regressed in .NET 10 with MauiEnableXamlCBindingWithSourceCompilation):
///   The source generator incorrectly compiled the Command binding using
///   x:DataType="Maui33291Item" as the source type, producing a TypedBinding
///   that could not find BindingContext.ItemTappedCommand on the item model.
///   The Command was null at runtime, so tapping an item did nothing.
///
/// Fix:
///   When a binding has Source={x:Reference ...}, skip the compiled binding path
///   and use the fallback string-based binding instead, just as is done for
///   Source={RelativeSource ...} (PR #33248 / #33247).
/// </summary>
public partial class Maui33291 : ContentPage
{
	public ObservableCollection<Maui33291Item> Items { get; } = new()
	{
		new Maui33291Item { Name = "Item A" },
		new Maui33291Item { Name = "Item B" },
	};

	public ICommand ItemTappedCommand { get; } = new Command<Maui33291Item>(_ => { });

	public Maui33291()
	{
		InitializeComponent();
		BindingContext = this;
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void TapGestureCommandBindsToPageCommandViaXReference(XamlInflator inflator)
		{
			var page = new Maui33291(inflator);
			// The inflator constructor bypasses the user-defined constructor which sets
			// BindingContext = this, so we set it explicitly here to simulate the real
			// app scenario where BindingContext is assigned to the page's view-model.
			page.BindingContext = page;

			// Create a DataTemplate item — this is what CollectionView does at scroll time.
			var itemLayout = page.TheCollectionView.ItemTemplate.CreateContent() as VerticalStackLayout;
			Assert.NotNull(itemLayout);

			var tapGesture = itemLayout.GestureRecognizers[0] as TapGestureRecognizer;
			Assert.NotNull(tapGesture);

			// Command must NOT be null — this is the key assertion.
			// When the bug is present, the SourceGen compiles the binding against the item
			// model type (Maui33291Item), fails to find BindingContext.ItemTappedCommand on
			// it, and the Command ends up null.
			Assert.NotNull(tapGesture.Command);

			// The resolved Command must be the same instance exposed by the page.
			Assert.Same(page.ItemTappedCommand, tapGesture.Command);
		}
	}
}

public class Maui33291Item
{
	public string Name { get; set; } = string.Empty;
}
