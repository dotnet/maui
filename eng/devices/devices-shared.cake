string TARGET = Argument("target", "Test");

string DEFAULT_PROJECT = "";
string DEFAULT_APP_PROJECT = "";

if (string.Equals(TARGET, "uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Controls/tests/UITests/Controls.AppiumTests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Controls/samples/Controls.Sample.UITests/Controls.Sample.UITests.csproj";
}
