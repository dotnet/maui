using System.Windows.Input;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Test for regression: RelativeSource AncestorType with Path=. returns null in v10.0.20
/// 
/// Scenario: A TapGestureRecognizer tries to get a reference to the parent ContentPage via
/// CommandParameter="{Binding Path=., Source={RelativeSource AncestorType={x:Type local:Maui33247}}}"
/// 
/// Expected: The CommandParameter should receive the ContentPage reference.
/// Bug: In v10.0.20, the CommandParameter was null when using SourceGen because the source generator
/// was incorrectly compiling the binding to use x:DataType as the source type instead of allowing
/// the RelativeSource to resolve the source at runtime.
/// 
/// Fix: When a binding has a Source property with a RelativeSource, skip the compiled binding path
/// and use the fallback string-based binding instead.
/// </summary>
public partial class Maui33247 : ContentPage
{
	public Maui33247()
	{
		InitializeComponent();
		BindingContext = this;
	}

	public ICommand Navigate => new Command((param) => { });

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void DirectRelativeSourceAncestorTypeWithSelfPathReturnsAncestor(XamlInflator inflator)
		{
			// Test that RelativeSource AncestorType with Path=. returns the ancestor object for direct children
			var page = new Maui33247(inflator);
			
			// The CommandParameter with RelativeSource AncestorType and Path=. should be the ContentPage itself
			Assert.NotNull(page.DirectTapGesture.CommandParameter);
			Assert.IsType<Maui33247>(page.DirectTapGesture.CommandParameter);
			Assert.Same(page, page.DirectTapGesture.CommandParameter);
		}
	}
}
