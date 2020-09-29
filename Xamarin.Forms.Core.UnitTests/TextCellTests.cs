using System;
using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TextCellTests : BaseTestFixture
	{
		[Test]
		public void TestTapped()
		{
			var cell = new TextCell();
			bool tapped = false;
			cell.Tapped += (sender, args) => tapped = true;

			cell.OnTapped();
			Assert.True(tapped);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TappedHonorsCanExecute(bool canExecute)
		{
			bool executed = false;

			var cmd = new Command(() => executed = true, () => canExecute);
			var cell = new TextCell { Command = cmd };
			cell.OnTapped();

			Assert.That(executed, Is.EqualTo(canExecute));
		}

		[Test]
		public void TestCommand()
		{
			bool executed = false;

			var cmd = new Command(() => executed = true);
			var cell = new TextCell();
			cell.Command = cmd;
			cell.OnTapped();

			Assert.IsTrue(executed, "Command was not executed");
		}

		[Test]
		public void TestCommandParameter()
		{
			bool executed = false;

			object obj = new object();
			var cmd = new Command(p =>
			{
				Assert.AreSame(obj, p);
				executed = true;
			});

			var cell = new TextCell
			{
				Command = cmd,
				CommandParameter = obj
			};

			cell.OnTapped();

			Assert.IsTrue(executed, "Command was not executed");
		}

		[Test]
		public void TestCommandCanExecute()
		{
			bool tested = false;

			var cmd = new Command(() => { },
				canExecute: () =>
				{
					tested = true;
					return true;
				});

			new TextCell { Command = cmd };
			Assert.IsTrue(tested, "Command.CanExecute was not called");
		}

		[Test]
		public void TestCommandCanExecuteDisables()
		{
			var cmd = new Command(() => { }, () => false);
			var cell = new TextCell { Command = cmd };
			Assert.IsFalse(cell.IsEnabled, "Cell was not disabled");
		}

		[Test]
		public void TestCommandCanExecuteChanged()
		{
			bool first = true;
			var cmd = new Command(() => { }, () =>
			{
				if (first)
				{
					first = false;
					return false;
				}
				else
				{
					return true;
				}
			});

			var cell = new TextCell { Command = cmd };
			Assert.IsFalse(cell.IsEnabled, "Cell was not disabled");

			cmd.ChangeCanExecute();

			Assert.IsTrue(cell.IsEnabled, "Cell was not reenabled");
		}

		[Test]
		public void Create()
		{
			var template = new DataTemplate(typeof(TextCell));
			var content = template.CreateContent();

			Assert.IsNotNull(content);
			Assert.That(content, Is.InstanceOf<TextCell>());
		}

		[Test]
		public void Detail()
		{
			var template = new DataTemplate(typeof(TextCell));
			template.SetValue(TextCell.DetailProperty, "detail");

			TextCell cell = (TextCell)template.CreateContent();
			Assert.That(cell.Detail, Is.EqualTo("detail"));
		}

		[Test]
		public void Text()
		{
			var template = new DataTemplate(typeof(TextCell));
			template.SetValue(TextCell.TextProperty, "text");

			TextCell cell = (TextCell)template.CreateContent();
			Assert.That(cell.Text, Is.EqualTo("text"));
		}
	}
}