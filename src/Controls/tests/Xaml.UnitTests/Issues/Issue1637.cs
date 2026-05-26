using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[Collection("Issue")]
	public class Issue1637
	{
		[Fact]
		public void ImplicitCollectionWithSingleElement()
		{
			var xaml = @"
				<Grid xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"">
					<Grid.RowDefinitions>
						<RowDefinition Height=""*"" />
			        </Grid.RowDefinitions>
				</Grid>";
			var grid = new Grid();
			var ex = Record.Exception(() => grid.LoadFromXaml<Grid>(xaml));
			Assert.Null(ex);
			Assert.Single(grid.RowDefinitions);
			Assert.True(grid.RowDefinitions[0].Height.IsStar);
		}
	}
}

