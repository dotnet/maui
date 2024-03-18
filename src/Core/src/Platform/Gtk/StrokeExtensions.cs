namespace Microsoft.Maui.Platform
{

	public static class StrokeExtensions
	{

		[MissingMapper]
		public static void UpdateStroke(this BorderView platformView, IBorderStroke border)
		{ }

		[MissingMapper]
		public static void UpdateStrokeShape(this Gtk.Widget platformView, IBorderStroke border) { }

		[MissingMapper]
		public static void UpdateStroke(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeThickness(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeDashPattern(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeDashOffset(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeMiterLimit(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeLineCap(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeLineJoin(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

	}

}