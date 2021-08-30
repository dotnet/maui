using ElmSharp;
using Microsoft.Maui.Graphics.Skia.Views;
using SkiaGraphicsView = Microsoft.Maui.Platform.Tizen.SkiaGraphicsView;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : SkiaGraphicsView
	{
		public MauiShapeView(EvasObject parent) : base(parent)
		{
		}
	}
}