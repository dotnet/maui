using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ButtonCornerRadius : ContentPage
{
	public ButtonCornerRadius() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => Application.Current = new MockApplication();

		[Theory]
		[Values]
		public void EscapedStringsAreTreatedAsLiterals(XamlInflator inflator)
		{
			var layout = new ButtonCornerRadius(inflator);
			Assert.Equal(0, layout.Button0.CornerRadius);
		}
	}
}