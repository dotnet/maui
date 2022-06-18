using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Text;

namespace GraphicsTester.Scenarios
{
	public class DrawMarkdigAttributedText : AbstractScenario
	{
		public DrawMarkdigAttributedText()
			: base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.StrokeSize = 1;
			canvas.StrokeColor = Colors.Blue;
			canvas.Font = Font.Default;
			canvas.FontSize = 12f;

			canvas.SaveState();

			var value = @"This is *italic* and __underline__ and **bold** and __*underline italic*__ and __**underline bold**__ and ***bold italic*** and __***underline bold italic***__.
This is ~~strike through~~.
This is ~sub~script and this is ^super^script.
This is <span style=""color:blue"">blue text</span> and <span style=""background:yellow"">highlighted text</span>

This is a list:
* line 1
* line 2";

			var attributedText = MarkdownAttributedTextReader.Read(value);
			canvas.Translate(0, 10);
			canvas.DrawRectangle(10, 0, 400, 400);
			canvas.DrawText(attributedText, 10, 0, 400, 400);
			canvas.RestoreState();
		}
	}
}
