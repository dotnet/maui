using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.ControlGallery.Android.Tests
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
			var actual = await GetControlProperty(label, control => control.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Button")]
		[Description("Button text should match renderer text")]
		public async Task ButtonTextMatchesRendererText()
		{
			var button = new Button { Text = "foo" };
			var expected = button.Text;
			var actual = await GetControlProperty(button, control => control.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Entry")]
		[Description("Entry text should match renderer text")]
		public async Task EntryTextMatchesRendererText()
		{
			var entry = new Entry { Text = "foo" };
			var expected = entry.Text;
			var actual = await GetControlProperty(entry, control => control.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Text"), Category("Editor")]
		[Description("Editor text should match renderer text")]
		public async Task EditorTextMatchesRendererText()
		{
			var editor = new Editor { Text = "foo" };
			var expected = editor.Text;
			var actual = await GetControlProperty(editor, control => control.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}