using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="View"/> that presents local HTML content in a web view and allows JavaScript and C# code to interop using messages.
	/// </summary>
	public class HybridWebView : View, IHybridWebView
	{
		/// <summary>Bindable property for <see cref="DefaultFile"/>.</summary>
		public static readonly BindableProperty DefaultFileProperty =
			BindableProperty.Create(nameof(DefaultFile), typeof(string), typeof(HybridWebView), defaultValue: "index.html");
		/// <summary>Bindable property for <see cref="HybridRoot"/>.</summary>
		public static readonly BindableProperty HybridRootProperty =
			BindableProperty.Create(nameof(HybridRoot), typeof(string), typeof(HybridWebView), defaultValue: "wwwroot");


		/// <summary>
		/// Specifies the file within the <see cref="HybridRoot"/> that should be served as the default file. The
		/// default value is <c>index.html</c>.
		/// </summary>
		public string? DefaultFile
		{
			get { return (string)GetValue(DefaultFileProperty); }
			set { SetValue(DefaultFileProperty, value); }
		}

		/// <summary>
		///  The path within the app's "Raw" asset resources that contain the web app's contents. For example, if the
		///  files are located in <c>[ProjectFolder]/Resources/Raw/hybrid_root</c>, then set this property to "hybrid_root".
		///  The default value is <c>wwwroot</c>, which maps to <c>[ProjectFolder]/Resources/Raw/wwwroot</c>.
		/// </summary>
		public string? HybridRoot
		{
			get { return (string)GetValue(HybridRootProperty); }
			set { SetValue(HybridRootProperty, value); }
		}

		void IHybridWebView.RawMessageReceived(string rawMessage)
		{
			RawMessageReceived?.Invoke(this, new HybridWebViewRawMessageReceivedEventArgs(rawMessage));
		}

		/// <summary>
		/// Raised when a raw message is received from the web view. Raw messages are strings that have no additional processing.
		/// </summary>
		public event EventHandler<HybridWebViewRawMessageReceivedEventArgs>? RawMessageReceived;

		public void SendRawMessage(string rawMessage)
		{
			Handler?.Invoke(nameof(IHybridWebView.SendRawMessage), rawMessage);
		}
	}
}
