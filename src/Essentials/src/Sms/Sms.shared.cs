#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <summary>
	/// The SMS API enables an application to open the default SMS application with a specified message to send to a recipient.
	/// </summary>
	public interface ISms
	{
		/// <summary>
		/// Gets a value indicating whether composing of SMS messages is supported on this device.
		/// </summary>
		bool IsComposeSupported { get; }

		/// <summary>
		/// Opens the default SMS client to allow the user to send the message.
		/// </summary>
		/// <param name="message">A <see cref="SmsMessage"/> instance with information about the message to send.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task ComposeAsync(SmsMessage? message);
	}

	/// <summary>
	/// The SMS API enables an application to open the default SMS application with a specified message to send to a recipient.
	/// </summary>
	/// <remarks>When using this on Android targeting Android 11 (R API 30) you must update your Android Manifest with queries that are used with the new package visibility requirements. See the conceptual docs for more information.</remarks>
	public static class Sms
	{
		/// <summary>
		/// Opens the default SMS client to allow the user to send the message.
		/// </summary>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task ComposeAsync()
			=> Current.ComposeAsync(null);

		/// <summary>
		/// Opens the default SMS client to allow the user to send the message.
		/// </summary>
		/// <param name="message">A <see cref="SmsMessage"/> instance with information about the message to send.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task ComposeAsync(SmsMessage? message)
			=> Current.ComposeAsync(message);

		static ISms Current => ApplicationModel.Communication.Sms.Default;

		static ISms? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static ISms Default =>
			defaultImplementation ??= new SmsImplementation();

		internal static void SetDefault(ISms? implementation) =>
			defaultImplementation = implementation;
	}

	partial class SmsImplementation : ISms
	{
		public Task ComposeAsync() =>
			ComposeAsync(null);

		public Task ComposeAsync(SmsMessage? message)
		{
			if (!IsComposeSupported)
				throw new FeatureNotSupportedException();

			message ??= new SmsMessage();

			message.Recipients ??= new List<string>();

			return PlatformComposeAsync(message);
		}
	}

	/// <summary>
	/// Represents a single SMS message.
	/// </summary>
	public class SmsMessage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SmsMessage"/> class.
		/// </summary>
		public SmsMessage()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SmsMessage"/> class.
		/// </summary>
		/// <param name="body">The body text that is used to prefill the composed SMS message.</param>
		/// <param name="recipient">A single recipient that is added to the composed SMS message.</param>
		public SmsMessage(string body, string? recipient)
		{
			Body = body;
			if (!string.IsNullOrWhiteSpace(recipient))
				Recipients.Add(recipient!);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SmsMessage"/> class.
		/// </summary>
		/// <param name="body">The body text that is used to prefill the composed SMS message.</param>
		/// <param name="recipients">A collection of recipients that are added to the composed SMS message.</param>
		/// <remarks>Values in <paramref name="recipients"/> that are <see langword="null"/> or whitespace are not added as recipients.</remarks>
		public SmsMessage(string body, IEnumerable<string>? recipients)
		{
			Body = body;
			if (recipients != null)
			{
				Recipients.AddRange(recipients.Where(x => !string.IsNullOrWhiteSpace(x)));
			}
		}

		/// <summary>
		/// Gets or sets the body of this message.
		/// </summary>
		public string? Body { get; set; }

		/// <summary>
		/// Gets or sets the recipients for this message.
		/// </summary>
		public List<string> Recipients { get; set; } = new List<string>();
	}
}
