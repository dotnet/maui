namespace Maui.Controls.Sample.Issues;

// x:Name="issue34975SecondPage" in the XAML is the key trigger for the memory leak.
// It causes the page to register itself in its own XAML NameScope, which combined with
// Shell.TitleView creates a retain cycle on iOS that prevents GC collection.
public partial class Issue34975SecondPage : ContentPage
{
	public static List<WeakReference> Instances { get; } = [];

	public Issue34975SecondPage()
	{
		InitializeComponent();
		Instances.Add(new WeakReference(this));
	}
}
