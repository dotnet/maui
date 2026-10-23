using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.Maui.Platform
{
	internal static partial class Issue17389CoreTrace
	{
		[Flags]
		internal enum ApplicationSample
		{
			None = 0,
			Argument = 1,
			Type = 2,
			Timestamp = 4,
			TouchSet = 8,
			TouchCount = 16,
			TouchArgument = 32,
			TouchPhase = 64,
			TouchTimestamp = 128,
			TouchLocation = 256,
			Target = 512,
			TargetKind = 1024,
			Window = 2048,
			WindowKey = 4096,
			ApplicationState = 8192
		}

		[Flags]
		enum NativeSample
		{
			None = 0, Argument = 1, Kind = 2, Timestamp = 4,
			WindowNumber = 8, Location = 16, Window = 32,
			ClickCount = 64, ButtonNumber = 128, ModifierFlags = 256, WindowKey = 512
		}

		enum NativeInstall
		{
			NotAttempted, Installed, ClassUnavailable, SignatureMismatch,
			UnsupportedArchitecture, NotMainThread, NullMonitorToken,
			ManagedException, PreviousMonitorNotRemoved
		}

		enum NativeRemoval { NotRequired, Removed, NotMainThread, ManagedException }
		enum NativeFault
		{
			None, NullEventArgument, UnexpectedEventClass, UnexpectedEventKind,
			ManagedSampleException, MissingBlockContext, ManagedCallbackException,
			SetupFailed, RemovalFailed, UnexpectedWindowClass
		}

		const int MaxAppKitEvents = 16;
		static int _appKitEventCount, _appKitBudgetSkips, _appKitBufferSkips;
		static int _appKitFault, _applicationObserverFault;
		static NativeInstall _appKitInstall;
		static NativeRemoval _appKitRemoval;

		static partial void StartAppKitMonitor(long generation);
		static partial void StopAppKitMonitor();

		static void ResetBoundaryScope()
		{
			_appKitEventCount = _appKitBudgetSkips = _appKitBufferSkips = 0;
			_appKitFault = _applicationObserverFault = 0;
			_appKitInstall = NativeInstall.NotAttempted;
			_appKitRemoval = NativeRemoval.NotRequired;
		}

		static bool ReserveAppKitEvent(long generation)
		{
			lock (Sync)
			{
				if (!_recording || generation != _generation)
					return false;
				if (_appKitEventCount == MaxAppKitEvents)
				{
					if (_appKitBudgetSkips != int.MaxValue)
						_appKitBudgetSkips++;
					return false;
				}
				if (_count == Capacity)
				{
					if (_appKitBufferSkips != int.MaxValue)
						_appKitBufferSkips++;
					return false;
				}
				_appKitEventCount++;
				return true;
			}
		}

		internal static void ApplicationObserverFailed() =>
			Interlocked.CompareExchange(ref _applicationObserverFault, 1, 0);

		static void AppKitFailed(NativeFault fault) =>
			Interlocked.CompareExchange(ref _appKitFault, (int)fault, 0);

		internal static void RecordApplicationBoundaryEvent(long generation, int sampledFlags, int fault,
			long eventType, long eventHandle, double eventTime, bool touchSetPresent, int touchCount,
			long touchHandle, int touchPhase, double touchTime, double x, double y, long targetHandle,
			int targetManagedKind, long windowHandle, bool windowIsKey, int applicationState)
		{
			var samples = (ApplicationSample)sampledFlags;
			const ApplicationSample touchFields = ApplicationSample.TouchArgument | ApplicationSample.TouchPhase |
				ApplicationSample.TouchTimestamp | ApplicationSample.TouchLocation;
			Publish(generation, new Entry
			{
				Stage = Stage.ApplicationEventBeforeDispatch,
				EventHandle = eventHandle,
				EventTime = eventTime,
				SuppliedTouchSetPresent = touchSetPresent,
				EventTouchCount = touchCount,
				TouchSampled = (samples & touchFields) == touchFields,
				TouchHandle = touchHandle,
				TouchPhase = touchPhase,
				TouchTime = touchTime,
				TouchX = x,
				TouchY = y,
				EventTargetHandle = targetHandle,
				EventTargetManagedKind = targetManagedKind,
				WindowHandle = windowHandle,
				WindowIsKey = windowIsKey,
				ApplicationState = applicationState,
				Boundary = new BoundaryFields
				{
					Version = 1,
					Samples = sampledFlags,
					Fault = fault,
					RawEventKind = unchecked((ulong)eventType)
				}
			});
		}

		static void AppendBoundaryHeader(StringBuilder text)
		{
			text.Append(" appKitEventLimit=").Append(MaxAppKitEvents)
				.Append(" appKitEventsReserved=").Append(_appKitEventCount)
				.Append(" appKitBudgetSkips=").Append(_appKitBudgetSkips)
				.Append(" appKitBufferFullBeforeRead=").Append(_appKitBufferSkips)
				.Append(" appKitInstall=").Append(_appKitInstall)
				.Append(" appKitRemoval=").Append(_appKitRemoval)
				.Append(" appKitFaultCode=").Append(Volatile.Read(ref _appKitFault))
				.Append(" applicationObserverFaultCode=").Append(Volatile.Read(ref _applicationObserverFault))
				.Append(" boundaryFaultsInvalidateCoverage=true")
				.Append(" appKitInstallAndRemovalReported=").Append(_appKitInstall == NativeInstall.Installed && _appKitRemoval == NativeRemoval.Removed)
				.Append(" applicationTypeSelection=AllUIEventTypesWithinExisting16Budget applicationTouchRead=TouchesOnly")
				.Append(" appKitBoundary=LocalMonitorBeforeNSApplicationSendEvent appKitMask=LeftMouseDown|LeftMouseUp")
				.Append(" appKitReturn=ExactSuppliedPointer windowBridge=UnknownNoPublicCatalystBridge")
				.Append(" appKitCoverage=AppProcessSendEventOnly excludes=OtherProcesses,NestedTrackingLoops,EarlierMonitorConsumption")
				.Append(" osClickCausalIdentityProven=false wdaRequestAssociationProven=false nativeToUIKitTranslationProven=false noRowsProvesOSAbsence=false")
				.Append(" boundarySampleVersion=1 unknown=NotSampled null=SampledNil");
			text.Append(" nativeEventContext=ClickCount,ButtonNumber,ModifierFlags")
				.Append(" nativeWindowKeyObservation=SampledNSWindowIsKeyWindowOnly")
				.Append(" nativeContextGetterLimitPerEvent=9 nativeContextGetterLimitPerScope=144");
		}

		static void AppendBoundaryRow(StringBuilder text, Entry entry)
		{
			text.Append(" boundarySampleMask=").Append(entry.Boundary.Samples)
				.Append(" observerFaultCode=").Append(entry.Boundary.Fault);
			if (entry.Stage == Stage.AppKitLocalEventBeforeSendEvent)
			{
				var samples = (NativeSample)entry.Boundary.Samples;
				bool Has(NativeSample value) => (samples & value) == value;
				text.Append(" nativeEventArgumentSampled=").Append(Has(NativeSample.Argument))
					.Append(" nativeEventArgumentHandle=").Append(Handle(entry.EventHandle, Has(NativeSample.Argument)))
					.Append(" nativeEventKindSampled=").Append(Has(NativeSample.Kind))
					.Append(" nativeEventKind=").Append(Scalar(entry.Boundary.RawEventKind, Has(NativeSample.Kind)))
					.Append(" nativeTimestampSampled=").Append(Has(NativeSample.Timestamp))
					.Append(" nativeTimestamp=").Append(Real(entry.EventTime, Has(NativeSample.Timestamp)))
					.Append(" nativeWindowNumberSampled=").Append(Has(NativeSample.WindowNumber))
					.Append(" nativeWindowNumber=").Append(Scalar(entry.Boundary.NativeWindowNumber, Has(NativeSample.WindowNumber)))
					.Append(" nativePointSampled=").Append(Has(NativeSample.Location))
					.Append(" nativeLocationInWindow=").Append(Real(entry.X, Has(NativeSample.Location)))
					.Append(',').Append(Real(entry.Y, Has(NativeSample.Location)))
					.Append(" nativeWindowSampled=").Append(Has(NativeSample.Window))
					.Append(" nativeWindowHandle=").Append(Handle(entry.WindowHandle, Has(NativeSample.Window)))
					.Append(" nativePointSpace=").Append(!Has(NativeSample.Window) ? "unknown" :
						entry.WindowHandle == 0 ? "AppKitScreenForNilWindow" : "AppKitWindowBaseUnflippedPoints")
					.Append(" matchedUIWindowSampled=false matchedUIWindowHandle=unknown matchedUIWindowRelation=UnknownNoPublicCatalystBridge")
					.Append(" eventReturnPolicy=ExactSuppliedPointer uiEventAssociation=unknown osClickCausalIdentityProven=false")
					.Append(" nativeClickCountSampled=").Append(Has(NativeSample.ClickCount))
					.Append(" nativeClickCount=").Append(Scalar(entry.Boundary.NativeClickCount, Has(NativeSample.ClickCount)))
					.Append(" nativeButtonNumberSampled=").Append(Has(NativeSample.ButtonNumber))
					.Append(" nativeButtonNumber=").Append(Scalar(entry.Boundary.NativeButtonNumber, Has(NativeSample.ButtonNumber)))
					.Append(" nativeModifierFlagsSampled=").Append(Has(NativeSample.ModifierFlags))
					.Append(" nativeModifierFlags=").Append(Scalar(entry.Boundary.NativeModifierFlags, Has(NativeSample.ModifierFlags)))
					.Append(" nativeWindowKeySampled=").Append(Has(NativeSample.WindowKey))
					.Append(" nativeWindowKeyRaw=").Append(Scalar(entry.Boundary.NativeWindowKeyRaw, Has(NativeSample.WindowKey)))
					.Append(" nativeWindowIsKey=").Append(Has(NativeSample.WindowKey) ?
						(entry.Boundary.NativeWindowKeyRaw != 0).ToString() : "unknown");
				return;
			}

			var application = (ApplicationSample)entry.Boundary.Samples;
			bool Sampled(ApplicationSample value) => (application & value) == value;
			text.Append(" suppliedEventSampled=").Append(Sampled(ApplicationSample.Argument | ApplicationSample.Type | ApplicationSample.Timestamp))
				.Append(" eventArgumentSampled=").Append(Sampled(ApplicationSample.Argument))
				.Append(" eventArgumentHandle=").Append(Handle(entry.EventHandle, Sampled(ApplicationSample.Argument)))
				.Append(" suppliedEventTypeSampled=").Append(Sampled(ApplicationSample.Type))
				.Append(" suppliedEventType=").Append(Scalar(unchecked((long)entry.Boundary.RawEventKind), Sampled(ApplicationSample.Type)))
				.Append(" eventTimestampSampled=").Append(Sampled(ApplicationSample.Timestamp))
				.Append(" eventTimestamp=").Append(Real(entry.EventTime, Sampled(ApplicationSample.Timestamp)))
				.Append(" suppliedTouchSetSampled=").Append(Sampled(ApplicationSample.TouchSet))
				.Append(" suppliedTouchSetPresent=").Append(Sampled(ApplicationSample.TouchSet) ? entry.SuppliedTouchSetPresent.ToString() : "unknown")
				.Append(" eventTouchCountSampled=").Append(Sampled(ApplicationSample.TouchCount))
				.Append(" eventTouchCount=").Append(Sampled(ApplicationSample.TouchSet) && !entry.SuppliedTouchSetPresent ?
					"null" : Scalar(entry.EventTouchCount, Sampled(ApplicationSample.TouchCount)))
				.Append(" eventTouchSampled=").Append(entry.TouchSampled)
				.Append(" sampledTouchHandle=").Append(Handle(entry.TouchHandle, Sampled(ApplicationSample.TouchArgument)))
				.Append(" touchPhaseSampled=").Append(Sampled(ApplicationSample.TouchPhase))
				.Append(" touchPhase=").Append(Scalar(entry.TouchPhase, Sampled(ApplicationSample.TouchPhase)))
				.Append(" touchTimestampSampled=").Append(Sampled(ApplicationSample.TouchTimestamp))
				.Append(" touchTimestamp=").Append(Real(entry.TouchTime, Sampled(ApplicationSample.TouchTimestamp)))
				.Append(" touchLocationInWindowSampled=").Append(Sampled(ApplicationSample.TouchLocation))
				.Append(" touchPoint=").Append(Real(entry.TouchX, Sampled(ApplicationSample.TouchLocation)))
				.Append(',').Append(Real(entry.TouchY, Sampled(ApplicationSample.TouchLocation)))
				.Append(" eventTargetSampled=").Append(Sampled(ApplicationSample.Target))
				.Append(" eventTargetHandle=").Append(Handle(entry.EventTargetHandle, Sampled(ApplicationSample.Target)))
				.Append(" eventTargetManagedKind=").Append(Scalar(entry.EventTargetManagedKind, Sampled(ApplicationSample.TargetKind)))
				.Append(" windowSampled=").Append(Sampled(ApplicationSample.Window))
				.Append(" windowHandle=").Append(Handle(entry.WindowHandle, Sampled(ApplicationSample.Window)))
				.Append(" windowKeySampled=").Append(Sampled(ApplicationSample.WindowKey))
				.Append(" windowIsKey=").Append(Sampled(ApplicationSample.WindowKey) ? entry.WindowIsKey.ToString() : "unknown")
				.Append(" applicationStateSampled=").Append(Sampled(ApplicationSample.ApplicationState))
				.Append(" applicationState=").Append(Scalar(entry.ApplicationState, Sampled(ApplicationSample.ApplicationState)))
				.Append(" touchPointSpace=UIKitWindowPoints nativeEventAssociation=unknown nativeDispatchCompletionProven=false");
		}

		static string Handle(long value, bool sampled) =>
			!sampled ? "unknown" : value == 0 ? "null" : value.ToString(CultureInfo.InvariantCulture);
		static string Scalar<T>(T value, bool sampled) where T : IFormattable =>
			sampled ? value.ToString(null, CultureInfo.InvariantCulture) : "unknown";
		static string Real(double value, bool sampled) =>
			sampled ? value.ToString("R", CultureInfo.InvariantCulture) : "unknown";

		struct BoundaryFields
		{
			internal int Version, Samples, Fault;
			internal ulong RawEventKind;
			internal long NativeWindowNumber;
			internal long NativeClickCount, NativeButtonNumber;
			internal ulong NativeModifierFlags;
			internal byte NativeWindowKeyRaw;

			public BoundaryFields()
			{
				Version = Samples = Fault = 0;
				RawEventKind = 0;
				NativeWindowNumber = 0;
				NativeClickCount = NativeButtonNumber = 0;
				NativeModifierFlags = 0;
				NativeWindowKeyRaw = 0;
			}
		}
	}
}
