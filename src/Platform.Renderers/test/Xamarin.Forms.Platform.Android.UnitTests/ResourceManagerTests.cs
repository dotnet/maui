using System;
using System.Threading.Tasks;
using Android.Views;
using NUnit.Framework;

namespace Xamarin.Forms.Platform.Android.UnitTests
{
	[TestFixture]
	public class ResourceManagerTests : PlatformTestFixture
	{
		[Test, Category("Resource")]
		[Description("Retrieve Resources by Name")]
		public void RetrieveResourcesByName()
		{
			ResourceManager.Init(null);
			ResourceManager.DrawableClass = null;
			ResourceManager.LayoutClass = null;
			ResourceManager.ResourceClass = null;
			ResourceManager.StyleClass = null;

			Assert.Greater(ResourceManager.GetDrawableId(this.Context, "DrawableTEST"), 0);
			Assert.Greater(ResourceManager.GetDrawableId(this.Context, "DrawableTEST.png"), 0);
			Assert.Greater(ResourceManager.GetLayout(this.Context, "LayoutTest"), 0);
			Assert.Greater(ResourceManager.GetStyle(this.Context, "TextAllCapsStyleTrue"), 0);
			Assert.Greater(ResourceManager.GetResource(this.Context, "namewith.adot"), 0);
			Assert.Greater(ResourceManager.GetResource(this.Context, "namewith_adot"), 0);

		}
	}
}
