using System.Windows.Controls;
using System.Windows.Media;

namespace System.Maui.Platform
{
	public partial class LabelRenderer : AbstractViewRenderer<ILabel, TextBlock>
	{
		protected override TextBlock CreateView() => new TextBlock();

		public static void MapPropertyText(IViewRenderer renderer, IText view) => (renderer as LabelRenderer)?.UpdateText();
		public static void MapPropertyColor(IViewRenderer renderer, IText view) => (renderer as LabelRenderer)?.UpdateColor();
		public static void MapPropertyLineHeight(IViewRenderer renderer, ILabel view) { }

		public virtual void UpdateText()
		{
			TypedNativeView.Text = VirtualView.Text;	
		}

		public virtual void UpdateColor()
		{
			var textColor = VirtualView.Color;
			TypedNativeView.Foreground = !textColor.IsDefault ? textColor.ToBrush() : Brushes.Black;
		}
	}
}
