namespace System.Maui.Xaml
{
	public interface IProvideValueTarget
	{
		object TargetObject { get; }
		object TargetProperty { get; }
	}
}