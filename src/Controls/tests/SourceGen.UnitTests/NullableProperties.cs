using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class NullableProperties : SourceGenXamlInitializeComponentTestBase
{

	[Fact]
	public void NullableStringPropertyShouldWork()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
		<ContentPage.Behaviors>
			<local:EventToCommandBehavior
					EventName="NavigatedTo" />
		</ContentPage.Behaviors>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class EventToCommandBehavior : Behavior
{
	public static readonly BindableProperty EventNameProperty;

    // Key here is that this is a nullable string property, this was causing issues
	public string? EventName { get; set; }
}
""";

		var (result, generated) = RunGenerator(xaml, code);
		Assert.False(result.Diagnostics.Any());

		Assert.True(generated?.Contains("Test.EventToCommandBehavior", StringComparison.Ordinal) ?? false);
		Assert.True(generated?.Contains("eventToCommandBehavior.SetValue(global::Test.EventToCommandBehavior.EventNameProperty", StringComparison.Ordinal) ?? false);
	}
}