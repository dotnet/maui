using System;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33877 : ContentPage
{
    public Maui33877() => InitializeComponent();

    // Note: OnMissingHandler is intentionally NOT defined to test the diagnostic

    [Collection("Issue")]
    public class Tests
    {
        [Fact]
        public void MissingEventHandlerProducesDiagnostic()
        {
            var result = CreateMauiCompilation()
                .RunMauiSourceGenerator(typeof(Maui33877));

            Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2014");
        }

        [Fact]
        public void MissingEventHandlerMessageIncludesHandlerName()
        {
            var result = CreateMauiCompilation()
                .RunMauiSourceGenerator(typeof(Maui33877));

            var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2014");
            Assert.Contains("OnMissingHandler", diagnostic.GetMessage(), StringComparison.Ordinal);
        }

        [Fact]
        public void MissingEventHandlerMessageIncludesTypeName()
        {
            var result = CreateMauiCompilation()
                .RunMauiSourceGenerator(typeof(Maui33877));

            var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2014");
            Assert.Contains("Maui33877", diagnostic.GetMessage(), StringComparison.Ordinal);
        }
    }
}
