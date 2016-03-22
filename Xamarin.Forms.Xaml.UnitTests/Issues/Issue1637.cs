using System;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1637
	{
		[Test]
		public void ImplicitCollectionWithSingleElement ()
		{
			var xaml = @"
				<Grid>
					<Grid.RowDefinitions>
						<RowDefinition Height=""*"" />
			        </Grid.RowDefinitions>
				</Grid>";
			var grid = new Grid ();
			Assert.DoesNotThrow(()=> grid.LoadFromXaml<Grid> (xaml));
			Assert.AreEqual (1, grid.RowDefinitions.Count);
			Assert.IsTrue (grid.RowDefinitions [0].Height.IsStar);
		}
	}
}

