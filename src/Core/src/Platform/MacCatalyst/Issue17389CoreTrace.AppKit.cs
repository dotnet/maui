#if MACCATALYST
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform
{
	internal static partial class Issue17389CoreTrace
	{
		const ulong LeftMouseMask = (1UL << 1) | (1UL << 2);
		const string EventBlockSignature = "@\"NSEvent\"16@?0@\"NSEvent\"8";
		const string RectReturnEncoding = "{CGRect={CGPoint=dd}{CGSize=dd}}";
		const int MaxNativeClassDepth = 32;
		const int NativeTypeBufferLength = 64;
		static nint _nsEventClass, _nsWindowClass, _monitorToken;
		static nint _addMonitor, _removeMonitor, _eventKind, _eventTimestamp;
		static nint _eventWindowNumber, _eventLocation, _eventWindow;
		static nint _eventClickCount, _eventButtonNumber, _eventModifierFlags, _windowIsKey;
		static nint _windowFrame, _windowContentLayout;

		static partial void StartAppKitMonitor(long generation)
		{
			try
			{
				if (_monitorToken != 0)
				{
					_appKitInstall = NativeInstall.PreviousMonitorNotRemoved;
					AppKitFailed(NativeFault.SetupFailed);
					return;
				}
				if (!NSThread.IsMain)
				{
					_appKitInstall = NativeInstall.NotMainThread;
					AppKitFailed(NativeFault.SetupFailed);
					return;
				}
				if (IntPtr.Size != 8 || RuntimeInformation.ProcessArchitecture is not (Architecture.X64 or Architecture.Arm64))
				{
					_appKitInstall = NativeInstall.UnsupportedArchitecture;
					AppKitFailed(NativeFault.SetupFailed);
					return;
				}
				_nsEventClass = (nint)Class.GetHandle("NSEvent");
				_nsWindowClass = (nint)Class.GetHandle("NSWindow");
				if (_nsEventClass == 0 || _nsWindowClass == 0)
				{
					_appKitInstall = NativeInstall.ClassUnavailable;
					AppKitFailed(NativeFault.SetupFailed);
					return;
				}
				_addMonitor = (nint)Selector.GetHandle("addLocalMonitorForEventsMatchingMask:handler:");
				_removeMonitor = (nint)Selector.GetHandle("removeMonitor:");
				_eventKind = (nint)Selector.GetHandle("type");
				_eventTimestamp = (nint)Selector.GetHandle("timestamp");
				_eventWindowNumber = (nint)Selector.GetHandle("windowNumber");
				_eventLocation = (nint)Selector.GetHandle("locationInWindow");
				_eventWindow = (nint)Selector.GetHandle("window");
				_eventClickCount = (nint)Selector.GetHandle("clickCount");
				_eventButtonNumber = (nint)Selector.GetHandle("buttonNumber");
				_eventModifierFlags = (nint)Selector.GetHandle("modifierFlags");
				_windowIsKey = (nint)Selector.GetHandle("isKeyWindow");
				_windowFrame = (nint)Selector.GetHandle("frame");
				_windowContentLayout = (nint)Selector.GetHandle("contentLayoutRect");
				if (!Signature(class_getClassMethod(_nsEventClass, _addMonitor), "@", "@", ":", "Q", "@?") ||
					!Signature(class_getClassMethod(_nsEventClass, _removeMonitor), "v", "@", ":", "@") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventKind), "Q", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventTimestamp), "d", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventWindowNumber), "q", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventLocation), "{CGPoint=dd}", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventWindow), "@", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventClickCount), "q", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventButtonNumber), "q", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsEventClass, _eventModifierFlags), "Q", "@", ":") ||
					!Signature(class_getInstanceMethod(_nsWindowClass, _windowFrame), RectReturnEncoding, "@", ":") ||
					!Signature(class_getInstanceMethod(_nsWindowClass, _windowContentLayout), RectReturnEncoding, "@", ":") ||
					(!Signature(class_getInstanceMethod(_nsWindowClass, _windowIsKey), "c", "@", ":") &&
					 !Signature(class_getInstanceMethod(_nsWindowClass, _windowIsKey), "B", "@", ":")))
				{
					_appKitInstall = NativeInstall.SignatureMismatch;
					AppKitFailed(NativeFault.SetupFailed);
					return;
				}
				InstallMonitor(generation);
			}
			catch
			{
				_appKitInstall = NativeInstall.ManagedException;
				AppKitFailed(NativeFault.SetupFailed);
			}
		}

		static unsafe void InstallMonitor(long generation)
		{
			delegate* unmanaged[Cdecl]<nint, nint, nint> callback = &MonitorCallback;
			// Clang's LP64 block encoding: NSEvent return, block at 0, NSEvent argument at 8.
			Action<nint> observer = new NativeScope(generation).Observe;
			using var block = new BlockLiteral(callback, observer, EventBlockSignature);
			_monitorToken = AddLocalMonitor(_nsEventClass, _addMonitor, LeftMouseMask, &block);
			_appKitInstall = _monitorToken == 0 ? NativeInstall.NullMonitorToken : NativeInstall.Installed;
			if (_monitorToken == 0)
				AppKitFailed(NativeFault.SetupFailed);
		}

		static partial void StopAppKitMonitor()
		{
			if (_monitorToken == 0)
				return;
			try
			{
				if (!NSThread.IsMain)
				{
					_appKitRemoval = NativeRemoval.NotMainThread;
					AppKitFailed(NativeFault.RemovalFailed);
					return;
				}
				RemoveLocalMonitor(_nsEventClass, _removeMonitor, _monitorToken);
				// AppKit owns the opaque token while installed; do not release it again.
				_monitorToken = 0;
				_appKitRemoval = NativeRemoval.Removed;
			}
			catch
			{
				_appKitRemoval = NativeRemoval.ManagedException;
				AppKitFailed(NativeFault.RemovalFailed);
			}
		}

		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
		static nint MonitorCallback(nint block, nint suppliedEvent)
		{
			try
			{
				var observer = block == 0 ? null : BlockLiteral.GetTarget<Action<nint>>(block);
				if (observer is null)
					AppKitFailed(NativeFault.MissingBlockContext);
				else
					observer(suppliedEvent);
			}
			catch
			{
				AppKitFailed(NativeFault.ManagedCallbackException);
			}
			return suppliedEvent;
		}

		static void ObserveAppKitEvent(long generation, nint suppliedEvent)
		{
			if (Volatile.Read(ref _appKitFault) != 0 || !ReserveAppKitEvent(generation))
				return;
			var entry = new Entry
			{
				Stage = Stage.AppKitLocalEventBeforeSendEvent,
				EventHandle = (long)suppliedEvent,
				Boundary = new BoundaryFields { Version = 1, Samples = (int)NativeSample.Argument }
			};
			try
			{
				if (suppliedEvent == 0)
					entry.Boundary.Fault = (int)NativeFault.NullEventArgument;
				else if (!IsNativeObject(suppliedEvent, _nsEventClass))
					entry.Boundary.Fault = (int)NativeFault.UnexpectedEventClass;
				else
				{
					entry.Boundary.RawEventKind = (ulong)ReadUnsigned(suppliedEvent, _eventKind);
					entry.Boundary.Samples |= (int)NativeSample.Kind;
					if (entry.Boundary.RawEventKind is not (1 or 2))
						entry.Boundary.Fault = (int)NativeFault.UnexpectedEventKind;
					else
					{
						entry.EventTime = ReadDouble(suppliedEvent, _eventTimestamp);
						entry.Boundary.Samples |= (int)NativeSample.Timestamp;
						entry.Boundary.NativeWindowNumber = (long)ReadSigned(suppliedEvent, _eventWindowNumber);
						entry.Boundary.Samples |= (int)NativeSample.WindowNumber;
						var point = ReadPoint(suppliedEvent, _eventLocation);
						entry.X = point.X;
						entry.Y = point.Y;
						entry.Boundary.Samples |= (int)NativeSample.Location;
						entry.WindowHandle = (long)ReadObject(suppliedEvent, _eventWindow);
						entry.Boundary.Samples |= (int)NativeSample.Window;
						entry.Boundary.NativeClickCount = (long)ReadSigned(suppliedEvent, _eventClickCount);
						entry.Boundary.Samples |= (int)NativeSample.ClickCount;
						entry.Boundary.NativeButtonNumber = (long)ReadSigned(suppliedEvent, _eventButtonNumber);
						entry.Boundary.Samples |= (int)NativeSample.ButtonNumber;
						entry.Boundary.NativeModifierFlags = (ulong)ReadUnsigned(suppliedEvent, _eventModifierFlags);
						entry.Boundary.Samples |= (int)NativeSample.ModifierFlags;
						if (entry.WindowHandle != 0)
						{
							if (!IsNativeObject((nint)entry.WindowHandle, _nsWindowClass))
								entry.Boundary.Fault = (int)NativeFault.UnexpectedWindowClass;
							else
							{
								entry.Boundary.NativeWindowKeyRaw = ReadByte((nint)entry.WindowHandle, _windowIsKey);
								entry.Boundary.Samples |= (int)NativeSample.WindowKey;
								var frame = ReadRect((nint)entry.WindowHandle, _windowFrame);
								entry.Boundary.FrameX = frame.X;
								entry.Boundary.FrameY = frame.Y;
								entry.Boundary.FrameWidth = frame.Width;
								entry.Boundary.FrameHeight = frame.Height;
								entry.Boundary.Samples |= (int)NativeSample.WindowFrame;
								var content = ReadRect((nint)entry.WindowHandle, _windowContentLayout);
								entry.Boundary.ContentX = content.X;
								entry.Boundary.ContentY = content.Y;
								entry.Boundary.ContentWidth = content.Width;
								entry.Boundary.ContentHeight = content.Height;
								entry.Boundary.Samples |= (int)NativeSample.ContentLayoutRect;
							}
						}
					}
				}
			}
			catch
			{
				entry.Boundary.Fault = (int)NativeFault.ManagedSampleException;
			}
			if (entry.Boundary.Fault != 0)
				AppKitFailed((NativeFault)entry.Boundary.Fault);
			Publish(generation, entry);
		}

		static bool IsNativeObject(nint value, nint expectedClass)
		{
			var type = object_getClass(value);
			for (int depth = 0; type != 0 && depth < MaxNativeClassDepth; depth++)
			{
				if (type == expectedClass)
					return true;
				type = class_getSuperclass(type);
			}
			return false;
		}

		static unsafe bool Signature(nint method, string result, params string[] arguments)
		{
			if (method == 0 || method_getNumberOfArguments(method) != arguments.Length)
				return false;
			byte* buffer = stackalloc byte[NativeTypeBufferLength];
			new Span<byte>(buffer, NativeTypeBufferLength).Fill(255);
			method_getReturnType(method, buffer, NativeTypeBufferLength);
			if (!TypeEquals(buffer, result))
				return false;
			for (uint i = 0; i < arguments.Length; i++)
			{
				new Span<byte>(buffer, NativeTypeBufferLength).Fill(255);
				method_getArgumentType(method, i, buffer, NativeTypeBufferLength);
				if (!TypeEquals(buffer, arguments[i]))
					return false;
			}
			return true;
		}

		static unsafe bool TypeEquals(byte* value, string expected)
		{
			if (expected.Length >= NativeTypeBufferLength)
				return false;
			for (int i = 0; i < expected.Length; i++)
				if (value[i] != expected[i])
					return false;
			return value[expected.Length] == 0;
		}

		sealed class NativeScope
		{
			internal readonly long Generation;
			internal NativeScope(long generation) => Generation = generation;
			internal void Observe(nint suppliedEvent) => ObserveAppKitEvent(Generation, suppliedEvent);
		}

		[StructLayout(LayoutKind.Sequential)]
		struct NativePoint
		{
			internal double X, Y;

			public NativePoint() => X = Y = 0;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct NativeRect
		{
			internal double X, Y, Width, Height;

			public NativeRect() => X = Y = Width = Height = 0;
		}

		static NativeRect ReadRect(nint receiver, nint selector)
		{
			switch (RuntimeInformation.ProcessArchitecture)
			{
				case Architecture.X64:
					ReadRectX64(out var result, receiver, selector);
					return result;
				case Architecture.Arm64:
					return ReadRectArm64(receiver, selector);
				default:
					throw new PlatformNotSupportedException("Only LP64 X64 and Arm64 rectangle getters are supported.");
			}
		}

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend_stret", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
		static extern void ReadRectX64(out NativeRect result, nint receiver, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
		static extern NativeRect ReadRectArm64(nint receiver, nint selector);

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe nint AddLocalMonitor(nint receiver, nint selector, ulong mask, BlockLiteral* block);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern void RemoveLocalMonitor(nint receiver, nint selector, nint monitor);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern nuint ReadUnsigned(nint receiver, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern nint ReadSigned(nint receiver, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern double ReadDouble(nint receiver, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern NativePoint ReadPoint(nint receiver, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern nint ReadObject(nint receiver, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		static extern byte ReadByte(nint receiver, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern nint class_getClassMethod(nint type, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern nint class_getInstanceMethod(nint type, nint selector);
		[DllImport(Constants.ObjectiveCLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern nint object_getClass(nint value);
		[DllImport(Constants.ObjectiveCLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern nint class_getSuperclass(nint type);
		[DllImport(Constants.ObjectiveCLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern uint method_getNumberOfArguments(nint method);
		[DllImport(Constants.ObjectiveCLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void method_getReturnType(nint method, byte* destination, nuint length);
		[DllImport(Constants.ObjectiveCLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern unsafe void method_getArgumentType(nint method, uint index, byte* destination, nuint length);
	}
}
#endif
