using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[TestFixture]
public class Issue1497
{
	[Test]
	public void BPCollectionsWithSingleElement()
	{
		var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
						<Grid
							xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
							xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">	 
							<Grid.ColumnDefinitions>
								<ColumnDefinition Width=""*""/>
							</Grid.ColumnDefinitions>
					    </Grid>";

		var grid = new Grid().LoadFromXaml(xaml);
		Assert.AreEqual(1, grid.ColumnDefinitions.Count);
		Assert.True(grid.ColumnDefinitions[0].Width.IsStar);
	}
}