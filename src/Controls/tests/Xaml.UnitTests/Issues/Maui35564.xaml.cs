using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Regression test for https://github.com/dotnet/maui/issues/35564
///
/// Scenario A (Runtime inflator):
///   A TapGestureRecognizer inside a CollectionView ItemTemplate binds its Command
///   to the *page* using Source={RelativeSource AncestorType=...}, while the
///   DataTemplate has x:DataType="local:Maui35564Item" (the item model type).
///   When IsXamlCBindingWithSourceCompilationEnabled is true (AOT), BindingExtension
///   must NOT propagate the DataTemplate's x:DataType to a RelativeSource binding.
///
/// Scenario B (SourceGen inflator):
///   The binding has x:DataType=local:Maui35564 directly on it, alongside
///   Source={RelativeSource AncestorType=...}.  SourceGen must compile this to a
///   TypedBinding (no reflection) so the binding survives AOT/linker trimming.
///
/// Scenario C (Regression guard — RelativeSource Self with inherited x:DataType):
///   A {RelativeSource Self} binding inside a DataTemplate that has x:DataType=
///   "local:Maui35564Item". The inherited item type must NOT be applied to Self
///   bindings — before the fix, Maui35564Item.IsAssignableFrom(Label) = false
///   would null out the source; after the fix DataType = null for inherited-type
///   RelativeSource bindings, so Self resolves to the Label correctly.
/// </summary>
public partial class Maui35564 : ContentPage
{
	public ObservableCollection<Maui35564Item> Items { get; } = new()
	{
		new Maui35564Item { Name = "Item A" },
		new Maui35564Item { Name = "Item B" },
	};

	public ICommand ItemTappedCommand { get; } = new Command<Maui35564Item>(_ => { });

