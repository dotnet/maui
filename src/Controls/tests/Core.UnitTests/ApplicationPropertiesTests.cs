using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ApplicationPropertiesTests : BaseTestFixture
	{
		[Test]
		public void SettingAndRetrievingProperties()
		{
			var app = new Application();

			app.Properties["the_answer"] = "seven";

			var answer = app.Properties["the_answer"];

			Assert.AreEqual("seven", answer);
		}

		[Test]
		public void SettingAndUpdatingProperties()
		{
			var app = new Application();

			app.Properties["the_answer"] = "seven";
			app.Properties["the_answer"] = "after seven";

			var answer = app.Properties["the_answer"];

			Assert.AreEqual("after seven", answer);
		}

		[Test]
		public async Task SaveDoesNotAffectProperties()
		{
			var app = new Application();

			app.Properties["the_answer"] = "seven";

			await app.SavePropertiesAsync();

			Assert.AreEqual("seven", app.Properties["the_answer"]);
		}
	}
}
