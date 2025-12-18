using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21757
{
	public Maui21757() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void TypeLiteralAndXTypeCanBeUsedInterchangeably(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui21757));
			}
			var resourceDictionary = new Maui21757(inflator);

			var styleA = resourceDictionary["A"] as Style;
			Assert.NotNull(styleA);
			Assert.Equal(typeof(BoxView), styleA.TargetType);
			Assert.Equal(BoxView.ColorProperty, styleA.Setters[0].Property);
			Assert.Equal(Color.FromArgb("#C8C8C8"), styleA.Setters[0].Value);

			var styleB = resourceDictionary["B"] as Style;
			Assert.NotNull(styleB);
			Assert.Equal(typeof(BoxView), styleB.TargetType);
			Assert.Equal(BoxView.ColorProperty, styleB.Setters[0].Property);
			Assert.Equal(Color.FromArgb("#C8C8C8"), styleB.Setters[0].Value);
		}
	}
}
