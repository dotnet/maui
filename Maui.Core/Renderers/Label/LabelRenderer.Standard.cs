using System;
namespace System.Maui.Platform
{
	public partial class LabelRenderer : AbstractViewRenderer<ILabel, object> 
	{
		public static void MapPropertyText(IViewRenderer renderer, IText view) { }
		public static void MapPropertyColor(IViewRenderer renderer, IText view) { }
		public static void MapPropertyLineHeight(IViewRenderer renderer, ILabel view) { }
		protected override object CreateView () => throw new NotImplementedException ();
	}
}
