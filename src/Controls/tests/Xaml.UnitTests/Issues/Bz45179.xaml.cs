using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz45179_0 : ContentView
{
	public static int creator_count;
	public Bz45179_0()
	{
		creator_count++;
	}

}
public partial class Bz45179 : ContentPage
{
	public Bz45179()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void DTDoNotInstantiateTheirContent(XamlInflator inflator)
		{
			Bz45179_0.creator_count = 0;
			Assert.Equal(0, Bz45179_0.creator_count);
			var page = new Bz45179(inflator);
			Assert.Equal(0, Bz45179_0.creator_count);
		}
	}
}