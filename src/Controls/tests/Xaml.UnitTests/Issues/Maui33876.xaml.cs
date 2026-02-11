using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui33876ViewModel
{
    public string Command { get; set; } = "Test";
}

public partial class Maui33876 : ContentPage
{
    public Maui33876() => InitializeComponent();

    [Collection("Issue")]
    public class Tests
    {
        [Theory]
        [XamlInflatorData]
        internal void RelativeSourceInDataTemplate(XamlInflator inflator)
        {
            var page = new Maui33876(inflator);
            Assert.NotNull(page);
        }
    }
}
