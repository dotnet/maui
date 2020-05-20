namespace System.Maui
{
	public interface ISwitch : IView
	{
		bool IsOn { get; set; }
		Color OnColor { get; }
		Color ThumbColor { get; }
	}
}
