using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz30074 : ContentPage
	{
		public Bz30074()
		{
			InitializeComponent();
		}

		[Collection("Issue")]
		public class Tests
		{
			[Theory]
			[XamlInflatorData]
			internal void DataTriggerInTemplates(XamlInflator inflator)
			{
				var layout = new Bz30074(inflator);
				Assert.Null(layout.image.Source);

				layout.BindingContext = new { IsSelected = true };
				Assert.Equal("Add.png", ((FileImageSource)layout.image.Source).File);

				layout.BindingContext = new { IsSelected = false };
				Assert.Null(layout.image.Source);
			}
		}
	}
}

