using Microsoft.Maui.ManualTests.Models;

namespace Microsoft.Maui.ManualTests.Tests.ScrollView;

public partial class MonkeyDetailPage : ContentPage
{
	public MonkeyDetailPage(Monkey monkey)
	{
		InitializeComponent();
		BindingContext = monkey;
	}
}
