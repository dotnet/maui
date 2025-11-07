using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20818
{
	public Maui20818() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void TypeLiteralAndXTypeCanBeUsedInterchangeably(XamlInflator inflator)
		{
			var page = new Maui20818(inflator);

			Assert.Equal(typeof(Label), (page.Resources["A"] as Style).TargetType);
			Assert.Equal(typeof(Label), (page.Resources["B"] as Style).TargetType);

			Assert.Equal(typeof(Label), page.TriggerC.TargetType);
			Assert.Equal(typeof(Label), page.TriggerD.TargetType);
			Assert.Equal(typeof(Label), page.TriggerE.TargetType);
			Assert.Equal(typeof(Label), page.TriggerF.TargetType);
			Assert.Equal(typeof(Label), page.TriggerG.TargetType);
			Assert.Equal(typeof(Label), page.TriggerH.TargetType);
		}
	}
}
