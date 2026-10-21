using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui37551 : ContentPage
{
    public Maui37551() => InitializeComponent();

    [Collection("Issue")]
    public class Tests
    {
        [Theory]
        [XamlInflatorData]
        internal void XReferenceInMultipleDataTemplateResourcesResolvesOuterNamescope(XamlInflator inflator)
        {
            if (inflator == XamlInflator.SourceGen)
            {
                var result = MockSourceGenerator.RunMauiSourceGenerator(typeof(Maui37551));
                Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "MAUIG1001");
            }

            var page = new Maui37551(inflator);
            var firstTemplate = Assert.IsType<Microsoft.Maui.Controls.DataTemplate>(page.Resources["FirstDataTemplate"]);
            var secondTemplate = Assert.IsType<Microsoft.Maui.Controls.DataTemplate>(page.Resources["SecondDataTemplate"]);
            var firstLabel = Assert.IsType<Label>(firstTemplate.CreateContent());
            var secondLabel = Assert.IsType<Label>(secondTemplate.CreateContent());

            Assert.Same(page, firstLabel.BindingContext);
            Assert.Same(page, secondLabel.BindingContext);
        }
    }
}
