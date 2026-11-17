using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3059VisualStates : ContentPage
{
	public Maui3059VisualStates()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void VisualStateGroupList_MultipleChildren_NoWarning(XamlInflator inflator)
		{
			var page = new Maui3059VisualStates(inflator);
			Assert.NotNull(page.Content);
			Assert.IsType<Grid>(page.Content);
		}
	}
}
