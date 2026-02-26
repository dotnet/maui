using System.Collections.Generic;
using Xunit;

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

[Collection("Issue")]
public class Tests
{
[Theory]
[XamlInflatorData]
internal void DictionaryWithEnumKeyBinding(XamlInflator inflator)
{
// https://github.com/dotnet/maui/issues/13856
// Binding to Dictionary<CustomEnum, object> with x:DataType should compile and work
// .rt.xaml files are runtime-only (no XamlC or SourceGen code generated)
if (inflator != XamlInflator.Runtime)
return;

var page = new Maui13856(inflator);
page.BindingContext = page;

Assert.Equal("test value", page.entry.Text);
}
}
}
