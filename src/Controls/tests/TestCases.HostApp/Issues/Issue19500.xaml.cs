namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19500, "[iOS] Editor is not be able to scroll if IsReadOnly is true", PlatformAffected.iOS)]
	public partial class Issue19500 : ContentPage
	{
		public Issue19500()
		{
			InitializeComponent();
#if MACCATALYST
			// Adding extra text to make the editor scrollable on Catalyst
			editor.Text += " Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla porttitor mauris non ornare ultrices. Ut semper ultrices justo eget semper. Ut imperdiet dolor ut vestibulum molestie. Duis a libero ex. Etiam mi urna, lobortis sed tincidunt in, tempus eget magna. Aenean quis malesuada eros. Phasellus felis eros, condimentum et tortor sed, condimentum convallis turpis. Sed in varius metus, at auctor orci. Maecenas luctus nibh nibh, nec aliquam est fermentum in. Etiam consectetur lectus erat, sed placerat sapien rutrum eu. Suspendisse tincidunt fermentum tempor. Maecenas egestas neque nec lacinia fringilla.";
#endif
		}
	}
}