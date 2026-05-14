namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33510, "[Android] RefreshView triggers pull-to-refresh immediately when scrolling up inside a WebView", PlatformAffected.Android)]
public class Issue33510 : TestContentPage
{
	RefreshView _refreshView;
	Label _statusLabel;

	protected override void Init()
	{
		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Loading..."
		};

		var webView = new WebView
		{
			AutomationId = "TestWebView",
			Source = new HtmlWebViewSource { Html = ScrollableHtml }
		};

		webView.Navigated += (_, _) => _statusLabel.Text = "WebView ready";

		_refreshView = new RefreshView
		{
			AutomationId = "TestRefreshView",
			Content = webView
		};

		_refreshView.Command = new Command(async () =>
		{
			_statusLabel.Text = "Refresh triggered";
			await Task.Delay(150);
			_refreshView.IsRefreshing = false;
		});

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(_statusLabel, 0, 0);
		grid.Add(_refreshView, 0, 1);

		Content = grid;
	}

	const string ScrollableHtml = """
		<!doctype html>
		<html>
		<head>
			<meta charset="utf-8" />
			<meta name="viewport" content="width=device-width, initial-scale=1" />
			<style>
				html, body {
					margin: 0;
					padding: 0;
					height: 100%;
					overflow: hidden;
					font-family: sans-serif;
				}

				#wrapper {
					height: 100%;
					display: flex;
					flex-direction: column;
				}

				#header {
					position: sticky;
					top: 0;
					padding: 12px;
					background: #f2f2f2;
					border-bottom: 1px solid #d9d9d9;
					font-weight: 600;
				}

				#scrollHost {
					flex: 1;
					overflow-y: auto;
					-webkit-overflow-scrolling: touch;
					padding: 12px;
					box-sizing: border-box;
				}

				.card {
					margin-bottom: 12px;
					padding: 12px;
					border-radius: 8px;
					background: #e8f0fe;
				}
			</style>
		</head>
		<body>
			<div id="wrapper">
				<div id="header">Issue33510 Internal Scroll Host</div>
				<div id="scrollHost">
					<div class="card">Content 01</div>
					<div class="card">Content 02</div>
					<div class="card">Content 03</div>
					<div class="card">Content 04</div>
					<div class="card">Content 05</div>
					<div class="card">Content 06</div>
					<div class="card">Content 07</div>
					<div class="card">Content 08</div>
					<div class="card">Content 09</div>
					<div class="card">Content 10</div>
					<div class="card">Content 11</div>
					<div class="card">Content 12</div>
					<div class="card">Content 13</div>
					<div class="card">Content 14</div>
					<div class="card">Content 15</div>
					<div class="card">Content 16</div>
					<div class="card">Content 17</div>
					<div class="card">Content 18</div>
					<div class="card">Content 19</div>
					<div class="card">Content 20</div>
				</div>
			</div>
		</body>
		</html>
		""";
}
