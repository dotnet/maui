using System;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public enum Maui34021MyEnum { Value1, Value2, Value3 }

internal static class Maui34021MyEnumExtension
{
    public static string ToFriendlyString(this Maui34021MyEnum value) => value.ToString();
}

public class Maui34021DataObject<T> : View where T : Enum
{
}

public partial class Maui34021 : ContentPage
{
    public Maui34021() => InitializeComponent();

    [Collection("Issue")]
    public class Tests : IDisposable
    {
        public void Dispose() => Application.Current = null;

        [Theory]
        [InlineData(XamlInflator.Runtime)]
        [InlineData(XamlInflator.XamlC)]
        [InlineData(XamlInflator.SourceGen)]
        internal void SourceGenResolvesEnumTypeNotExtensionClass(XamlInflator inflator)
        {
            if (inflator == XamlInflator.SourceGen)
            {
                const string xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests"
             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui34021SourceGenRepro">
    <local:Maui34021DataObject x:TypeArguments="local:Maui34021MyEnum" />
</ContentPage>
""";

                const string csharp = """
using System;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public enum Maui34021MyEnum { Value1, Value2, Value3 }

internal static class Maui34021MyEnumExtension
{
    public static string ToFriendlyString(this Maui34021MyEnum value) => value.ToString();
}

public class Maui34021DataObject<T> : View where T : Enum
{
}

public partial class Maui34021SourceGenRepro : ContentPage
{
    public Maui34021SourceGenRepro() => InitializeComponent();
}
""";

                var result = CreateMauiCompilation()
                .WithAdditionalSource(csharp)
                .RunMauiSourceGenerator(new AdditionalXamlFile("Issues/Maui34021SourceGenRepro.xaml", xaml, TargetFramework: "net10.0"));
                var generated = result.GeneratedInitializeComponent();
                Assert.DoesNotContain("Maui34021MyEnumExtension", generated, StringComparison.Ordinal);
                Assert.Contains("Maui34021MyEnum", generated, StringComparison.Ordinal);
            }
            else
            {
                var page = new Maui34021(inflator);
                Assert.IsType<Maui34021DataObject<Maui34021MyEnum>>(page.Content);
            }
        }
    }
}
