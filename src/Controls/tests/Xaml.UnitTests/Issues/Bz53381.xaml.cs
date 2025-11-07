using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz53381 : ContentView
{
	public Bz53381()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}

		public void ControlTemplateAsImplicitAppLevelStyles(XamlInflator inflator)
		{
			Application.Current = new Bz53381App();
			var view = new Bz53381(inflator);
			Application.Current.LoadPage(new ContentPage { Content = view });
			var presenter = ((StackLayout)((IVisualTreeElement)view).GetVisualChildren()[0]).Children[1] as ContentPresenter;
			// TODO: Convert Assume to Skip or Assert`r`nAssert.True(true);
			var grid = presenter.Content as Grid;
			Assert.NotNull(grid);
			Assert.Equal(Colors.Green, grid.BackgroundColor);
		}
	}
}
