using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui7744 : ContentPage
{
	public Maui7744() => InitializeComponent();

	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void ConvertersAreExecutedWhileApplyingSetter(XamlInflator inflator)
		{
			var page = new Maui7744(inflator);
			Assert.IsType<RoundRectangle>(page.border0.StrokeShape);
			Assert.IsType<RoundRectangle>(page.border1.StrokeShape);
		}
	}
}