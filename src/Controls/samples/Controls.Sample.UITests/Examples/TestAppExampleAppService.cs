using System.Threading.Tasks;
using ExampleFramework.Tooling;
using ExampleFramework.Tooling.Maui;
using VisualTestUtils;

namespace Maui.Controls.Sample;

public class TestAppExampleAppService : MauiExampleAppService
{
    public override Task NaviateToExampleAsync(string componentName, string exampleName)
    {
        UIExample example = GetExample(componentName, exampleName);

		ExamplesGalleryPage examplesGalleryPage = new ExamplesGalleryPage(example);

		App.Current.MainPage = examplesGalleryPage;

		return Task.CompletedTask;
    }

    public override async Task<ImageSnapshot> GetExampleSnapshotAsync(string componentName, string exampleName)
    {
		UIExample example = GetExample(componentName, exampleName);

		ExamplesGalleryPage examplesGalleryPage = new ExamplesGalleryPage(example);
		App.Current.MainPage = examplesGalleryPage;

		return await examplesGalleryPage.GetExampleSnapshotAsync();
	}
}
