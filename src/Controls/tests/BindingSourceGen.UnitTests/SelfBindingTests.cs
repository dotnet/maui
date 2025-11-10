using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;
using System.Linq;

namespace BindingSourceGen.UnitTests;

public class SelfBindingTests
{
    [Fact]
    public void GenerateSelfBinding()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.TextProperty, static (string s) => s);
        """;

        var result = SourceGenHelpers.Run(source);
        
        // Should have no diagnostics
        AssertExtensions.AssertNoDiagnostics(result);
        
        // Should have generated a binding
        Assert.NotNull(result.Binding);
        
        // Check all generated files
        foreach (var file in result.GeneratedFiles)
        {
            Console.WriteLine($"\n=== File: {file.Key} ===");
            Console.WriteLine(file.Value);
        }
    }
}
