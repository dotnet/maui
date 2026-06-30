using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Page-level ViewModel — this is what the RelativeSource AncestorType points to
public class Maui34056PageViewModel
{
    public ObservableCollection<Maui34056ItemViewModel> Items { get; } =
        [new Maui34056ItemViewModel { ItemName = "Item 1" }];

    public ICommand TestCommand { get; } = new Command(() => { });
}

// Item ViewModel — this is what the DataTemplate's x:DataType is set to
public class Maui34056ItemViewModel
{
    public string ItemName { get; set; } = "";
}

public partial class Maui34056 : ContentPage
{
    public Maui34056()
    {
        InitializeComponent();
        BindingContext = new Maui34056PageViewModel();
    }

    [Collection("Issue")]
    public class Maui34056Tests
    {
        [Theory]
        [XamlInflatorData]
        internal void RelativeSourceAncestorTypeInDataTemplateGeneratesCompiledBinding(XamlInflator inflator)
        {
            var page = new Maui34056(inflator);

            var template = ((CollectionView)page.TestCollectionView).ItemTemplate;
            var content = template.CreateContent() as Button;
            Assert.NotNull(content);

            var bindingContext = content.GetContext(Button.CommandProperty);
            Assert.NotNull(bindingContext);
            var binding = bindingContext.Bindings.GetValue();

            if (inflator is XamlInflator.Runtime)
            {
                // Runtime inflator uses the string-based Binding — no compile-time type info available.
                Assert.IsType<Binding>(binding);
            }
            else
            {
                // SourceGen: produces a trim-safe TypedBinding using AncestorType as the source type (the PR fix).
                // XamlC: produces a TypedBinding here via the inline x:DataType='local:Maui34056PageViewModel'
                // attribute on the Binding markup — not via AncestorType resolution. XamlC's AncestorType-only
                // behavior (Scenario 3, no inline x:DataType) still falls back to runtime Binding.
                var typedBinding = Assert.IsType<TypedBinding<Maui34056PageViewModel, ICommand>>(binding);

                // Verify the RelativeSource is correctly configured (mode + ancestor type).
                var relativeSource = Assert.IsType<RelativeBindingSource>(typedBinding.Source);
                Assert.Equal(RelativeBindingSourceMode.FindAncestorBindingContext, relativeSource.Mode);
                Assert.Equal(typeof(Maui34056PageViewModel), relativeSource.AncestorType);
            }
        }

        [Theory]
        [XamlInflatorData]
        internal void RelativeSourceAncestorTypeWithoutInlineXDataTypeGeneratesCompiledBinding(XamlInflator inflator)
        {
            // Regression guard for SourceGen: the ambient DataTemplate x:DataType is
            // Maui34056ItemViewModel (no TestCommand). If SourceGen regressed and used ambient
            // x:DataType instead of AncestorType as the source, the TypedBinding assertion would
            // fail, catching the regression.
            // XamlC pre-existing behavior: does not compile AncestorType bindings without an
            // explicit inline x:DataType — falls back to runtime Binding. This is separate from
            // the SourceGen fix and not addressed by this PR.
            var page = new Maui34056(inflator);

            var template = ((CollectionView)page.AncestorTypeNoInlineDataTypeCollectionView).ItemTemplate;
            var content = template.CreateContent() as Button;
            Assert.NotNull(content);

            var bindingContext = content.GetContext(Button.CommandProperty);
            Assert.NotNull(bindingContext);
            var binding = bindingContext.Bindings.GetValue();

            if (inflator is XamlInflator.Runtime or XamlInflator.XamlC)
            {
                // Runtime: no compile-time type info.
                // XamlC: pre-existing behavior — does not compile AncestorType without inline x:DataType.
                Assert.IsType<Binding>(binding);
            }
            else
            {
                // SourceGen: compiles to TypedBinding using AncestorType as the source.
                var typedBinding = Assert.IsType<TypedBinding<Maui34056PageViewModel, ICommand>>(binding);

                var relativeSource = Assert.IsType<RelativeBindingSource>(typedBinding.Source);
                Assert.Equal(RelativeBindingSourceMode.FindAncestorBindingContext, relativeSource.Mode);
                Assert.Equal(typeof(Maui34056PageViewModel), relativeSource.AncestorType);
            }
        }

        [Theory]
        [XamlInflatorData]
        internal void RelativeSourceSelfInDataTemplateWithXDataTypeUsesStringBinding(XamlInflator inflator)
        {
            // Verifies SourceGen does not use the DataTemplate's x:DataType as the source type for
            // {RelativeSource Self} bindings. The source is the element itself, resolved at runtime.
            // Path=ItemName exists on Maui34056ItemViewModel to ensure the guard is what prevents
            // compiled binding, not a failed type lookup. XamlC behavior is pre-existing and separate.
            var page = new Maui34056(inflator);

            var template = ((CollectionView)page.SelfBindingCollectionView).ItemTemplate;
            var content = template.CreateContent() as Label;
            Assert.NotNull(content);

            var bindingContext = content.GetContext(Label.TextProperty);
            Assert.NotNull(bindingContext);
            var binding = bindingContext.Bindings.GetValue();

            if (inflator is XamlInflator.XamlC)
            {
                // XamlC pre-existing behavior: compiles RelativeSource Self using DataTemplate x:DataType.
                // This is a separate issue, not addressed by this fix.
                Assert.IsType<TypedBinding<Maui34056ItemViewModel, string>>(binding);
            }
            else
            {
                // Runtime: no compile-time type info, always string-based Binding.
                // SourceGen (the fix): HasRelativeSourceBinding blocks x:DataType path for Self bindings.
                Assert.IsType<Binding>(binding);
            }
        }

        [Theory]
        [XamlInflatorData]
        internal void RelativeSourceElementAncestorTypeUsesFindAncestorMode(XamlInflator inflator)
        {
            // Verifies that when AncestorType is an Element subclass (ContentPage), SourceGen selects
            // RelativeBindingSourceMode.FindAncestor (not FindAncestorBindingContext) and produces a
            // TypedBinding<ContentPage, string>. This exercises the HasImplicitConversion/FindAncestor
            // branch in KnownMarkups.cs that the other scenarios do not cover.
            var page = new Maui34056(inflator);
            page.Title = "TestTitle";

            var template = ((CollectionView)page.FindAncestorCollectionView).ItemTemplate;
            var content = template.CreateContent() as Label;
            Assert.NotNull(content);

            var bindingContext = content.GetContext(Label.TextProperty);
            Assert.NotNull(bindingContext);
            var binding = bindingContext.Bindings.GetValue();

            if (inflator is XamlInflator.Runtime or XamlInflator.XamlC)
            {
                // Runtime: no compile-time type info.
                // XamlC: pre-existing behavior — does not compile AncestorType without inline x:DataType.
                Assert.IsType<Binding>(binding);
            }
            else
            {
                // SourceGen: AncestorType=ContentPage (an Element subclass) → FindAncestor mode.
                var typedBinding = Assert.IsType<TypedBinding<ContentPage, string>>(binding);
                var relativeSource = Assert.IsType<RelativeBindingSource>(typedBinding.Source);
                Assert.Equal(RelativeBindingSourceMode.FindAncestor, relativeSource.Mode);
                Assert.Equal(typeof(ContentPage), relativeSource.AncestorType);
            }
        }
    }
}
