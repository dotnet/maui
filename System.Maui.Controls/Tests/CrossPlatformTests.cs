using System.Threading.Tasks;
using NUnit.Framework;

namespace System.Maui.Controls.Tests
{
	[TestFixture]
	public class CrossPlatformTests : CrossPlatformTestFixture
	{
		[Test]
		[Description("Always Passes")]
		public void PassingCrossPlatformTest()
		{
			Assert.Pass();
		}

		[Test]
		[Description("Setting ListView Header to null should not crash")]
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

		[Test]
		[Description("isPresentedChanged raises multiple times")]
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

		[Test]
		[Description("ButtonRenderer UpdateTextColor function crash")]
		public async Task Bugzilla35738() 
		{
			var customButton = new TestClasses.CustomButton() { Text = "This is a custom button", TextColor = Color.Fuchsia };
			await TestingPlatform.CreateRenderer(customButton);
		}

		[Test]
		[Description("[Bug] CollectionView exception when IsGrouped=true and null ItemSource")]
		public async Task GitHub8269() 
		{
			var collectionView = new CollectionView { ItemsSource = null, IsGrouped = true };
			await TestingPlatform.CreateRenderer(collectionView);
		}

		[Test]
		[Description("[Bug] [UWP] NullReferenceException when call SavePropertiesAsync method off the main thread")]
		public async Task GitHub8682()
		{
			await Task.Run(async () =>
			{
				await Application.Current.SavePropertiesAsync();
			});
		}
	}
}
