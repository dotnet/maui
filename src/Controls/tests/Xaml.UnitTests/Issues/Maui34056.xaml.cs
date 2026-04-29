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
                // Both XamlC and SourceGen produce a trim-safe TypedBinding when x:DataType is present
                // on the binding node alongside RelativeSource AncestorType.
                Assert.IsType<TypedBinding<Maui34056PageViewModel, ICommand>>(binding);
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
    }
}
