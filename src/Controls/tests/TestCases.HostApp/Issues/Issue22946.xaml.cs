using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 22946, "Editor and ScrollView problems in iOS", PlatformAffected.iOS)]
	public partial class Issue22946 : ContentPage
	{
		public Issue22946()
		{
			InitializeComponent();
		}

        private void OnCounterClicked(object sender, EventArgs e)
        {
            edtOcrResult.Text = "AAAAA-BBBBB CCCCC-DDDDD EEEEE-FFFFF GGGGG-HHHHH IIIII-JJJJJ KKKKK-LLLLL MMMMM-NNNNN OOOOO-PPPPP QQQQQ-RRRRR SSSSS-TTTTT UUUUU-VVVVV WWWWW-XXXXX YYYYY-ZZZZZ Welcome to .NET Multi-platform App UI";
        }
	}
}