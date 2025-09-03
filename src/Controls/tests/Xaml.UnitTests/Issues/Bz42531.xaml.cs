using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz42531 : ContentPage
{
	public Bz42531()
	{
		InitializeComponent();
	}


	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void RDInDataTemplates([Values] XamlInflator inflator)
		{
			var p = new Bz42531(inflator);
			ListView lv = p.lv;
			var template = lv.ItemTemplate;
			var cell = template.CreateContent(null, lv) as ViewCell;
			var sl = cell.View as StackLayout;
			Assert.AreEqual(1, sl.Resources.Count);
			var label = sl.Children[0] as Label;
			Assert.AreEqual(LayoutOptions.Center, label.HorizontalOptions);
		}
	}
}
