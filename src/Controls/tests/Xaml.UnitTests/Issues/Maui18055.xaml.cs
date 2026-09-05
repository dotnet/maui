using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Regression test for https://github.com/dotnet/maui/issues/18055
// XamlC must use Dup (not Ldarg_0) before ldvirtftn when wiring virtual event handlers
// inside a DataTemplate, so the correct vtable object is used for virtual dispatch.
public partial class Maui18055 : ContentPage
{
	public Maui18055() => InitializeComponent();

	public int BaseForVirtualClicked;
	protected virtual void HandleVirtualClicked(object sender, EventArgs e) => BaseForVirtualClicked++;

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		// Verifies that a virtual handler wired inside a DataTemplate dispatches to the override,
		// not the base class. Without the fix, XamlC emitted Ldarg_0 (the anonymous DataTemplate
		// class) as the ldvirtftn vtable source — causing wrong dispatch on JIT and a hard crash
		// on iOS/macOS Full AOT.
		internal void VirtualHandlerInDataTemplateCallsOverride(XamlInflator inflator)
		{
			var page = new SubMaui18055(inflator);
			Assert.Equal(0, page.BaseForVirtualClicked);
			Assert.Equal(0, page.OverrideClicked);

			var template = (Microsoft.Maui.Controls.DataTemplate)page.Resources["virtualHandlerTemplate"];
			var element = (ElementWithEvent)template.CreateContent();
			element.SendClicked();

			// Override must be called; base must NOT be called.
			Assert.Equal(1, page.OverrideClicked);
			Assert.Equal(0, page.BaseForVirtualClicked);
		}
	}
}

class SubMaui18055 : Maui18055
{
	public SubMaui18055(XamlInflator inflator) : base(inflator) { }

	public int OverrideClicked;
	protected override void HandleVirtualClicked(object sender, EventArgs e) => OverrideClicked++;
}
