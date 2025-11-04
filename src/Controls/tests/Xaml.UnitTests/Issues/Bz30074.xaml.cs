using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz30074 : ContentPage
	{
		public Bz30074()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void DataTriggerInTemplates([Values] XamlInflator inflator)
			{
				var layout = new Bz30074(inflator);
				Assert.Null(layout.image.Source);

				layout.BindingContext = new { IsSelected = true };
				Assert.AreEqual("Add.png", ((FileImageSource)layout.image.Source).File);

				layout.BindingContext = new { IsSelected = false };
				Assert.Null(layout.image.Source);
			}
		}
	}
}

