using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Issue1497
	{
		[Fact]
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
			Assert.Single(grid.ColumnDefinitions);
			Assert.True(grid.ColumnDefinitions[0].Width.IsStar);
		}
	}
}