using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class DesignPropertiesTests
	{
		[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
		[TearDown] public void TearDown() => Device.PlatformServices = null;

		[Test]
		public void DesignProperties()
		{
			var xaml = @"
				<ContentPage
						xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
						xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
						xmlns:d=""http://schemas.microsoft.com/dotnet/2021/maui/design""
						xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
						mc:Ignorable=""d"">
					<Label  d:Text=""Bar"" Text=""Foo"" x:Name=""label"" />
				</ContentPage>";

			var view = new ContentPage();
			XamlLoader.Load(view, xaml, useDesignProperties: true); //this is equiv as LoadFromXaml, but with the bool set

			var label = ((Maui.Controls.Internals.INameScope)view).FindByName("label") as Label;

			Assert.That(label.Text, Is.EqualTo("Bar"));
		}
	}
}
