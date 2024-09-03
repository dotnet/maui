namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24174, "DatePicker does not leak", PlatformAffected.All)]
	public class Issue24174 : NavigationPage
	{
		public Issue24174() 
		{
            this.RunMemoryTest(() =>
            {
                return new DatePicker
                {
                    AutomationId = "DatePicker",
                    Date = new DateTime(2021, 1, 1)
                };
            });
        }

    
	}
}