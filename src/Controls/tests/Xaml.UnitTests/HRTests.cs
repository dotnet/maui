using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class HRTests
	{
		[SetUp]
		public void SetUp()
		{
			Application.Current = null;
		}

		[TearDown]
		public void TearDown()
		{
			Maui.Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
			Application.ClearCurrent();
		}

		[Test]
		public void LoadResources()
		{
			var app = @"
				<Application xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
					<Application.Resources>
						<ResourceDictionary>
							<Color x:Key=""almostPink"">HotPink</Color>
						</ResourceDictionary>
					</Application.Resources>
				</Application>
			";
			Assert.That(Application.Current, Is.Null);
			var mockApplication = new MockApplication();
			var rd = XamlLoader.LoadResources(app, mockApplication);
			Assert.That(rd, Is.TypeOf<ResourceDictionary>());
			Assert.That(((ResourceDictionary)rd).Count, Is.EqualTo(1));

			//check that the live app hasn't ben modified
			Assert.That(Application.Current, Is.EqualTo(mockApplication));
			Assert.That(Application.Current.Resources.Count, Is.EqualTo(0));
		}

		[Test]
		public void LoadMultipleResources()
		{
			var app = @"
				<Application xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
					<Application.Resources>
						<Color x:Key=""almostPink"">HotPink</Color>
						<Color x:Key=""yellowOrGreen"">Chartreuse</Color>
					</Application.Resources>
				</Application>
			";

			Assert.That(Application.Current, Is.Null);
			var mockApplication = new MockApplication();
			var rd = XamlLoader.LoadResources(app, mockApplication);
			Assert.That(rd, Is.TypeOf<ResourceDictionary>());
			Assert.That(((ResourceDictionary)rd).Count, Is.EqualTo(2));

			//check that the live app hasn't ben modified
			Assert.That(Application.Current, Is.EqualTo(mockApplication));
			Assert.That(Application.Current.Resources.Count, Is.EqualTo(0));
		}

		[Test]
		public void LoadSingleImplicitResources()
		{
			var app = @"
				<Application xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
					<Application.Resources>
						<Color x:Key=""almostPink"">HotPink</Color>
					</Application.Resources>
				</Application>
			";

			Assert.That(Application.Current, Is.Null);
			var mockApplication = new MockApplication();
			var rd = XamlLoader.LoadResources(app, mockApplication);
			Assert.That(rd, Is.TypeOf<ResourceDictionary>());
			Assert.That(((ResourceDictionary)rd).Count, Is.EqualTo(1));

			//check that the live app hasn't ben modified
			Assert.That(Application.Current, Is.EqualTo(mockApplication));
			Assert.That(Application.Current.Resources.Count, Is.EqualTo(0));
		}
	}
}
