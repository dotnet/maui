using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			[TearDown]
			public void TearDown()
			{
				Application.Current = null;
				DispatcherProvider.SetCurrent(null);
			}

			[TestCase(true)]
			[TestCase(false)]
			public void ControlTemplateAsImplicitAppLevelStyles(bool useCompiledXaml)
			{
				Application.Current = new Bz53381App();
				var view = new Bz53381(useCompiledXaml);
				Application.Current.LoadPage(new ContentPage { Content = view });
				var presenter = ((StackLayout)view.InternalChildren[0]).Children[1] as ContentPresenter;
				Assume.That(presenter, Is.Not.Null);
				var grid = presenter.Content as Grid;
				Assert.That(grid, Is.Not.Null);
				Assert.That(grid.BackgroundColor, Is.EqualTo(Colors.Green));
			}
		}
	}
}