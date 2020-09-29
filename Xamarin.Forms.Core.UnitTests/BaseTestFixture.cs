using System;
using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public class BaseTestFixture
	{
		CultureInfo _defaultCulture;
		CultureInfo _defaultUICulture;

		[SetUp]
		public virtual void Setup()
		{
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_defaultUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public virtual void TearDown()
		{
			Device.PlatformServices = null;
			System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
		}
	}
}