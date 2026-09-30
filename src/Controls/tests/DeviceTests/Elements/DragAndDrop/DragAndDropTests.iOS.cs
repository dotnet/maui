using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Gesture)]
	public class DragAndDropTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task SessionDidUpdateDefaultsToCopy(bool allowsMove)
		{
			return WithSession(allowsMove, (label, update) =>
			{
				label.GestureRecognizers.Add(new DropGestureRecognizer());

				using var proposal = update();

				Assert.Equal(UIDropOperation.Copy, proposal.Operation);
			});
		}

		[Theory]
		[InlineData(DataPackageOperation.None, false, UIDropOperation.Cancel)]
		[InlineData(DataPackageOperation.None, true, UIDropOperation.Cancel)]
		[InlineData(DataPackageOperation.Copy, false, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Copy, true, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Move, false, UIDropOperation.Cancel)]
		[InlineData(DataPackageOperation.Move, true, UIDropOperation.Move)]
		[InlineData(DataPackageOperation.Copy | DataPackageOperation.Move, false, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Copy | DataPackageOperation.Move, true, UIDropOperation.Copy)]
		public Task SessionDidUpdateRoutesAcceptedOperation(DataPackageOperation accepted, bool allowsMove, UIDropOperation expected)
		{
			return WithSession(allowsMove, (label, update) =>
			{
				var recognizer = new DropGestureRecognizer();
				var raised = false;
				recognizer.DragOver += (_, args) =>
				{
					raised = true;
					Assert.Equal(DataPackageOperation.Copy, args.AcceptedOperation);
					Assert.NotNull(args.PlatformArgs);
					Assert.Same(label.Handler.PlatformView, args.PlatformArgs.Sender);
					args.AcceptedOperation = accepted;
				};
				label.GestureRecognizers.Add(recognizer);

				using var proposal = update();

				Assert.True(raised);
				Assert.Equal(expected, proposal.Operation);
			});
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public Task SessionDidUpdateWithoutDropRecognizerCancels(bool allowsMove, bool addUnrelatedRecognizer)
		{
			return WithSession(allowsMove, (label, update) =>
			{
				if (addUnrelatedRecognizer)
					label.GestureRecognizers.Add(new TapGestureRecognizer());

				using var proposal = update();

				Assert.Equal(UIDropOperation.Cancel, proposal.Operation);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task SessionDidUpdateSkipsDisabledDropRecognizer(bool allowsMove)
		{
			return WithSession(allowsMove, (label, update) =>
			{
				var recognizer = new DropGestureRecognizer { AllowDrop = false };
				recognizer.DragOver += (_, _) => Assert.Fail("A disabled drop recognizer must not receive DragOver.");
				label.GestureRecognizers.Add(recognizer);

				using var proposal = update();

				Assert.Equal(UIDropOperation.Cancel, proposal.Operation);
			});
		}

		[Theory]
		[InlineData(DataPackageOperation.Copy, false, UIDropOperation.Move)]
		[InlineData(DataPackageOperation.Move, false, UIDropOperation.Move)]
		[InlineData(DataPackageOperation.Move, true, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Copy | DataPackageOperation.Move, true, UIDropOperation.Forbidden)]
		[InlineData(DataPackageOperation.Copy, true, UIDropOperation.Cancel)]
		[InlineData(DataPackageOperation.None, true, UIDropOperation.Move)]
		[InlineData(DataPackageOperation.None, false, UIDropOperation.Copy)]
		public Task SessionDidUpdateHonorsCustomProposalOnlyWhenAccepted(DataPackageOperation accepted, bool allowsMove, UIDropOperation customOperation)
		{
			return WithSession(allowsMove, (label, update) =>
			{
				using var customProposal = new UIDropProposal(customOperation);
				var recognizer = new DropGestureRecognizer();
				recognizer.DragOver += (_, args) =>
				{
					args.AcceptedOperation = accepted;
					args.PlatformArgs.SetDropProposal(customProposal);
				};
				label.GestureRecognizers.Add(recognizer);

				var proposal = update();
				if (accepted == DataPackageOperation.None)
				{
					using (proposal)
					{
						Assert.NotSame(customProposal, proposal);
						Assert.Equal(UIDropOperation.Cancel, proposal.Operation);
					}
				}
				else
				{
					Assert.Same(customProposal, proposal);
					Assert.Equal(customOperation, proposal.Operation);
				}
			});
		}

		[Theory]
		[InlineData(DataPackageOperation.Move, DataPackageOperation.None, true, UIDropOperation.Move)]
		[InlineData(DataPackageOperation.Move, DataPackageOperation.None, false, UIDropOperation.Cancel)]
		[InlineData(DataPackageOperation.Copy, DataPackageOperation.None, true, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.None, DataPackageOperation.Move, true, UIDropOperation.Move)]
		[InlineData(DataPackageOperation.Copy, DataPackageOperation.Move, true, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Copy, DataPackageOperation.Move, false, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Move, DataPackageOperation.Copy, true, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.None, DataPackageOperation.None, true, UIDropOperation.Cancel)]
		public Task SessionDidUpdateCombinesRecognizerAcceptance(DataPackageOperation first, DataPackageOperation second, bool allowsMove, UIDropOperation expected)
		{
			return WithSession(allowsMove, (label, update) =>
			{
				var calls = new List<int>();
				var firstRecognizer = new DropGestureRecognizer();
				firstRecognizer.DragOver += (_, args) =>
				{
					calls.Add(1);
					args.AcceptedOperation = first;
				};
				var secondRecognizer = new DropGestureRecognizer();
				secondRecognizer.DragOver += (_, args) =>
				{
					calls.Add(2);
					args.AcceptedOperation = second;
				};
				label.GestureRecognizers.Add(firstRecognizer);
				var disabledRecognizer = new DropGestureRecognizer { AllowDrop = false };
				disabledRecognizer.DragOver += (_, _) => Assert.Fail("A disabled drop recognizer must not receive DragOver.");
				label.GestureRecognizers.Add(disabledRecognizer);
				label.GestureRecognizers.Add(secondRecognizer);

				using var proposal = update();

				Assert.Equal(new[] { 1, 2 }, calls);
				Assert.Equal(expected, proposal.Operation);
			});
		}

		[Fact]
		public Task SessionDidUpdateDoesNotRetainPreviousAcceptance()
		{
			return WithSession(true, (label, update) =>
			{
				var accepted = DataPackageOperation.Move;
				var recognizer = new DropGestureRecognizer();
				recognizer.DragOver += (_, args) =>
				{
					Assert.Equal(DataPackageOperation.Copy, args.AcceptedOperation);
					args.AcceptedOperation = accepted;
				};
				label.GestureRecognizers.Add(recognizer);

				using var firstProposal = update();
				Assert.Equal(UIDropOperation.Move, firstProposal.Operation);

				accepted = DataPackageOperation.None;
				using var secondProposal = update();
				Assert.Equal(UIDropOperation.Cancel, secondProposal.Operation);
			});
		}

		[Fact]
		public Task SessionDidUpdateDoesNotRetainPreviousCustomProposal()
		{
			return WithSession(true, (label, update) =>
			{
				using var customProposal = new UIDropProposal(UIDropOperation.Forbidden);
				var useCustomProposal = true;
				var recognizer = new DropGestureRecognizer();
				recognizer.DragOver += (_, args) =>
				{
					args.AcceptedOperation = DataPackageOperation.Move;
					if (useCustomProposal)
						args.PlatformArgs.SetDropProposal(customProposal);
				};
				label.GestureRecognizers.Add(recognizer);

				Assert.Same(customProposal, update());

				useCustomProposal = false;
				using var proposal = update();
				Assert.Equal(UIDropOperation.Move, proposal.Operation);
			});
		}

		[Theory]
		[InlineData(DataPackageOperation.Copy, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Move, UIDropOperation.Move)]
		public Task SessionDidUpdateReevaluatesChangedRecognizers(DataPackageOperation accepted, UIDropOperation expected)
		{
			return WithSession(true, (label, update) =>
			{
				var calls = 0;
				var recognizer = new DropGestureRecognizer();
				recognizer.DragOver += (_, args) =>
				{
					calls++;
					args.AcceptedOperation = accepted;
				};
				label.GestureRecognizers.Add(recognizer);

				using var initial = update();
				Assert.Equal(expected, initial.Operation);
				Assert.Equal(1, calls);

				recognizer.AllowDrop = false;
				using var disabled = update();
				Assert.Equal(UIDropOperation.Cancel, disabled.Operation);
				Assert.Equal(1, calls);

				recognizer.AllowDrop = true;
				using var enabled = update();
				Assert.Equal(expected, enabled.Operation);
				Assert.Equal(2, calls);

				label.GestureRecognizers.Remove(recognizer);
				using var removed = update();
				Assert.Equal(UIDropOperation.Cancel, removed.Operation);
				Assert.Equal(2, calls);

				label.GestureRecognizers.Add(new DropGestureRecognizer());
				using var replacement = update();
				Assert.Equal(UIDropOperation.Copy, replacement.Operation);
			});
		}

		[Theory]
		[InlineData(DataPackageOperation.None, UIDropOperation.Cancel)]
		[InlineData(DataPackageOperation.Copy, UIDropOperation.Copy)]
		[InlineData(DataPackageOperation.Move, UIDropOperation.Move)]
		public Task SessionDidUpdatePreservesAcceptanceThroughPassiveRecognizer(DataPackageOperation accepted, UIDropOperation expected)
		{
			return WithSession(true, (label, update) =>
			{
				var calls = new List<string>();
				var first = new DropGestureRecognizer();
				first.DragOver += (_, args) =>
				{
					calls.Add("first");
					args.AcceptedOperation = accepted;
				};
				var second = new DropGestureRecognizer
				{
					DragOverCommand = new Command(() => calls.Add("command"))
				};
				second.DragOver += (_, args) =>
				{
					calls.Add("second");
					Assert.Equal(accepted, args.AcceptedOperation);
				};
				label.GestureRecognizers.Add(first);
				label.GestureRecognizers.Add(second);

				using var proposal = update();

				Assert.Equal(new[] { "first", "command", "second" }, calls);
				Assert.Equal(expected, proposal.Operation);
			});
		}

		Task WithSession(bool allowsMove, Action<Label, Func<UIDropProposal>> test)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Drop target" };
				var handler = CreateHandler<LabelHandler>(label);
				using var dropDelegate = new DragAndDropDelegate(handler);
				using var interaction = new UIDropInteraction(dropDelegate);
				using var session = new DropSession(allowsMove);
				try
				{
					test(label, () => dropDelegate.SessionDidUpdate(interaction, session));
				}
				finally
				{
					dropDelegate.Disconnect();
					((IElementHandler)handler).DisconnectHandler();
				}
			});
		}

		sealed class DropSession : NSObject, IUIDropSession
		{
			public DropSession(bool allowsMove)
			{
				AllowsMoveOperation = allowsMove;
			}

			public bool AllowsMoveOperation { get; }
			public UIDragItem[] Items => Array.Empty<UIDragItem>();
			public bool RestrictedToDraggingApplication => false;
			public IUIDragSession LocalDragSession => null;
			public UIDropSessionProgressIndicatorStyle ProgressIndicatorStyle { get; set; }
			public NSProgress Progress => throw new NotSupportedException();
			public CGPoint LocationInView(UIView view) => CGPoint.Empty;
			public bool HasConformingItems(string[] typeIdentifiers) => false;
			public bool CanLoadObjects(Class itemProviderReadingClass) => false;
			public NSProgress LoadObjects(Class itemProviderReadingClass, Action<INSItemProviderReading[]> completion) =>
				throw new NotSupportedException();
		}
	}
}
