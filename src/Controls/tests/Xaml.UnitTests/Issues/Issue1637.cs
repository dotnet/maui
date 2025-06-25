using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{	public class Issue1637
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
			Assert.DoesNotThrow(() => grid.LoadFromXaml<Grid>(xaml));
			Assert.Equal(1, grid.RowDefinitions.Count);
			Assert.True(grid.RowDefinitions[0].Height.IsStar);
		}
	}
}

