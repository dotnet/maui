using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ApplicationPropertiesTests
	{
		[Test]
		public void SettingAndRetrievingProperties()
		{
			var app = new StubApp();

			app.Properties["the_answer"] = "seven";

			var answer = app.Properties["the_anser"];

			Assert.AreEqual("seven", answer);
		}

		[Test]
		public void PropertiesCanSave()
		{
			var app = new StubApp();

			app.Properties["the_answer"] = "seven";
			app.SavePropertiesAsFireAndForget();
		}

		class StubApp : Application
		{
		}
	}
}
