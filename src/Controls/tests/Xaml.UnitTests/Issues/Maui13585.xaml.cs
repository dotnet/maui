using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13585 : ContentPage
{
	public Maui13585() => InitializeComponent();

	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void TriggerWithDynamicResource(XamlInflator inflator)
		{
			var page = new Maui13585(inflator);
			Assert.Equal(Colors.Green, page.styleTriggerWithStaticResources.BackgroundColor);
			Assert.Equal(Colors.Green, page.styleTriggerWithDynamicResources.BackgroundColor);

			page.styleTriggerWithStaticResources.IsEnabled = false;
			page.styleTriggerWithDynamicResources.IsEnabled = false;

			Assert.Equal(Colors.Purple, page.styleTriggerWithStaticResources.BackgroundColor);
			Assert.Equal(Colors.Purple, page.styleTriggerWithDynamicResources.BackgroundColor);

			page.styleTriggerWithStaticResources.IsEnabled = true;
			page.styleTriggerWithDynamicResources.IsEnabled = true;

			Assert.Equal(Colors.Green, page.styleTriggerWithStaticResources.BackgroundColor);
			Assert.Equal(Colors.Green, page.styleTriggerWithDynamicResources.BackgroundColor);
		}
	}
}