using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{	public class HRTests
	{
		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
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
			Assert.Equal(1, ((ResourceDictionary)rd).Count);

			//check that the live app hasn't ben modified
			Assert.Equal(mockApplication, Application.Current);
			Assert.Equal(0, Application.Current.Resources.Count);
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
			Assert.Equal(0, Application.Current.Resources.Count);
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
			Assert.Equal(1, ((ResourceDictionary)rd).Count);

			//check that the live app hasn't ben modified
			Assert.Equal(mockApplication, Application.Current);
			Assert.Equal(0, Application.Current.Resources.Count);
		}
	}
}
