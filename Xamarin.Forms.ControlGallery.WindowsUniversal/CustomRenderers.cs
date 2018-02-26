using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Controls.Bugzilla42602.TextBoxView), typeof(Xamarin.Forms.ControlGallery.WindowsUniversal.TextBoxViewRenderer))]
[assembly: ExportRenderer(typeof(Issue1683.EntryKeyboardFlags), typeof(EntryRendererKeyboardFlags))]
[assembly: ExportRenderer(typeof(Issue1683.EditorKeyboardFlags), typeof(EditorRendererKeyboardFlags))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class EntryRendererKeyboardFlags : EntryRenderer
	{
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			Control.SetKeyboardFlags(((Issue1683.EntryKeyboardFlags)Element).FlagsToSet);
			Control.TestKeyboardFlags(((Issue1683.EntryKeyboardFlags)Element).FlagsToSet);


		}
	}
	public class EditorRendererKeyboardFlags : EditorRenderer
	{
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			Control.SetKeyboardFlags(((Issue1683.EditorKeyboardFlags)Element).FlagsToSet);
			Control.TestKeyboardFlags(((Issue1683.EditorKeyboardFlags)Element).FlagsToSet);


		}
	}

	public static class KeyboardFlagExtensions
	{
		public static void TestKeyboardFlags(this FormsTextBox Control, KeyboardFlags? flags)
		{
			if (flags == null) { return; }
			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				if (!Control.IsSpellCheckEnabled)
				{
					throw new System.Exception("IsSpellCheckEnabled not enabled");
				}
			}
			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				if (!Control.InputScope.Names.Select(x => x.NameValue).Contains(InputScopeNameValue.NameOrPhoneNumber))
				{
					throw new System.Exception("Input Scope Not Set to NameOrPhoneNumber");
				}

				if (!Control.IsSpellCheckEnabled)
				{
					throw new System.Exception("IsSpellCheckEnabled not enabled");
				}

			}
			else
			{
				return;
			}
		}

		public static void SetKeyboardFlags(this FormsTextBox Control, KeyboardFlags? flags)
		{
			if (flags == null) { return; }
			var result = new InputScope();
			var value = InputScopeNameValue.Default;

			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				Control.IsSpellCheckEnabled = true;
			}
			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				value = InputScopeNameValue.NameOrPhoneNumber;
				Control.IsSpellCheckEnabled = true;
			}
			else
			{
				return;
			}


			InputScopeName nameValue = new InputScopeName();
			nameValue.NameValue = value;
			result.Names.Add(nameValue);
			Control.InputScope = result;
		}
	}


	public class TextBoxViewRenderer : BoxViewRenderer
	{
		Canvas m_Canvas;

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			ArrangeNativeChildren = true;

			if (m_Canvas != null)
				Children.Remove(m_Canvas);

			m_Canvas = new Canvas()
			{
				Width = 200,
				Height = 200,
				Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255)),
				IsHitTestVisible = false
			};

			Children.Add(m_Canvas);

			//ellipse
			Shape ellipse = new Ellipse()
			{
				Width = 100,
				Height = 100,
				Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),

			};
			Canvas.SetLeft(ellipse, 0);
			Canvas.SetTop(ellipse, 0);
			m_Canvas.Children.Add(ellipse);

			//text
			TextBlock text = new TextBlock()
			{
				FontSize = 50,
				FontWeight = Windows.UI.Text.FontWeights.Normal,
				Text = "hello world",
				Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0))
			};
			Canvas.SetLeft(text, 0);
			Canvas.SetTop(text, 150);
			m_Canvas.Children.Add(text);
		}
	}
}
