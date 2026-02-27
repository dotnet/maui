using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[Collection("Issue")]
	public class Issue1564
	{
		[Fact]
		public void ViewCellAsXamlRoot()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
				<ViewCell 
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
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
			Assert.Equal("This is my label", ((cell.View as StackLayout).Children[0] as Label).Text);
		}
	}
}

