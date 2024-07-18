namespace Microsoft.Maui
{
	public interface IHybridWebView : IView
	{
		string? DefaultFile { get; }

		/// <summary>
		///  The path within the app's "Raw" asset resources that contain the web app's contents. For example, if the
		///  files are located in <c>[ProjectFolder]/Resources/Raw/hybrid_root</c>, then set this property to "hybrid_root".
		///  The default value is <c>HybridRoot</c>, which maps to <c>[ProjectFolder]/Resources/Raw/HybridRoot</c>.
		/// </summary>
		string? HybridRoot { get; }

		void RawMessageReceived(string rawMessage);

		void SendRawMessage(string rawMessage);
	}
}
