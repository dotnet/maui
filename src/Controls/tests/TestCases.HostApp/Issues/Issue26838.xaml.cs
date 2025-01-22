namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26838, "[MAUI]After entering the string, ‘any records’ is not displayed.", PlatformAffected.iOS)]
	public partial class Issue26838 : TestContentPage
	{
		public Issue26838()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			BindingContext = new MonkeysViewModel();
		}
	}

	public class FilterData : BindableObject
	{
		public static readonly BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(string), typeof(FilterData), null);

		public string Filter
		{
			get { return (string)GetValue(FilterProperty); }
			set { SetValue(FilterProperty, value); }
		}
	}
}