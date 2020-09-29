using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DataPackageTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void PropertySetters()
		{
			var dataPackage = new DataPackage();

			ImageSource imageSource = "somefile.jpg";
			dataPackage.Text = "text";
			dataPackage.Image = imageSource;
			dataPackage.Properties["key"] = "value";

			Assert.AreEqual("text", dataPackage.Text);
			Assert.AreEqual(imageSource, dataPackage.Image);
			Assert.AreEqual(dataPackage.Properties["key"], "value");
		}

		[Test]
		public async Task DataPackageViewGetters()
		{
			var dataPackage = new DataPackage();

			ImageSource imageSource = "somefile.jpg";
			dataPackage.Text = "text";
			dataPackage.Image = imageSource;
			dataPackage.Properties["key"] = "value";
			var dataView = dataPackage.View;

			Assert.AreEqual("text", await dataView.GetTextAsync());
			Assert.AreEqual(imageSource, await dataView.GetImageAsync());
			Assert.AreEqual(dataView.Properties["key"], "value");
		}


		[Test]
		public async Task DataPackageViewGettersArentTiedToInitialDataPackage()
		{
			var dataPackage = new DataPackage();

			ImageSource imageSource = "somefile.jpg";
			dataPackage.Text = "text";
			dataPackage.Image = imageSource;
			dataPackage.Properties["key"] = "value";
			var dataView = dataPackage.View;


			dataPackage.Text = "fail";
			dataPackage.Image = "differentfile.jpg";
			dataPackage.Properties["key"] = "fail";


			Assert.AreEqual("text", await dataView.GetTextAsync());
			Assert.AreEqual(imageSource, await dataView.GetImageAsync());
			Assert.AreEqual(dataView.Properties["key"], "value");
		}
	}
}