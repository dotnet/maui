using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[TestFixture]
	public class TextTests : PlatformTestFixture
	{
		[Test, Category("Text"), Category("Label")]
		[Description("Label text should match renderer text")]
		public async Task LabelTextMatchesRendererText()
		{
			var label = new Label { Text = "foo" };
			var expected = label.Text;
			var actual = await GetControlProperty(label, uiLabel => uiLabel.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Button")]
		[Description("Button text should match renderer text")]
		public async Task ButtonTextMatchesRendererText()
		{
			var button = new Button { Text = "foo" };
			var expected = button.Text;
			var actual = await GetControlProperty(button, uiButton => uiButton.TitleLabel.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Entry")]
		[Description("Entry text should match renderer text")]
		public async Task EntryTextMatchesRendererText()
		{
			var entry = new Entry { Text = "foo" };
			var expected = entry.Text;
			var actual = await GetControlProperty(entry, uiTextField => uiTextField.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Editor")]
		[Description("Editor text should match renderer text")]
		public async Task EditorTextMatchesRendererText()
		{
			var editor = new Editor { Text = "foo" };
			var expected = editor.Text;
			var actual = await GetControlProperty(editor, uiTextView => uiTextView.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}