namespace Microsoft.Maui.Controls.Xaml;

internal static class NodeExtensions
{
	public static bool TryGetPropertyName(this INode node, INode parentNode, out XmlName name)
	{
		name = default;

		if (parentNode is not ElementNode parentElement)
		{
			return false;
		}

		foreach (var kvp in parentElement.Properties)
		{
			if (kvp.Value != node)
			{
				continue;
			}

			name = kvp.Key;
			return true;
		}

		return false;
	}
}
