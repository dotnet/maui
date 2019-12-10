using NUnit.Framework;

namespace Xamarin.Forms.Controls.Tests
{
	[TestFixture]
	public class CrossPlatformTests
	{
		[Test(Description = "Always Passes")]
		public void PassingCrossPlatformTest()
		{
			Assert.Pass();
		}

		[Test(Description = "Setting ListView Header to null should not crash")]
		public void Bugzilla28575()
		{
			string header = "Hello I am Header!!!!";

			var listview = new ListView();
			listview.Header = new Label()
			{
				Text = header,
				TextColor = Color.Red,
#pragma warning disable 618
				XAlign = TextAlignment.Center
#pragma warning restore 618
			};

			listview.Header = null;
		}

		[Test(Description = "isPresentedChanged raises multiple times")]
		public void Bugzilla32230()
		{
			var mdp = new MasterDetailPage();
			var count = 0;
			mdp.IsPresentedChanged += (sender, args) => { count += 1; };

			mdp.IsPresented = true;
			Assert.That(count, Is.EqualTo(1));

			mdp.IsPresented = false;
			mdp.IsPresented = true;
			Assert.That(count, Is.EqualTo(3));
		}
	}
}
