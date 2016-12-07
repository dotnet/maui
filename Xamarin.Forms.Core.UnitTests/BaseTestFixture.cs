using System;
using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public class BaseTestFixture
	{
		[SetUp]
		public virtual void Setup ()
		{
#if !WINDOWS_PHONE
			var culture = Environment.GetEnvironmentVariable ("UNIT_TEST_CULTURE");
			
			if (!string.IsNullOrEmpty (culture)) {
				var thead = Thread.CurrentThread;
				thead.CurrentCulture = new CultureInfo (culture);
			}
#endif
		}

		[TearDown]
		public virtual void TearDown ()
		{
			
		}
	}
}
