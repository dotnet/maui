using System.Collections.Generic;

namespace System.Maui
{
	public interface ILayout : IView
	{
		IList<IView> Children { get; }
	}

	public interface IStackLayout : ILayout
	{
		Orientation Orientation { get; }
	}

	public enum Alignment
	{
		Start, 
		Center, 
		End, 
		Fill
	}

	public enum Orientation
	{ 
		Vertical,
		Horizontal
	}
}
