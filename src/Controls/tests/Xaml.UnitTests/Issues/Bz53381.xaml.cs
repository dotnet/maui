using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz53381 : ContentView
	{
		public Bz53381()
		{
			InitializeComponent();
		}

		public Bz53381(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub()); public void TearDown()
			{
				Application.Current = null;
				DispatcherProvider.SetCurrent(null);
			}

			[Theory]
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void ControlTemplateAsImplicitAppLevelStyles(bool useCompiledXaml)
			{
				Application.Current = new Bz53381App();
				var view = new Bz53381(useCompiledXaml);
				Application.Current.LoadPage(new ContentPage { Content = view });
				var presenter = ((StackLayout)view.InternalChildren[0]).Children[1] as ContentPresenter;
				Assume.That(presenter, Is.Not.Null);
				var grid = presenter.Content as Grid;
				Assert.NotNull(grid);
				Assert.Equal(Colors.Green, grid.BackgroundColor);
			}
		}
	}
}