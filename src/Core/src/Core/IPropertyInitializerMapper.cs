namespace Microsoft.Maui;

// TODO: Potentially make public in NET10
internal interface IPropertyInitializerMapper
{
	void InitializeProperties(IElementHandler viewHandler, IElement virtualView);
}