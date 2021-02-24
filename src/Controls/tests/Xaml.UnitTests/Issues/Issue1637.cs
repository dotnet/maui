using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1637
	{
		[Test]
		public void ImplicitCollectionWithSingleElement()
		{
			var xaml = @"
				<Grid xmlns=""http://xamarin.com/schemas/2014/forms"">
					<Grid.RowDefinitions>
						<RowDefinition Height=""*"" />
			        </Grid.RowDefinitions>
				</Grid>";
			var grid = new Grid();
			Assert.DoesNotThrow(() => grid.LoadFromXaml<Grid>(xaml));
			Assert.AreEqual(1, grid.RowDefinitions.Count);
			Assert.IsTrue(grid.RowDefinitions[0].Height.IsStar);
		}
	}
}

