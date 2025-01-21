using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 27194, "Border: PlatformView cannot be null here | Happens randomly in release builds", PlatformAffected.Android)]
	public partial class Issue27194 : TestContentPage
	{
		bool _updatedStyle;
		bool _updatedSrokeShape;

		public Issue27194()
		{
			InitializeComponent();

		}

		protected override void Init()
		{

		}

		void UpdateStrokeShapeClicked(object sender, EventArgs e)
		{
			if (_updatedSrokeShape)
			{
				BorderTest1.StrokeShape = BorderTest2.StrokeShape = Resources["StrokeShape1"] as Shape;
				_updatedSrokeShape = false;
			}
			else
			{
				BorderTest1.StrokeShape = BorderTest2.StrokeShape = Resources["StrokeShape2"] as Shape;
				_updatedSrokeShape = true;
			}
		}

		void UpdateStyleClicked(object sender, EventArgs e)
		{
			if (_updatedStyle)
			{
				BorderTest1.Style = BorderTest2.Style = Resources["BorderStyle1"] as Style;
				_updatedStyle = false;
			}
			else
			{
				BorderTest1.Style = BorderTest2.Style = Resources["BorderStyle2"] as Style;
				_updatedStyle = true;
			}
		}
	}
}