using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Bugzilla, 60045,
	"ListView with RecycleElement strategy doesn't handle CanExecute of TextCell Command properly",
	PlatformAffected.iOS)]
public partial class Bugzilla60045 : TestContentPage
{
	public const string ClickThis = "Click This";
	public const string Fail = "Fail";

	public object Items { get; set; }

	public Bugzilla60045()
	{

		InitializeComponent();

	}

	protected override void Init()
	{
		Items = new[]
		{
			new {
				Action = new Command(async () =>
				{
					await DisplayAlert(Fail, "Well, this is embarrassing.", "Ok");
				},
				() => false) }
		};
	}
}
