using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34056StringAncestorType : ContentPage
{
    public Maui34056StringAncestorType()
    {
        InitializeComponent();
        BindingContext = new Maui34056PageViewModel();
    }

    [Collection("Issue")]
    public class Tests
    {
        [Fact]
        internal void RelativeSourceAncestorTypeAsStringGeneratesCompiledBinding()
        {
            // Concern 2 from review #4492884861:
            // TryGetRelativeSourceAncestorType only handled the {x:Type ...} ElementNode form.
            // The bare string form AncestorType="local:MyViewModel" (a ValueNode) was not handled
            // and silently fell back to a runtime string Binding.
            // After the fix it should produce a trim-safe TypedBinding<Maui34056PageViewModel, ICommand>.
            var page = new Maui34056StringAncestorType();

            var template = page.StringAncestorTypeCollectionView.ItemTemplate;
            var content = template.CreateContent() as Button;
            Assert.NotNull(content);

            var bindingContext = content.GetContext(Button.CommandProperty);
            Assert.NotNull(bindingContext);
            var binding = bindingContext.Bindings.GetValue();

            // SourceGen should produce a TypedBinding for the string AncestorType form.
            var typedBinding = Assert.IsType<TypedBinding<Maui34056PageViewModel, ICommand>>(binding);

            // Also verify the RelativeSource is correctly configured.
            var relativeSource = Assert.IsType<RelativeBindingSource>(typedBinding.Source);
            Assert.Equal(RelativeBindingSourceMode.FindAncestorBindingContext, relativeSource.Mode);
            Assert.Equal(typeof(Maui34056PageViewModel), relativeSource.AncestorType);
        }
    }
}
