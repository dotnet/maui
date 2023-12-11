string TARGET = Argument("target", "Test");

string DEFAULT_PROJECT = "";
string DEFAULT_APP_PROJECT = "";

if (string.Equals(TARGET, "uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Controls/tests/UITests/Controls.AppiumTests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Controls/samples/Controls.Sample.UITests/Controls.Sample.UITests.csproj";
}

if (string.Equals(TARGET, "cg-uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Compatibility/ControlGallery/test/iOS.UITests/Compatibility.ControlGallery.iOS.UITests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Compatibility/ControlGallery/src/iOS/Compatibility.ControlGallery.iOS.csproj";
}