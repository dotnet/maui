
namespace Maui.Controls.Sample;

public class ScopedPage : ContentPage
{
	static int i = 0;
	public ScopedPage()
	{
		Index = i;
		Content = new Label { Text = $"I'm a scoped page: {Index}" };
		i++;
	}

	public int Index { get; private set; }
}