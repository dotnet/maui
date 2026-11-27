using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3868 : Shell
{
	public Maui3868() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void StaticShellContentParametersLoadAndApply(XamlInflator inflator)
		{
			var shell = new Maui3868(inflator);
			shell.Yesterday.ContentTemplate = CreateTemplate();
			shell.Today.ContentTemplate = CreateTemplate();
			shell.Tomorrow.ContentTemplate = CreateTemplate();

			Assert.Equal("2021-12-13", GetPage(shell.Yesterday).Date);

			shell.ReminderTab.CurrentItem = shell.Today;
			Assert.Equal("2021-12-14", GetPage(shell.Today).Date);
			Assert.Equal("xaml", GetPage(shell.Today).ReceivedQuery["source"]);

			shell.ReminderTab.CurrentItem = shell.Tomorrow;
			Assert.Equal("2021-12-15", GetPage(shell.Tomorrow).Date);
		}

		static Maui3868ReminderPage GetPage(ShellContent content) =>
			(Maui3868ReminderPage)((IShellContentController)content).GetOrCreateContent();

		static Microsoft.Maui.Controls.DataTemplate CreateTemplate() =>
			new(() => new Maui3868ReminderPage());
	}
}

public class Maui3868ReminderPage : ContentPage, IQueryAttributable
{
	public string Date { get; private set; }
	public IDictionary<string, object> ReceivedQuery { get; private set; }

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		ReceivedQuery = new Dictionary<string, object>(query);
		Date = (string)query["date"];
	}
}
