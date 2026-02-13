using System.Collections.Generic;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public enum Maui13856UserSetting
{
	BrowserInvisible,
	GlobalWaitForElementsInBrowserInSek,
	TBD,
}

public partial class Maui13856 : ContentPage
{
	public Maui13856()
	{
		InitializeComponent();
	}

	public Dictionary<Maui13856UserSetting, object> UserSettings { get; set; } = new Dictionary<Maui13856UserSetting, object>
	{
		{ Maui13856UserSetting.TBD, "test value" }
	};

	[TestFixture]
	class Tests
	{
		[Test]
		public void DictionaryWithEnumKeyBinding([Values] XamlInflator inflator)
		{
			// https://github.com/dotnet/maui/issues/13856
			// Binding to Dictionary<CustomEnum, object> with x:DataType should compile and work
			var page = new Maui13856(inflator);
			page.BindingContext = page;
			
			Assert.That(page.entry.Text, Is.EqualTo("test value"));
		}
	}
}
