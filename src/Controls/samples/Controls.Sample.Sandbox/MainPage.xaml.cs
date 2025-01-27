using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		var target = new InvokeJavaScriptTarget();

		hwv.SetInvokeJavaScriptTarget(target);

		hwv.RawMessageReceived +=  (sender, e) =>
		{
			//var result = FakeHWV.Invoke(e.Message!);

			//var obj = await hwv.InvokeJavaScriptAsync<string?>(
			//	"dotnetCallback",
			//	null!,
			//	[result],
			//	[JSObjContext.Default.String]);
		};
	}

	private void CallJS(object sender, EventArgs e)
	{
	}

	[JsonSourceGenerationOptions]
	[JsonSerializable(typeof(string))]
	private partial class JSObjContext : JsonSerializerContext
	{
	}
}

public partial class InvokeJavaScriptTarget
{
	// TODO: JSExport
	public JSObj GetName(JSTest obj)
	{
		return new JSObj(obj.second, obj.first);
	}
}

public record class JSObj(string? first, string? second);

public record class JSTest(string? first, string? second);
