namespace Microsoft.Maui.Maps
{
	public interface IMapElement : IElement, IStroke
	{
		object? MapElementId { get; set; }
	}
}
