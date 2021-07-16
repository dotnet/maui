namespace Microsoft.Maui.Semantics
{
	interface ISemanticService
	{
		/// <summary>
		/// Tell screen reader to read out the text specified
		/// </summary>
		/// <param name="text"></param>
		void Announce(string text);
	}
}
