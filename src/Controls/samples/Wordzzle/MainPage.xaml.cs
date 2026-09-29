using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;

namespace Wordzzle;

public partial class MainPage : ContentPage
{
    private const string WordLengthPreferenceKey = "Wordzzle.WordLength";
    private const string StatsPrefix = "Wordzzle.Stats.";
    private const int DefaultWordLength = 5;
    private static readonly string[] WordLengthChoices = ["4 letters", "5 letters", "6 letters", "7 letters"];

    private readonly DateOnly _puzzleDate = GetPuzzleDate();
    private WordzzleGame _game;
    private string? _statusMessage;

    public MainPage()
    {
        InitializeComponent();

        var wordLength = Preferences.Default.Get(WordLengthPreferenceKey, DefaultWordLength);
        if (!WordzzleGame.IsSupportedWordLength(wordLength))
        {
            wordLength = DefaultWordLength;
            Preferences.Default.Set(WordLengthPreferenceKey, wordLength);
        }

        _game = new WordzzleGame(wordLength, _puzzleDate);
        RestoreGame();
        RenderGame();
    }

    private static DateOnly GetPuzzleDate()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-12));
    }

    private string StateKey => $"Wordzzle.State.{_puzzleDate:yyyy-MM-dd}.{_game.WordLength}";

    private void RestoreGame()
    {
        var savedState = Preferences.Default.Get(StateKey, string.Empty);
        if (string.IsNullOrWhiteSpace(savedState))
        {
            return;
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<GameSnapshot>(savedState);
            if (snapshot is not null && _game.Restore(snapshot))
            {
                return;
            }
        }
        catch (JsonException exception)
        {
            Debug.WriteLine($"Wordzzle saved game could not be read: {exception.Message}");
        }

        Preferences.Default.Remove(StateKey);
        _game = new WordzzleGame(_game.WordLength, _puzzleDate);
        _statusMessage = "The saved puzzle was invalid, so a new round was started.";
    }

    private void SaveGame()
    {
        Preferences.Default.Set(StateKey, JsonSerializer.Serialize(_game.CreateSnapshot()));
    }

    private void RenderGame()
    {
        LengthButton.Text = $"{_game.WordLength} letters";
        DateLabel.Text = $"Daily puzzle  ·  {_puzzleDate:ddd, MMM d}";
        StatusLabel.Text = _statusMessage ?? GetDefaultStatus();
        RenderBoard();
        RenderKeyboard();
    }

    private string GetDefaultStatus()
    {
        if (_game.IsWon)
        {
            return $"Solved in {_game.Guesses.Count}/{WordzzleGame.MaximumAttempts}. New puzzle at noon UTC.";
        }

        if (_game.IsComplete)
        {
            return $"The word was {_game.Answer}. New puzzle at noon UTC.";
        }

        return $"Guess {_game.Guesses.Count + 1} of {WordzzleGame.MaximumAttempts} · Find the {_game.WordLength}-letter word.";
    }

    private void RenderBoard()
    {
        BoardGrid.Children.Clear();
        BoardGrid.RowDefinitions.Clear();
        BoardGrid.ColumnDefinitions.Clear();

        var tileSize = _game.WordLength switch
        {
            4 => 58,
            5 => 52,
            6 => 45,
            _ => 39
        };

        for (var column = 0; column < _game.WordLength; column++)
        {
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(tileSize)));
        }

        for (var row = 0; row < WordzzleGame.MaximumAttempts; row++)
        {
            BoardGrid.RowDefinitions.Add(new RowDefinition(new GridLength(tileSize)));
        }

        BoardGrid.ColumnSpacing = 5;
        BoardGrid.RowSpacing = 5;
        BoardGrid.WidthRequest = (_game.WordLength * tileSize) + ((_game.WordLength - 1) * 5);
        BoardGrid.HeightRequest = (WordzzleGame.MaximumAttempts * tileSize) + ((WordzzleGame.MaximumAttempts - 1) * 5);

        for (var row = 0; row < WordzzleGame.MaximumAttempts; row++)
        {
            for (var column = 0; column < _game.WordLength; column++)
            {
                var mark = _game.GetMark(row, column);
                var letter = _game.GetLetter(row, column);
                var tile = CreateTile(letter, mark, tileSize, row, column);
                Grid.SetRow(tile, row);
                Grid.SetColumn(tile, column);
                BoardGrid.Children.Add(tile);
            }
        }
    }

    private Border CreateTile(string letter, LetterMark mark, int tileSize, int row, int column)
    {
        var (fill, stroke, text) = GetTileColors(mark);
        return new Border
        {
            AutomationId = $"Tile_{row}_{column}",
            WidthRequest = tileSize,
            HeightRequest = tileSize,
            Background = new SolidColorBrush(fill),
            Stroke = new SolidColorBrush(stroke),
            StrokeThickness = mark == LetterMark.Empty ? 2 : 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(5) },
            Content = new Label
            {
                Text = letter,
                FontSize = tileSize * 0.43,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = text
            }
        };
    }

    private void RenderKeyboard()
    {
        KeyboardGrid.Children.Clear();
        KeyboardGrid.RowDefinitions.Clear();

        var keyRows = new[]
        {
            new[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
            new[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
            new[] { "ENTER", "Z", "X", "C", "V", "B", "N", "M", "DELETE" }
        };

        for (var rowIndex = 0; rowIndex < keyRows.Length; rowIndex++)
        {
            KeyboardGrid.RowDefinitions.Add(new RowDefinition(new GridLength(44)));
            var row = CreateKeyboardRow(keyRows[rowIndex], rowIndex);
            Grid.SetRow(row, rowIndex);
            KeyboardGrid.Children.Add(row);
        }
    }

    private Grid CreateKeyboardRow(string[] keys, int rowIndex)
    {
        var row = new Grid
        {
            ColumnSpacing = 4,
            Padding = rowIndex == 1 ? new Thickness(14, 0) : Thickness.Zero
        };

        for (var column = 0; column < keys.Length; column++)
        {
            var key = keys[column];
            var weight = key is "ENTER" or "DELETE" ? 1.7 : 1;
            row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(weight, GridUnitType.Star)));

            var mark = key.Length == 1 ? _game.GetKeyboardMark(key[0]) : LetterMark.Empty;
            var (fill, _, text) = GetTileColors(mark);
            if (mark == LetterMark.Empty)
            {
                fill = IsDarkTheme ? Color.FromArgb("#41454B") : Color.FromArgb("#D7D9DD");
                text = IsDarkTheme ? Color.FromArgb("#F5F4EF") : Color.FromArgb("#30343B");
            }

            var button = new Button
            {
                AutomationId = key == "DELETE" ? "KeyDelete" : key == "ENTER" ? "KeyEnter" : $"Key{key}",
                Text = key == "DELETE" ? "DEL" : key,
                FontSize = key.Length == 1 ? 14 : 10,
                FontAttributes = FontAttributes.Bold,
                Padding = 0,
                Margin = 0,
                CornerRadius = 5,
                HeightRequest = 42,
                BackgroundColor = fill,
                TextColor = text,
                IsEnabled = !_game.IsComplete
            };

            button.Clicked += (_, _) => HandleKey(key);
            Grid.SetColumn(button, column);
            row.Children.Add(button);
        }

        return row;
    }

    private static bool IsDarkTheme => Application.Current?.RequestedTheme == AppTheme.Dark;

    private (Color Fill, Color Stroke, Color Text) GetTileColors(LetterMark mark)
    {
        var dark = IsDarkTheme;
        return mark switch
        {
            LetterMark.Correct => (Color.FromArgb("#538D4E"), Color.FromArgb("#538D4E"), Colors.White),
            LetterMark.Present => (Color.FromArgb("#B59F3B"), Color.FromArgb("#B59F3B"), Colors.White),
            LetterMark.Absent => (Color.FromArgb(dark ? "#3A3D42" : "#7B7F85"), Color.FromArgb(dark ? "#3A3D42" : "#7B7F85"), Colors.White),
            _ => (Color.FromArgb(dark ? "#17191C" : "#F5F4EF"), Color.FromArgb(dark ? "#4A4E55" : "#C9CBCD"), dark ? Color.FromArgb("#F5F4EF") : Color.FromArgb("#30343B"))
        };
    }

    private void HandleKey(string key)
    {
        _statusMessage = null;

        if (_game.IsComplete)
        {
            return;
        }

        if (key == "ENTER")
        {
            HandleSubmit();
            return;
        }

        if (key == "DELETE")
        {
            _game.RemoveLetter();
        }
        else
        {
            _game.AddLetter(key[0]);
        }

        SaveGame();
        RenderGame();
    }

    private void HandleSubmit()
    {
        var result = _game.SubmitGuess();
        switch (result)
        {
            case GuessResult.Incomplete:
                _statusMessage = $"Enter all {_game.WordLength} letters first.";
                break;
            case GuessResult.InvalidWord:
                _statusMessage = "That word is not in the offline dictionary.";
                break;
            case GuessResult.Accepted:
                _statusMessage = $"Good guess. Try {_game.Guesses.Count + 1} of {WordzzleGame.MaximumAttempts}.";
                break;
            case GuessResult.Won:
                _statusMessage = $"Brilliant! Solved in {_game.Guesses.Count}/{WordzzleGame.MaximumAttempts}.";
                RecordGame(won: true, _game.Guesses.Count);
                break;
            case GuessResult.Lost:
                _statusMessage = $"The word was {_game.Answer}.";
                RecordGame(won: false, _game.Guesses.Count);
                break;
        }

        SaveGame();
        RenderGame();
    }

    private void RecordGame(bool won, int guesses)
    {
        var recordedKey = $"{StatsPrefix}Recorded.{_puzzleDate:yyyy-MM-dd}.{_game.WordLength}";
        if (Preferences.Default.Get(recordedKey, false))
        {
            return;
        }

        var played = Preferences.Default.Get($"{StatsPrefix}Played", 0) + 1;
        Preferences.Default.Set($"{StatsPrefix}Played", played);

        if (won)
        {
            var wins = Preferences.Default.Get($"{StatsPrefix}Wins", 0) + 1;
            Preferences.Default.Set($"{StatsPrefix}Wins", wins);
            var distributionKey = $"{StatsPrefix}Guesses.{guesses}";
            Preferences.Default.Set(distributionKey, Preferences.Default.Get(distributionKey, 0) + 1);
        }

        Preferences.Default.Set(recordedKey, true);
    }

    private async void OnHelpClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "How to play",
            $"Guess the hidden {_game.WordLength}-letter word in six tries. Green means the letter is in the right place, gold means it belongs elsewhere, and gray means it is not in the word. Your puzzle and progress are saved on this device.",
            "Got it");
    }

    private async void OnStatsClicked(object? sender, EventArgs e)
    {
        var played = Preferences.Default.Get($"{StatsPrefix}Played", 0);
        var wins = Preferences.Default.Get($"{StatsPrefix}Wins", 0);
        var rate = played == 0 ? 0 : (int)Math.Round(wins * 100d / played);
        var distribution = Enumerable.Range(1, WordzzleGame.MaximumAttempts)
            .Select(count => $"{count}: {Preferences.Default.Get($"{StatsPrefix}Guesses.{count}", 0)}")
            .Aggregate((left, right) => $"{left}   {right}");

        await DisplayAlertAsync(
            "Your statistics",
            $"Games played: {played}\nWins: {wins}\nWin rate: {rate}%\n\nWins by guesses\n{distribution}",
            "Close");
    }

    private async void OnLengthClicked(object? sender, EventArgs e)
    {
        await ChooseWordLengthAsync();
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        await ChooseWordLengthAsync();
    }

    private async Task ChooseWordLengthAsync()
    {
        var selection = await DisplayActionSheetAsync("Daily puzzle length", "Cancel", null, WordLengthChoices);
        if (selection is null || selection == "Cancel")
        {
            return;
        }

        var selectedIndex = Array.IndexOf(WordLengthChoices, selection);
        if (selectedIndex < 0)
        {
            throw new InvalidOperationException($"Unexpected puzzle length selection: {selection}");
        }

        var wordLength = selectedIndex + 4;
        if (wordLength == _game.WordLength)
        {
            return;
        }

        Preferences.Default.Set(WordLengthPreferenceKey, wordLength);
        _game = new WordzzleGame(wordLength, _puzzleDate);
        _statusMessage = null;
        RestoreGame();
        RenderGame();
    }
}
