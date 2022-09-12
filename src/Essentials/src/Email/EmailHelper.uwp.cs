using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace Microsoft.Maui.ApplicationModel.Communication
{   
	// TODO Make public for NET7
	internal class EmailHelper
	{
		const int MAPI_LOGON_UI = 0x00000001;
		const int MAPI_DIALOG = 0x00000008;
		const int MaxAttachments = 200;

		readonly string[] _errors;
		int _lastError;

		readonly List<MapiRecipDesc> _recipients;
		readonly List<string> _attachments;

		enum HowTo
		{
			MAPI_ORIG = 0,
			MAPI_TO,
			MAPI_CC,  // Not supported
			MAPI_BCC  // Not supported 
		};

		public EmailHelper()
		{
			_errors = new string[]
			{
				"OK [0]",
				"User abort [1]",
				"General MAPI failure [2]",
				"MAPI login failure [3]",
				"Disk full [4]",
				"Insufficient memory [5]",
				"Access denied [6]",
				"-unknown- [7]",
				"Too many sessions [8]",
				"Too many files were specified [9]",
				"Too many recipients were specified [10]",
				"A specified attachment was not found [11]",
				"Attachment open failure [12]",
				"Attachment write failure [13]",
				"Unknown recipient [14]",
				"Bad recipient type [15]",
				"No messages [16]",
				"Invalid message [17]",
				"Text too large [18]",
				"Invalid session [19]",
				"Type not supported [20]",
				"A recipient was specified ambiguously [21]",
				"Message in use [22]",
				"Network failure [23]",
				"Invalid edit fields [24]",
				"Invalid recipients [25]",
				"Not supported [26]"
			};

			_recipients = new List<MapiRecipDesc>();
			_attachments = new List<string>();
			_lastError = 0;

		}

		public bool AddRecipient(string email, string name)
		{
			MapiRecipDesc recipient;

			recipient = new MapiRecipDesc
			{
				recipClass = (int)HowTo.MAPI_TO,
				address = email,
				name = name
			};

			_recipients.Add(recipient);
			return true;
		}

		public bool AddRecipient(MapiRecipDesc recipient)
		{
			_recipients.Add(recipient);
			return true;
		}

		public void AddAttachment(string strAttachmentFileName)
		{
			_attachments.Add(strAttachmentFileName);
		}

		[DllImport("MAPI32.DLL")]
		static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MapiMessage message, int flg, int rsv);

		[DllImport("MAPI32.DLL")]
		static extern int MAPIResolveName(IntPtr sess, IntPtr hwnd, string name, int flg, int rsv, ref MapiRecipDesc recipient);

		public bool SendMail(string strSubject, string strBody)
		{
			MapiMessage msg;

			msg = new MapiMessage
			{
				subject = strSubject,
				noteText = strBody
			};

			msg.recips = GetRecipients(out msg.recipCount);
			msg.files = GetAttachments(out msg.fileCount);

			_lastError = MAPISendMail(new IntPtr(0), new IntPtr(0), msg, MAPI_LOGON_UI | MAPI_DIALOG, 0);

			if (_lastError > 1)
			{
				Cleanup(ref msg);
				return false;
			}

			Cleanup(ref msg);
			return true;
		}

		public bool ResolveName(string name, ref MapiRecipDesc recipient)
		{
			_lastError = MAPIResolveName(new IntPtr(0), new IntPtr(0), name, 0, 0, ref recipient);

			if (_lastError > 1)
			{
				return false;
			}

			return true;
		}

		IntPtr GetRecipients(out int recipCount)
		{
			int size;
			IntPtr intPtr;
			IntPtr ptr;

			if (_recipients.Count == 0)
			{
				recipCount = 0;
				return IntPtr.Zero;
			}

			size = Marshal.SizeOf(typeof(MapiRecipDesc));
			intPtr = Marshal.AllocHGlobal(_recipients.Count * size);

			ptr = intPtr;

			foreach (MapiRecipDesc mapiDesc in _recipients)
			{
				Marshal.StructureToPtr(mapiDesc, (IntPtr)ptr, false);
				ptr += size;
			}

			recipCount = _recipients.Count;
			return intPtr;
		}

		IntPtr GetAttachments(out int fileCount)
		{
			MapiFileDesc mapiFileDesc;
			int size;
			IntPtr intPtr;
			IntPtr ptr;

			if (_attachments == null)
			{
				fileCount = 0;
				return IntPtr.Zero;
			}

			if ((_attachments.Count <= 0) || (_attachments.Count > MaxAttachments))
			{
				fileCount = 0;
				return IntPtr.Zero;
			}

			size = Marshal.SizeOf(typeof(MapiFileDesc));
			intPtr = Marshal.AllocHGlobal(_attachments.Count * size);

			mapiFileDesc = new MapiFileDesc
			{
				position = -1
			};

			ptr = intPtr;

			foreach (string strAttachment in _attachments)
			{
				mapiFileDesc.name = Path.GetFileName(strAttachment);
				mapiFileDesc.path = strAttachment;
				Marshal.StructureToPtr(mapiFileDesc, (IntPtr)ptr, false);
				ptr += size;
			}

			fileCount = _attachments.Count;

			return intPtr;
		}

		void Cleanup(ref MapiMessage msg)
		{
			int size;
			IntPtr ptr;
			int i;

			if (msg.recips != IntPtr.Zero)
			{
				size = Marshal.SizeOf(typeof(MapiRecipDesc));
				ptr = msg.recips;

				for (i = 0; i < msg.recipCount; i++)
				{
					Marshal.DestroyStructure((IntPtr)ptr, typeof(MapiRecipDesc));
					ptr += size;
				}
				Marshal.FreeHGlobal(msg.recips);
			}

			if (msg.files != IntPtr.Zero)
			{
				size = Marshal.SizeOf(typeof(MapiFileDesc));
				ptr = msg.files;

				for (i = 0; i < msg.fileCount; i++)
				{
					Marshal.DestroyStructure((IntPtr)ptr, typeof(MapiFileDesc));
					ptr += size;
				}
				Marshal.FreeHGlobal(msg.files);
			}

			_recipients.Clear();
			_attachments.Clear();
			_lastError = 0;
		}

		public string GetLastError()
		{
			if (_lastError <= 26)
				return _errors[_lastError];

			return "EmailHelper error [" + _lastError.ToString() + "]";
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal class MapiMessage
	{
		public int reserved;
		public string subject;
		public string noteText;
		public string messageType;
		public string dateReceived;
		public string conversationID;
		public int flags;
		public IntPtr originator;
		public int recipCount;
		public IntPtr recips;
		public int fileCount;
		public IntPtr files;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal class MapiFileDesc
	{
		public int reserved;
		public int flags;
		public int position;
		public string path;
		public string name;
		public IntPtr type;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal class MapiRecipDesc
	{
		public int reserved;
		public int recipClass;
		public string name;
		public string address;
		public int eIDSize;
		public IntPtr entryID;
	}
}