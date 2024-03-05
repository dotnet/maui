using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

public class DiagnosticsTests
{
    // [Fact]
    // public void ReportsWarningWhenContainingClassIsNotPartial()
    // {
    //     var source = """
    //         using Microsoft.Maui.Controls;

    //         public class ContainingClass
    //         {
    //             public void Method()
    //             {
    //                 var button = new Button();
    //                 button.SetBinding(Button.TextProperty, static (string x) => x);
    //             }
    //         }
    //         """;

    //     var result = SourceGenHelpers.Run(source);

    //     Assert.Single(result.Diagnostics);
    //     Assert.Equal("MAUIG2001", result.Diagnostics[0].Id);
    // }
}
