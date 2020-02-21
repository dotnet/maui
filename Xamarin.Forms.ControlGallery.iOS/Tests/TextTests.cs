using NUnit.Framework;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[TestFixture]
	public class TextTests : PlatformTestFixture
	{
		[Test, Category("Text"), Category("Label")]
		[Description("Label text should match renderer text")]
		public void LabelTextMatchesRendererText()
		{
			var label = new Label { Text = "foo" };
			using (var uiLabel = GetNativeControl(label))
			{
				Assert.That(label.Text, Is.EqualTo(uiLabel.Text));
			}
		}

		[Test, Category("Text"), Category("Button")]
		[Description("Button text should match renderer text")]
		public void ButtonTextMatchesRendererText()
		{
			var button = new Button { Text = "foo" };
			using (var nativeControl = GetNativeControl(button))
			{
				Assert.That(nativeControl.TitleLabel.Text, Is.EqualTo(button.Text));
			}
		}

		[Test, Category("Text"), Category("Entry")]
		[Description("Entry text should match renderer text")]
		public void EntryTextMatchesRendererText()
		{
			var entry = new Entry { Text = "foo" };
			using (var nativeControl = GetNativeControl(entry))
			{
				Assert.That(nativeControl.Text, Is.EqualTo(entry.Text));
			}
		}

		[Test, Category("Text"), Category("Editor")]
		[Description("Editor text should match renderer text")]
		public void EditorTextMatchesRendererText()
		{
			var editor = new Editor { Text = "foo" };
			using (var nativeControl = GetNativeControl(editor))
			{
				Assert.That(nativeControl.Text, Is.EqualTo(editor.Text));
			}
		}
	}
}