	public Maui35564()
	{
		InitializeComponent();
		BindingContext = this;
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		const string FeatureSwitch =
			"Microsoft.Maui.RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled";

		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		/// <summary>
		/// Scenario A: RelativeSource binding without x:DataType directly on the binding node.
		/// The DataTemplate's inherited x:DataType (Maui35564Item) must NOT be used to validate
		/// the RelativeSource binding's resolved ancestor (the Maui35564 page).
		/// For SourceGen, AncestorType should be resolved as the canonical source type and
		/// compiled to TypedBinding even without x:DataType on the binding node.
		/// </summary>
		[Theory]
		[XamlInflatorData]
		internal void RelativeSourceCommandBindsToAncestorWithXamlCCompilationEnabled(XamlInflator inflator)
		{
			AppContext.SetSwitch(FeatureSwitch, true);
			try
			{
				var page = new Maui35564(inflator);
				page.BindingContext = page;

				var itemLayout = page.TheCollectionView.ItemTemplate.CreateContent() as VerticalStackLayout;
				Assert.NotNull(itemLayout);

				var container = new VerticalStackLayout();
				container.Add(itemLayout);
				page.Content = container;

				itemLayout.BindingContext = new Maui35564Item { Name = "Test" };

				var tapGesture = itemLayout.GestureRecognizers[0] as TapGestureRecognizer;
				Assert.NotNull(tapGesture);

				Assert.NotNull(tapGesture.Command);
				Assert.Same(page.ItemTappedCommand, tapGesture.Command);

				if (inflator == XamlInflator.SourceGen)
				{
					var binding = tapGesture.GetContext(TapGestureRecognizer.CommandProperty).Bindings.GetValue();
					Assert.IsAssignableFrom<TypedBindingBase>(binding);
				}
			}
			finally
			{
				AppContext.SetSwitch(FeatureSwitch, false);
			}
		}

		/// <summary>
		/// Scenario B: RelativeSource binding WITH x:DataType directly on the binding node.
		/// This is the real-world pattern users write in AOT apps.  SourceGen must compile it
		/// to a TypedBinding (no reflection) so the binding survives linker trimming.
		/// For all inflators, the Command must resolve correctly.
		/// For the SourceGen inflator specifically, the binding must be a TypedBinding.
		/// </summary>
		[Theory]
		[XamlInflatorData]
		internal void RelativeSourceCommandWithExplicitXDataTypeCompilesTypedBinding(XamlInflator inflator)
		{
			AppContext.SetSwitch(FeatureSwitch, true);
			try
			{
				var page = new Maui35564(inflator);
				page.BindingContext = page;

				var itemLayout = page.TheCollectionView2.ItemTemplate.CreateContent() as VerticalStackLayout;
				Assert.NotNull(itemLayout);

				var container = new VerticalStackLayout();
				container.Add(itemLayout);
				page.Content = container;

				itemLayout.BindingContext = new Maui35564Item { Name = "Test" };

				var tapGesture = itemLayout.GestureRecognizers[0] as TapGestureRecognizer;
				Assert.NotNull(tapGesture);

				// Command must resolve to the page's command for ALL inflators.
				Assert.NotNull(tapGesture.Command);
				Assert.Same(page.ItemTappedCommand, tapGesture.Command);

				// For the SourceGen inflator, the binding must be a TypedBinding — not a reflective
				// Binding — so it survives AOT linker trimming.
				if (inflator == XamlInflator.SourceGen)
				{
					var binding = tapGesture.GetContext(TapGestureRecognizer.CommandProperty).Bindings.GetValue();
					Assert.IsAssignableFrom<TypedBindingBase>(binding);
				}
			}
			finally
			{
				AppContext.SetSwitch(FeatureSwitch, false);
			}
		}
		/// <summary>
		/// Scenario C: {RelativeSource Self} inside a DataTemplate that has x:DataType="Maui35564Item".
		/// The inherited item type (Maui35564Item) must NOT be applied to the Self binding —
		/// Self resolves to the Label itself, not to the DataTemplate item.
		/// Before the fix: inherited DataType caused IsAssignableFrom(Label)=false → source=null.
		/// After the fix: DataType=null for inherited-type RelativeSource bindings → Self resolves.
		/// </summary>
		[Theory]
		[XamlInflatorData]
		internal void RelativeSourceSelfInsideDataTemplateWithInheritedXDataType(XamlInflator inflator)
		{
			AppContext.SetSwitch(FeatureSwitch, true);
			try
			{
				var page = new Maui35564(inflator);
				page.BindingContext = page;

				var itemLayout = page.TheCollectionView3.ItemTemplate.CreateContent() as VerticalStackLayout;
				Assert.NotNull(itemLayout);

				itemLayout.BindingContext = new Maui35564Item { Name = "Test" };

				var label = itemLayout.Children[0] as Label;
				Assert.NotNull(label);

				// Label.Text must equal the Label's own AutomationId ("scenario-c"), bound via {RelativeSource Self}.
				// If the inherited x:DataType were applied, the Self source would be nulled out
				// and Text would be null or empty.
				Assert.Equal("scenario-c", label.Text);
			}
			finally
			{
				AppContext.SetSwitch(FeatureSwitch, false);
			}
		}

		/// <summary>
		/// Scenario D: {RelativeSource Self} with x:DataType directly on the binding node.
		/// Self bindings should not be compiled to TypedBinding, even with explicit x:DataType,
		/// because the source is the view element itself.
		/// </summary>
		[Theory]
		[XamlInflatorData]
		internal void RelativeSourceSelfWithExplicitXDataTypeStaysUncompiled(XamlInflator inflator)
		{
			AppContext.SetSwitch(FeatureSwitch, true);
			try
			{
				var page = new Maui35564(inflator);
				page.BindingContext = page;

				var itemLayout = page.TheCollectionView4.ItemTemplate.CreateContent() as VerticalStackLayout;
				Assert.NotNull(itemLayout);

				itemLayout.BindingContext = new Maui35564Item { Name = "Test" };

				var label = itemLayout.Children[0] as Label;
				Assert.NotNull(label);
				Assert.Equal("scenario-d", label.Text);

				if (inflator == XamlInflator.SourceGen)
				{
					var binding = label.GetContext(Label.TextProperty).Bindings.GetValue();
					Assert.IsNotAssignableFrom<TypedBindingBase>(binding);
				}
			}
			finally
			{
				AppContext.SetSwitch(FeatureSwitch, false);
			}
		}
	}
}

public class Maui35564Item
{
	public string Name { get; set; } = string.Empty;
}
