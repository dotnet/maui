namespace System.Maui.Core
{
	public interface IScroll
	{
		ScrollOrientation Orientation { get; }
		Size ContentSize { get; }
		IView Content { get; }
		double ScrollX { get; set; }
		double ScrollY { get; set; }
		bool HorizontalScrollBarVisible { get; }
		bool VerticalScrollBarVisible { get; }

		void Scrolled();
	}

	public enum ScrollOrientation
	{
		Vertical,
		Horizontal,
		Both,
		Neither
	}
}
