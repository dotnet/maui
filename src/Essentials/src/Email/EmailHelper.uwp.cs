#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	static class EmailHelper
	{
		public static Task<bool> ShowComposeNewEmailAsync(PlatformEmailMessage message) =>
			Task.Run(() => SendMail(message) == 0);

		static int SendMail(PlatformEmailMessage message)
		{
			var flags = SendMailFlags.MAPI_LOGON_UI | SendMailFlags.MAPI_DIALOG_MODELESS | SendMailFlags.MAPI_DIALOG;

			var recipients = GetRecipients(message);
			var attachments = GetAttachments(message);

			var msg = new MapiMessage
			{
				subject = message.Subject,
				noteText = message.Body,
				recipCount = recipients?.Length ?? 0,
				recips = GetUnmanagedArray(recipients),
				fileCount = attachments?.Length ?? 0,
				files = GetUnmanagedArray(attachments),
			};

			try
			{
				return MAPISendMail(IntPtr.Zero, IntPtr.Zero, ref msg, flags, 0);
			}
			finally
			{
				DestroyUnmanagedArray(msg.recips, recipients);
				DestroyUnmanagedArray(msg.files, attachments);
			}
		}

		static IntPtr GetUnmanagedArray<TStruct>(TStruct[]? array)
		{
			if (array?.Length > 0)
			{
				var size = Marshal.SizeOf(typeof(TStruct));

				var intptr = Marshal.AllocHGlobal(array.Length * size);

				var ptr = intptr;
				foreach (var item in array)
				{
					Marshal.StructureToPtr(item!, ptr, false);
					ptr += size;
				}

				return intptr;
			}

			return IntPtr.Zero;
		}

		static void DestroyUnmanagedArray<TStruct>(IntPtr intptr, TStruct[]? array)
		{
			var count = array?.Length;
			if (count > 0)
			{
				var size = Marshal.SizeOf(typeof(TStruct));

				var ptr = intptr;
				for (var i = 0; i < count; i++)
				{
					Marshal.DestroyStructure<TStruct>(ptr);
					ptr += size;
				}

				Marshal.FreeHGlobal(intptr);
			}
		}

		static MapiRecipDesc[]? GetRecipients(PlatformEmailMessage message)
		{
			var recipCount = message.To.Count + message.CC.Count + message.Bcc.Count;

			if (recipCount == 0)
				return null;

			var recipients = new MapiRecipDesc[recipCount];

			var idx = 0;
			foreach (var to in message.To)
				recipients[idx++] = Create(to, RecipientClass.MAPI_TO);
			foreach (var cc in message.CC)
				recipients[idx++] = Create(cc, RecipientClass.MAPI_CC);
			foreach (var bcc in message.Bcc)
				recipients[idx++] = Create(bcc, RecipientClass.MAPI_BCC);

			return recipients;

			static MapiRecipDesc Create(PlatformEmailRecipient recipient, RecipientClass type)
			{
				return new MapiRecipDesc
				{
					name = recipient.Address,
					recipClass = type
				};
			}
		}

		static MapiFileDesc[]? GetAttachments(PlatformEmailMessage message)
		{
			var attachCount = message.Attachments.Count;

			if (attachCount == 0)
				return null;

			var attachments = new MapiFileDesc[attachCount];

			var idx = 0;
			foreach (var file in message.Attachments)
				attachments[idx++] = Create(file);

			return attachments;

			static MapiFileDesc Create(string filename)
			{
				return new MapiFileDesc
				{
					name = Path.GetFileName(filename),
					path = filename,
					position = -1
				};
			}
		}

		[DllImport("MAPI32.DLL")]
		static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, ref MapiMessage message, SendMailFlags flg, int rsv);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		struct MapiMessage
		{
			private int reserved;
			public string? subject;
			public string? noteText;
			public string? messageType;
			public string? dateReceived;
			public string? conversationID;
			public int flags;
			public IntPtr originator;
			public int recipCount;
			public IntPtr recips;
			public int fileCount;
			public IntPtr files;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		struct MapiFileDesc
		{
			private int reserved;
			public int flags;
			public int position;
			public string? path;
			public string? name;
			public IntPtr type;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		struct MapiRecipDesc
		{
			private int reserved;
			public RecipientClass recipClass;
			public string? name;
			public string? address;
			public int eIDSize;
			public IntPtr entryID;
		}

		enum RecipientClass
		{
			MAPI_ORIG = 0,
			MAPI_TO,
			MAPI_CC,
			MAPI_BCC
		};

		[Flags]
		enum SendMailFlags
		{
			MAPI_LOGON_UI = 0x00000001,
			MAPI_DIALOG_MODELESS = 0x00000004,
			MAPI_DIALOG = 0x00000008,
		}
	}

	class PlatformEmailMessage
	{
		public string? Body { get; set; }

		public string? Subject { get; set; }

		public List<PlatformEmailRecipient> To { get; } = new();

		public List<PlatformEmailRecipient> CC { get; } = new();

		public List<PlatformEmailRecipient> Bcc { get; } = new();

		public List<string> Attachments { get; } = new();
	}

	class PlatformEmailRecipient
	{
		public PlatformEmailRecipient(string address)
		{
			Address = address;
		}

		public string Address { get; set; }
	}
}