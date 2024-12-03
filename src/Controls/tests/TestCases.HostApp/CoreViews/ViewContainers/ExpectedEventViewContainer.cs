namespace Maui.Controls.Sample
{
	internal class ExpectedEventViewContainer<T> : ViewContainer<T>
		where T : View
	{
		readonly string _key;
		readonly Label _eventLabel;

		int _numberOfTimesSuccessFired = 0;
		int _numberOfTimesFailedFired = 0;

		public ExpectedEventViewContainer(Enum key, Func<T> view)
			: this(key.ToString(), view)
		{
		}

		public ExpectedEventViewContainer(Enum key, T view)
			: this(key.ToString(), view)
		{
		}

		public ExpectedEventViewContainer(string key, T view)
			: this(key, () => view)
		{
		}

		public ExpectedEventViewContainer(string key, Func<T> view)
			: base(key, view())
		{
			_key = key.ToString();

			_eventLabel = new Label
			{
				AutomationId = $"{key}EventLabel",
				Text = $"Event: {key} (none)"
			};

			ContainerLayout.Children.Add(_eventLabel);
		}

		public void ReportSuccessEvent()
		{
			if (_numberOfTimesFailedFired > 0)
				return;

			_numberOfTimesSuccessFired++;
			_eventLabel.Text = $"Event: {_key} (SUCCESS {_numberOfTimesSuccessFired})";
		}

		public void ReportFailEvent()
		{
			_numberOfTimesFailedFired++;
			_eventLabel.Text = $"Event: {_key} (FAIL {_numberOfTimesFailedFired})";
		}
	}
}
