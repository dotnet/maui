using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;


namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1717, "Allow DetectReadingOrderFromContent on UWP", PlatformAffected.UWP)]
	public class Issue1717 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		Entry _entry1;
		Entry _entry2;
		Label _label1;
		Label _label2;

		Editor _editor1;
		Editor _editor2;
		Label _label3;
		Label _label4;
		Label _label5;
		Label _label6;

		private void DetectFromContent(bool detect) 
		{
			_entry1.On<Windows>().SetDetectReadingOrderFromContent(detect);
			_entry2.On<Windows>().SetDetectReadingOrderFromContent(detect);
			_editor1.On<Windows>().SetDetectReadingOrderFromContent(detect);
			_editor2.On<Windows>().SetDetectReadingOrderFromContent(detect);
			_label5.On<Windows>().SetDetectReadingOrderFromContent(detect);
			UpdateLabels();
		}

		void UpdateLabels()
		{
			_label1.Text = $"FlowDirection: {_entry1.FlowDirection}, DetectReadingOrderFromContent: {_entry1.On<Windows>().GetDetectReadingOrderFromContent()}";
			_label2.Text = $"FlowDirection: {_entry2.FlowDirection}, DetectReadingOrderFromContent: {_entry2.On<Windows>().GetDetectReadingOrderFromContent()}";
			_label3.Text = $"FlowDirection: {_editor1.FlowDirection}, DetectReadingOrderFromContent: {_editor1.On<Windows>().GetDetectReadingOrderFromContent()}";
			_label4.Text = $"FlowDirection: {_editor2.FlowDirection}, DetectReadingOrderFromContent: {_editor2.On<Windows>().GetDetectReadingOrderFromContent()}";
			_label6.Text = $"FlowDirection: {_label5.FlowDirection}, DetectReadingOrderFromContent: {_label5.On<Windows>().GetDetectReadingOrderFromContent()}";
		}

		protected override void Init()
		{
			_entry1 = new Entry 
			{
				Text = "היסט?שכל !ורי !ה שכל ב",
				FlowDirection = FlowDirection.LeftToRight
			};
			_entry2 = new Entry
			{
				Text = "Hello Xamarin Forms! Hello World!‬",
				FlowDirection = FlowDirection.RightToLeft
			};
			_editor1 = new Editor ()
			{
				Text = " שכל, ניווט ומהימנה תאולוגיה היא ב, זכר או מדעי תרומה מבוקשים. של ויש טכנולוגיה סוציולוגיה, מה אנא ביולי בקלות למחיקה. על חשמל אקטואליה רבה, שדרות ערכים ננקטת שמו בה. או עוד ציור מיזמים טבלאות, ריקוד קולנוע היסטוריה שכל ב.",
				FlowDirection = FlowDirection.LeftToRight
			};
			_editor2 = new Editor ()
			{
				Text = "Lorem ipsum dolor sit amet, qui eleifend adversarium ei, pro tamquam pertinax inimicus ut. Quis assentior ius no, ne vel modo tantas omnium, sint labitur id nec. Mel ad cetero repudiare definiebas, eos sint placerat cu.",
				FlowDirection = FlowDirection.LeftToRight
			};
			_label5 = new Label
			{
				Text = "היסט?שכל !ורי !ה שכל ב",
				FlowDirection = FlowDirection.LeftToRight
			};
			var buttonDetectFromContent = new Button 
			{
				Text = "Detect from content",
			};
			buttonDetectFromContent.Clicked += (x, o) => DetectFromContent(true);

			var buttonUseDefault = new Button 
			{
				Text = "Use FlowDirection"
			};
			buttonUseDefault.Clicked += (x, o) => DetectFromContent(false);
			_label1 = new Label();
			_label2 = new Label();
			_label3 = new Label();
			_label4 = new Label();
			_label6 = new Label();
			UpdateLabels();

			var stack = new StackLayout 
			{
				Children = {
					_entry1,
					_label1,
					_entry2,
					_label2,
					_editor1,
					_label3,
					_editor2,
					_label4,
					_label5,
					_label6,
					buttonDetectFromContent,
					buttonUseDefault,
				}
			};

			// Initialize ui here instead of ctor
			Content = stack;
		}
		
	}
}