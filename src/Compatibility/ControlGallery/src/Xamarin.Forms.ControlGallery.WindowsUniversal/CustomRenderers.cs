using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Controls.Issues.Bugzilla42602.TextBoxView), typeof(Xamarin.Forms.ControlGallery.WindowsUniversal.TextBoxViewRenderer))]
[assembly: ExportRenderer(typeof(Issue1683.EntryKeyboardFlags), typeof(EntryRendererKeyboardFlags))]
[assembly: ExportRenderer(typeof(Issue1683.EditorKeyboardFlags), typeof(EditorRendererKeyboardFlags))]
[assembly: ExportRenderer(typeof(Issue3273.SortableListView), typeof(SortableListViewRenderer))]
[assembly: ExportRenderer(typeof(Issue2172OldEntry), typeof(Issue2172OldEntryRenderer))]
[assembly: ExportRenderer(typeof(Issue2172OldEditor), typeof(Issue2172OldEditorRenderer))]
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

	public class SortableListViewRenderer : ListViewRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				var control = Control as Microsoft.UI.Xaml.Controls.ListView;

				control.AllowDrop = true;
				control.CanDragItems = true;
				control.CanReorderItems = true;
				control.ReorderMode = ListViewReorderMode.Enabled;
			}
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


	public class TextBoxViewRenderer : BoxViewBorderRenderer
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
				Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255)),
				IsHitTestVisible = false
			};

			Children.Add(m_Canvas);

			//ellipse
			Shape ellipse = new Ellipse()
			{
				Width = 100,
				Height = 100,
				Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),

			};
			Canvas.SetLeft(ellipse, 0);
			Canvas.SetTop(ellipse, 0);
			m_Canvas.Children.Add(ellipse);

			//text
			TextBlock text = new TextBlock()
			{
				FontSize = 50,
				FontWeight = Microsoft.UI.Text.FontWeights.Normal,
				Text = "hello world",
				Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0))
			};
			Canvas.SetLeft(text, 0);
			Canvas.SetTop(text, 150);
			m_Canvas.Children.Add(text);
		}
	}

	public class Issue2172OldEntryRenderer : EntryRenderer
	{
		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Children.Count == 0 || Control == null)
				return new SizeRequest();

			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
			FormsTextBox child = Control;

			child.Measure(constraint);
			var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

			return new SizeRequest(result);
		}
	}

	public class Issue2172OldEditorRenderer : EditorRenderer
	{
		static FormsTextBox _copyOfTextBox;
		static Windows.Foundation.Size _zeroSize = new Windows.Foundation.Size(0, 0);

		FormsTextBox CreateTextBox()
		{
			return new FormsTextBox
			{
				AcceptsReturn = true,
				TextWrapping = TextWrapping.Wrap,
				Style = Microsoft.UI.Xaml.Application.Current.Resources["FormsTextBoxStyle"] as Microsoft.UI.Xaml.Style
			};
		}

		/*
		 * Purely invalidating the layout as text is added to the TextBox will not cause it to expand.
		 * If the TextBox is set to WordWrap and it is part of the layout it will refuse to Measure itself beyond its established width.
		 * Even giving it infinite constraints will cause it to always set its DesiredSize to the same width but with a vertical growth.
		 * The only way I was able to grow it was by setting layout renderers width explicitly to some value but then it just set its own Width to that Width which is not helpful.
		 * Even vertically it would measure oddly in cases of rapid text changes.
		 * Holding down the backspace key or enter key would cause the final result to be not quite right.
		 * Both of these issues were fixed by just creating a static TextBox that is not part of the layout which let me just measure
		 * the size of the text as it would fit into the TextBox unconstrained and then just return that Size from the GetDesiredSize call.
		 * */
		Size GetCopyOfSize(FormsTextBox control, Windows.Foundation.Size constraint)
		{
			if (_copyOfTextBox == null)
			{
				_copyOfTextBox = CreateTextBox();

				// This causes the copy to be initially setup correctly. 
				// I found that if the first measure of this copy occurs with Text then it will just keep defaulting to a measure with no text.
				_copyOfTextBox.Measure(_zeroSize);
			}

			_copyOfTextBox.Text = control.Text;
			_copyOfTextBox.FontSize = control.FontSize;
			_copyOfTextBox.FontFamily = control.FontFamily;
			_copyOfTextBox.FontStretch = control.FontStretch;
			_copyOfTextBox.FontStyle = control.FontStyle;
			_copyOfTextBox.FontWeight = control.FontWeight;
			_copyOfTextBox.Margin = control.Margin;
			_copyOfTextBox.Padding = control.Padding;

			// have to reset the measure to zero before it will re-measure itself
			_copyOfTextBox.Measure(_zeroSize);
			_copyOfTextBox.Measure(constraint);

			Size result = new Size
			(
				Math.Ceiling(_copyOfTextBox.DesiredSize.Width),
				Math.Ceiling(_copyOfTextBox.DesiredSize.Height)
			);

			return result;
		}


		SizeRequest CalculateDesiredSizes(FormsTextBox control, Windows.Foundation.Size constraint, EditorAutoSizeOption sizeOption)
		{
			if (sizeOption == EditorAutoSizeOption.TextChanges)
			{
				Size result = GetCopyOfSize(control, constraint);
				control.Measure(constraint);
				return new SizeRequest(result);
			}
			else
			{
				control.Measure(constraint);
				Size result = new Size(Math.Ceiling(control.DesiredSize.Width), Math.Ceiling(control.DesiredSize.Height));
				return new SizeRequest(result);
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			FormsTextBox child = Control;

			if (Children.Count == 0 || child == null)
				return new SizeRequest();

			return CalculateDesiredSizes(child, new Windows.Foundation.Size(widthConstraint, heightConstraint), Element.AutoSize);
		}
	}
}
