using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DataPackageTests : BaseTestFixture
	{
		[Fact]
		public void PropertySetters()
		{
			var dataPackage = new DataPackage();

			ImageSource imageSource = "somefile.jpg";
			dataPackage.Text = "text";
			dataPackage.Image = imageSource;
			dataPackage.Properties["key"] = "value";

			Assert.Equal("text", dataPackage.Text);
			Assert.Equal(imageSource, dataPackage.Image);
			Assert.Equal("value", dataPackage.Properties["key"]);
		}

		[Fact]
		public async Task DataPackageViewGetters()
		{
			var dataPackage = new DataPackage();

			ImageSource imageSource = "somefile.jpg";
			dataPackage.Text = "text";
			dataPackage.Image = imageSource;
			dataPackage.Properties["key"] = "value";
			var dataView = dataPackage.View;

			Assert.Equal("text", await dataView.GetTextAsync());
			Assert.Equal(imageSource, await dataView.GetImageAsync());
			Assert.Equal("value", dataView.Properties["key"]);
		}


		[Fact]
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


			Assert.Equal("text", await dataView.GetTextAsync());
			Assert.Equal(imageSource, await dataView.GetImageAsync());
			Assert.Equal("value", dataView.Properties["key"]);
		}
	}
}
