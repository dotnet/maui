namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "RadioButton: Template From Style", PlatformAffected.All)]
public class RadioButtonTemplateFromStyle : TestNavigationPage
{
	protected override void Init()
	{

		PushAsync(new Pages.RadioButtonGalleries.TemplateFromStyle());

	}
}
