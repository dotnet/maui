
namespace Maui.Controls.Sample;

public class TransientPage : ContentPage
{
	static int i = 0;
	public TransientPage()
	{
		Index = i;
		Content = new Label { Text = $"I'm a transient page: {Index}" };
		i++;
	}

	public int Index { get; private set; }
}