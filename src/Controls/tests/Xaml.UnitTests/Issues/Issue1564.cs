using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1564
	{
		[Test]
		public void ViewCellAsXamlRoot()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
				<ViewCell 
					xmlns=""http://xamarin.com/schemas/2014/forms"" 
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" 
					x:Class=""m.transport.VehicleCell"">
				    <ViewCell.View>
				        <StackLayout>
					        <Label Text=""This is my label""></Label>
				        </StackLayout>
				    </ViewCell.View>
				</ViewCell>";
			var cell = new ViewCell().LoadFromXaml(xaml);
			Assert.NotNull(cell);
			Assert.AreEqual("This is my label", ((cell.View as StackLayout).Children[0] as Label).Text);
		}
	}
}

