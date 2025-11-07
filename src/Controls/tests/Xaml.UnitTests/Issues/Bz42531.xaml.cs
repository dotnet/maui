using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz42531 : ContentPage
{
	public Bz42531()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void RDInDataTemplates(XamlInflator inflator)
		{
			var p = new Bz42531(inflator);
			ListView lv = p.lv;
			var template = lv.ItemTemplate;
			var cell = template.CreateContent(null, lv) as ViewCell;
			var sl = cell.View as StackLayout;
			Assert.Single(sl.Resources);
			var label = sl.Children[0] as Label;
			Assert.Equal(LayoutOptions.Center, label.HorizontalOptions);
		}
	}
}
