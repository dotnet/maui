using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class CircularReferenceExtension : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public async Task CircularReferenceWithCustomViewExpandableContent()
	{
		// Test based on https://github.com/dotnet/maui/issues/32623
		//
		// This test reproduces the infinite loop that occurs when x:Reference is used
		// inside nested DataTemplates within a custom control's property, creating a
		// context chain where the buggy code (line 524: context.ParentContext) would
		// repeatedly assign the same parent context instead of walking up the chain.

		var expanderViewXaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentView
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Test.ExpanderView">
</ContentView>
""";

		var expanderViewCode =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class ExpanderView : ContentView
{
    public static readonly BindableProperty ExpandableContentProperty = BindableProperty.Create(
        nameof(ExpandableContent),
        typeof(View),
        typeof(ExpanderView));

    public ExpanderView()
    {
        InitializeComponent();
    }

    public View ExpandableContent
    {
        get => (View)GetValue(ExpandableContentProperty);
        set => SetValue(ExpandableContentProperty, value);
    }
}
""";

		var bugPageXaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
    x:Class="Test.BugPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:bug="using:Test"
    x:Name="Page"
    x:DataType="bug:BugPage">
    <ContentPage.Resources>
        <DataTemplate x:Key="MultiLocationControlCollection">
            <bug:ExpanderView>
                <bug:ExpanderView.ExpandableContent>
                    <Grid>
                        <BindableLayout.ItemTemplate>
                            <DataTemplate>
                                <Grid>
                                    <Grid.GestureRecognizers>
                                        <TapGestureRecognizer Command="{Binding Source={x:Reference Page}, Path=ToggleRow, x:DataType=bug:BugPage}" CommandParameter="{Binding .}" />
                                    </Grid.GestureRecognizers>
                                </Grid>
                            </DataTemplate>
                        </BindableLayout.ItemTemplate>
                    </Grid>
                </bug:ExpanderView.ExpandableContent>
            </bug:ExpanderView>
        </DataTemplate>
    </ContentPage.Resources>
</ContentPage>
""";

		var bugPageCode =
"""
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
partial class BugPage : ContentPage
{
	public BugPage()
	{
		InitializeComponent();
	}

	public ICommand? ToggleRow { get; set; }
}
""";

		// Use the multi-file overload to process both XAML files in a single compilation
		// This reproduces the context chain that triggers the infinite loop
		var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(10));
		
		var sw = Stopwatch.StartNew();
		
		try
		{
			var task = Task.Run(() =>
			{
				// Create a single compilation with both C# files
				var compilation = CreateMauiCompilation();
				compilation = compilation.AddSyntaxTrees(
					Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(expanderViewCode),
					Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(bugPageCode)
				);
				
				var workingDirectory = Environment.CurrentDirectory;
				
				// Create AdditionalFiles for both XAML files
				var expanderFile = new AdditionalXamlFile(
					Path.Combine(workingDirectory, "ExpanderView.xaml"),
					expanderViewXaml,
					RelativePath: "ExpanderView.xaml",
					TargetFramework: "",
					NoWarn: "",
					ManifestResourceName: $"{compilation.AssemblyName}.ExpanderView.xaml"
				);
				
				var bugFile = new AdditionalXamlFile(
					Path.Combine(workingDirectory, "BugPage.xaml"),
					bugPageXaml,
					RelativePath: "BugPage.xaml",
					TargetFramework: "",
					NoWarn: "",
					ManifestResourceName: $"{compilation.AssemblyName}.BugPage.xaml"
				);
				
				// Run generator with BOTH files at once - this creates the shared context
				// that triggers the infinite loop with the buggy code
				var result = RunGenerator<XamlGenerator>(compilation, expanderFile, bugFile);
				
				return result;
			}, cts.Token);
			
			var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10), cts.Token));
			
			if (completed != task)
			{
				Assert.Fail("Generator did not complete within 10 seconds - infinite loop detected!");
			}
			
			var result = await task;
			sw.Stop();
			
			// With the fix, this should complete quickly
			Assert.True(sw.Elapsed < TimeSpan.FromSeconds(5), 
				$"Generator took {sw.Elapsed.TotalSeconds:F2} seconds, expected < 5 seconds");
		}
		catch (OperationCanceledException)
		{
			Assert.Fail("Generator timed out - infinite loop detected!");
		}
	}
}
