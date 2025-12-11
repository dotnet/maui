using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[Collection("Xaml Inflation")]
	public class HRTests : IDisposable
	{
		public HRTests()
		{
			Application.Current = null;
		}

		public void Dispose()
		{
			Maui.Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
			Application.ClearCurrent();
		}

		[Fact]
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
			Assert.Null(Application.Current);
			var mockApplication = new MockApplication();
			var rd = XamlLoader.LoadResources(app, mockApplication);
			Assert.IsType<ResourceDictionary>(rd);
			Assert.Single((ResourceDictionary)rd);

			//check that the live app hasn't ben modified
			Assert.Equal(mockApplication, Application.Current);
			Assert.Empty(Application.Current.Resources);
		}

		[Fact]
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

			Assert.Null(Application.Current);
			var mockApplication = new MockApplication();
			var rd = XamlLoader.LoadResources(app, mockApplication);
			Assert.IsType<ResourceDictionary>(rd);
			Assert.Equal(2, ((ResourceDictionary)rd).Count);

			//check that the live app hasn't ben modified
			Assert.Equal(mockApplication, Application.Current);
			Assert.Empty(Application.Current.Resources);
		}

		[Fact]
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

			Assert.Null(Application.Current);
			var mockApplication = new MockApplication();
			var rd = XamlLoader.LoadResources(app, mockApplication);
			Assert.IsType<ResourceDictionary>(rd);
			Assert.Single((ResourceDictionary)rd);

			//check that the live app hasn't ben modified
			Assert.Equal(mockApplication, Application.Current);
			Assert.Empty(Application.Current.Resources);
		}
	}
}
