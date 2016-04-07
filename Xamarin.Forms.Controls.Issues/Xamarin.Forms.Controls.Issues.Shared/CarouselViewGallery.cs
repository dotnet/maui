using System;

using Xamarin.Forms.CustomAttributes;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 900000, "CarouselView General Tests")]
	public class CarouselViewGalleryTests
	{
#if UITEST

		public interface IUIProxy
		{
			void Load(IApp app);
		}
		public interface IGalleryPage : IUIProxy
		{
			string Name { get; }
		}

		public class Gallery
		{
			static class Id
			{
				internal const string SearchBar = nameof(SearchBar);
				internal const string GoToTestButton = nameof(GoToTestButton);
			}

			public static Gallery Launch()
			{
				var app = AppSetup.Setup();
				app.WaitForElement(Id.SearchBar);
				return new Gallery(app);
			}

			IApp _app;

			Gallery(IApp app)
			{
				_app = app;
			}

			public TGalleryPage NaviateToGallery<TGalleryPage>() where TGalleryPage : IGalleryPage, new()
			{
				var galleryPage = new TGalleryPage();
				_app.EnterText(Id.SearchBar, galleryPage.Name);
				_app.Tap(Id.GoToTestButton);
				galleryPage.Load(_app);
				return galleryPage;
			}
			public void Screenshot(string message) => _app.Screenshot(message);
		}

		public class CarouselViewGallery : IGalleryPage
		{
			internal const int InitialItems = 4;
			internal const int InitialItemId = 1;
			internal const string OnItemSelectedAbbr = "i";
			internal const string OnPositionSelectedAbbr = "p";
			internal const int EventQueueDepth = 7;

			private const double SwipePercentage = 0.50;
			private const int SwipeSpeed = 2000;

			static class Id
			{
				internal const string Name = "CarouselView Gallery";
				internal static string ItemId = nameof(ItemId);
				internal static string EventLog = nameof(EventLog);
				internal static string SelectedItem = nameof(SelectedItem);
				internal static string Position = nameof(Position);
				internal static string SelectedPosition = nameof(SelectedPosition);
				internal static string Next = nameof(Next);
				internal static string Previous = nameof(Previous);
				internal static string First = nameof(First);
				internal static string Last = nameof(Last);
			}
			enum Event
			{
				OnItemSelected,
				OnPositionSelected
			}

			IApp _app;
			List<int> _itemIds;
			int _currentPosition;
			int _currentItem;
			Queue<string> _expectedEvents;
			int _eventId;

			public CarouselViewGallery() {
				_itemIds = Enumerable.Range(0, InitialItems).ToList();
				_currentPosition = InitialItemId;
				_currentItem = _itemIds[_currentPosition];
				_expectedEvents = new Queue<string>();
				_eventId = 0;
			}

			void IUIProxy.Load(IApp app)
			{
				_app = app;
				WaitForValue(Id.ItemId, _currentItem);
				WaitForValue(Id.Position, _currentPosition);
			}

			private void WaitForValue(string marked, object value)
			{
				var query = $"* marked:'{marked}' text:'{value}'";
				_app.WaitForElement(o => o.Raw(query));

			}
			private void WaitForPosition(int expectedPosition)
			{
				var expectedItem = _itemIds[expectedPosition];

				// expect no movement
				if (_currentItem == expectedItem)
					Thread.Sleep(TimeSpan.FromMilliseconds(500));

				// wait for for expected item and corresponding event
				WaitForValue(Id.ItemId, expectedItem);
				WaitForValue(Id.SelectedItem, expectedItem);
				_currentItem = expectedItem;

				// wait for for expected position and corresponding event
				WaitForValue(Id.Position, expectedPosition);
				WaitForValue(Id.SelectedPosition, expectedPosition);
				_currentPosition = expectedPosition;

				// check expected events
				var expectedEvents = string.Join(", ", _expectedEvents.ToArray().Reverse());
				WaitForValue(Id.EventLog, expectedEvents);
			}
			private void ExpectMovementEvents(int expectedPosition)
			{
				if (expectedPosition == _currentPosition)
					return;

				ExpectEvent(Event.OnPositionSelected);
				ExpectEvent(Event.OnItemSelected);
			}
			private void ExpectEvent(Event e)
			{
				if (e == Event.OnItemSelected)
					_expectedEvents.Enqueue($"{OnItemSelectedAbbr}/{_eventId++}");

				if (e == Event.OnPositionSelected)
					_expectedEvents.Enqueue($"{OnPositionSelectedAbbr}/{_eventId++}");

				if (_expectedEvents.Count == EventQueueDepth)
					_expectedEvents.Dequeue();
			}
			private void Tap(string buttonText, int expectedPosition)
			{
				// tap
				_app.Tap(buttonText);

				// anticipate events
				ExpectMovementEvents(expectedPosition);

				// wait
				WaitForPosition(expectedPosition);
			}
			private void Swipe(bool next, int expectedPosition)
			{
				// swipe
				if (next)
					_app.SwipeRightToLeft(swipePercentage: SwipePercentage/*, swipeSpeed: SwipeSpeed*/);
				else
					_app.SwipeLeftToRight(swipePercentage: SwipePercentage/*, swipeSpeed: SwipeSpeed*/);

				// handle swipe past first
				if (expectedPosition == -1 && _currentPosition == 0)
					expectedPosition = 0;

				// handle swipe past last
				else if (expectedPosition == Count && _currentPosition == Count -1)
					expectedPosition = Count -1;

				// anticipate events
				ExpectMovementEvents(expectedPosition);

				// wait
				WaitForPosition(expectedPosition);
			}
			private void Move(int steps, bool swipe)
			{
				Action next = swipe ? (Action)SwipeNext : StepNext;
				Action previous = swipe ? (Action)SwipePrevious : StepPrevious;

				var action = next;
				if (steps < 0)
				{
					action = previous;
					steps = -steps;
				}

				for (int i = 0; i < steps; i++)
					action();
			}
			private void MoveToPosition(int position, bool swipe)
			{
				Assert.True(position >= 0 && position < Count);
				Move(position - _currentPosition, swipe);
			}
			private void MoveToItem(int targetPage, bool swipe)
			{
				MoveToPosition(_itemIds.IndexOf(targetPage), swipe);
			}
			public void MoveToFirst(bool swipe) => MoveToPosition(0, swipe);
			public void MoveToLast(bool swipe) => MoveToPosition(Count - 1, swipe);

			public int ItemId => int.Parse(_app.Query(Id.ItemId)[0].Text);

			public string Name => Id.Name;
			public int Count => _itemIds.Count;

			public void First() => Tap(Id.First, 0);
			public void Last() => Tap(Id.Last, _itemIds.Count - 1);

			public void StepNext() => Tap(Id.Next, _currentPosition + 1);
			public void StepPrevious() => Tap(Id.Previous, _currentPosition - 1);
			public void Step(int steps) => Move(steps, swipe: false);
			public void StepToPosition(int position) => MoveToPosition(position, swipe: false);
			public void StepToItem(int item) => MoveToItem(item, swipe: false);
			public void StepToFirst() => MoveToFirst(swipe: false);
			public void StepToLast() => MoveToLast(swipe: false);

			public void SwipeNext() => Swipe(next: true, expectedPosition: _currentPosition + 1);
			public void SwipePrevious() => Swipe(next: false, expectedPosition: _currentPosition - 1);
			public void Swipe(int swipes) => Move(swipes, swipe: true);
			public void SwipeToPosition(int position) => MoveToPosition(position, swipe: true);
			public void SwipeToItem(int item) => MoveToItem(item, swipe: true);
			public void SwipeToFirst() => MoveToFirst(swipe: true);
			public void SwipeToLast() => MoveToLast(swipe: true);
		}

		//[Test]
		public void SwipeStepJump()
		{
			var gallery = Gallery.Launch();

			try {
				var carousel = gallery.NaviateToGallery<CarouselViewGallery>();

				// start at something other than 0
				Assert.AreNotEqual(0, CarouselViewGallery.InitialItemId);
				Assert.AreEqual(CarouselViewGallery.InitialItemId, carousel.ItemId);

				// programatic jump to first/last
				carousel.Last();
				carousel.First();

				// programatic step to page
				carousel.StepToLast();
				carousel.StepToFirst();

				// swiping
				carousel.SwipeToLast();
				carousel.SwipeNext(); // test swipe past end
				carousel.SwipeToFirst();
				carousel.SwipePrevious(); // test swipe past start

			} catch (Exception e) {
				gallery.Screenshot(e.ToString());
				throw e;
			}
		}
#endif
    }
}
