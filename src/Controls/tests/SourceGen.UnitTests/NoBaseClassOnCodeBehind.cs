using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class NoBaseClassOnCodeBehind : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	//as the base class ContentPage is only defined in the sg.cs, 
	//it's not part of the compilation when we generate initializecomponent, and the Resources property can't be found
	public void CodeBehindWithNoBaseClass()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
    <ContentPage.Resources>    
        <Color x:Key="MyColor">Red</Color>
    </ContentPage.Resources>
    <Button x:Name="MyButton" Text="Hello MAUI!" />
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		Assert.False(result.Diagnostics.Any());
	}
}