namespace Xamarin.Forms.Xaml
{
	public interface IProvideValueTarget
	{
		object TargetObject { get; }
		object TargetProperty { get; }
	}
}