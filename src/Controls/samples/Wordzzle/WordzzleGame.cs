namespace Wordzzle;

internal enum LetterMark
{
    Empty,
    Absent,
    Present,
    Correct
}

internal enum GuessResult
{
    Incomplete,
    InvalidWord,
    Accepted,
    Won,
    Lost
}

internal sealed record EvaluatedGuess(string Word, LetterMark[] Marks);

internal sealed record GameSnapshot(int WordLength, string PuzzleDate, string[] Guesses, string CurrentGuess);

internal sealed class WordzzleGame
{
    public const int MaximumAttempts = 6;

    private static readonly IReadOnlyDictionary<int, string[]> DailyWords = new Dictionary<int, string[]>
    {
        [4] = ["WORD", "GAME", "PLAY", "CODE", "TREE", "FIRE", "MOON", "STAR", "WIND", "MATH"],
        [5] = ["CRANE", "SHINE", "PLANT", "GRAPE", "APPLE", "SMILE", "PLANE", "SOUND", "TRACE", "ROUTE", "CHARM", "CLOUD"],
        [6] = ["PUZZLE", "ORANGE", "STREAM", "SCREEN", "LETTER", "VIOLET", "MOBILE", "PLAYER", "GARDEN", "SQUARE"],
        [7] = ["JOURNEY", "ORCHARD", "MYSTERY", "RAINBOW", "CAPTURE", "WELCOME", "MONSTER", "HARMONY", "MACHINE", "DEVELOP"]
    };

    private static readonly IReadOnlyDictionary<int, HashSet<string>> AcceptedWords = new Dictionary<int, HashSet<string>>
    {
        [4] = BuildWordSet(DailyWords[4], "THIS THAT HAVE WITH BEEN FROM WARM DARK LAMP READ BLUE BIRD FISH"),
        [5] = BuildWordSet(DailyWords[5], "ABOUT AFTER AGAIN ALONE BEACH BEGIN BREAD BRAVE BRING BUILD CANDY CHAIR CHASE CHEER CHESS CHIEF CHILI CHUNK CLEAR CLOSE COUNT COURT DANCE EARTH ENJOY FAITH FLAME FOCUS FRESH FRONT FRUIT GHOST GRACE GRASS GREAT GREEN GROUP HEART HOUSE HUMAN IDEAL JUICE KNOWN LEARN LEVEL LIGHT MAGIC MAYBE MUSIC NIGHT NORTH OCEAN OFFER OTHER PAPER PARTY PEACE PHONE PLACE POINT POWER PRESS PRICE QUICK QUIET RADIO REACH RIGHT RIVER SCALE SHAPE SHARE SHEEP SHEET SHIFT SHORT SLEEP SMALL SMART SPACE SPARK SPEED SPEND SPICE STAGE STAND START STATE STILL STORE STORY STYLE SUGAR TABLE TEACH THANK THEIR THERE THESE THING THINK THREE THROW TIGER TITLE TODAY TOTAL TOUCH TOWER TRAIN TREAT TRUST TRUTH UNDER UNION UNTIL VALUE VIDEO VISIT VOICE WATER WATCH WHEEL WHERE WHICH WHITE WHOLE WOMAN WORLD WRITE"),
        [6] = BuildWordSet(DailyWords[6], "ANSWER AROUND BEAUTY BOTTLE BRIGHT CHANGE CHOICE CHOOSE COFFEE DANGER DESIGN DRIVER FAMILY FATHER FLOWER FOLLOW FRIEND FUTURE GOLDEN HEALTH HIDDEN HONEST ISLAND KINDLY LITTLE MOTHER NATURE NUMBER OFFICE ORIGIN POCKET PURPLE REASON SAFETY SCHOOL SECRET SHOULD SILVER SIMPLE SINGLE SPRING STREET STRONG SUMMER TARGET TRAVEL USEFUL WINDOW"),
        [7] = BuildWordSet(DailyWords[7], "BALANCE BETWEEN BROUGHT CENTRAL CHAPTER COMMAND COMPASS COUNTRY CRYSTAL DIGITAL DISCOVER DOLLARS ELEMENT EVENING EXAMPLE EXPLORE FANTASY FEATURE FREEDOM FURTHER GATEWAY GENERAL GOODNESS GROCERY IMAGINE IMPROVE INCLUDE INSPIRE KINDRED LIBRARY MILLION MORNING NATURAL OFFICER OUTDOOR PATTERN PICTURE PLANETS PLAYERS POCKETS POPULAR PROBLEM PROGRAM PROMISE QUALITY QUICKLY REALITY RECEIVE REFRESH REGULAR RELEASE REMAINS RESPECT SCIENCE SECTION SERIOUS SHADOWS SPECIAL SPIRITS STATION STRANGE SUCCESS SUPPORT SURFACE THOUGHT THROUGH TONIGHT TOWARDS UNUSUAL VARIOUS VILLAGE VISIBLE WEATHER WHATEVER")
    };

    private readonly string _answer;

    public WordzzleGame(int wordLength, DateOnly puzzleDate)
    {
        if (!IsSupportedWordLength(wordLength))
        {
            throw new ArgumentOutOfRangeException(nameof(wordLength), "Word length must be between four and seven.");
        }

        WordLength = wordLength;
        PuzzleDate = puzzleDate;
        var dailyWords = DailyWords[wordLength];
        var dayNumber = Math.Abs(puzzleDate.DayNumber - new DateOnly(2023, 1, 1).DayNumber);
        _answer = dailyWords[dayNumber % dailyWords.Length];
    }

    public int WordLength { get; }

    public DateOnly PuzzleDate { get; }

    public string Answer => _answer;

    public List<EvaluatedGuess> Guesses { get; } = [];

    public string CurrentGuess { get; private set; } = string.Empty;

    public bool IsWon { get; private set; }

    public bool IsComplete => IsWon || Guesses.Count == MaximumAttempts;

    public static bool IsSupportedWordLength(int wordLength) => DailyWords.ContainsKey(wordLength);

    public void AddLetter(char letter)
    {
        if (IsComplete || CurrentGuess.Length == WordLength || letter is < 'A' or > 'Z')
        {
            return;
        }

        CurrentGuess += letter;
    }

    public void RemoveLetter()
    {
        if (IsComplete || CurrentGuess.Length == 0)
        {
            return;
        }

        CurrentGuess = CurrentGuess[..^1];
    }

    public GuessResult SubmitGuess()
    {
        if (IsComplete)
        {
            return IsWon ? GuessResult.Won : GuessResult.Lost;
        }

        if (CurrentGuess.Length != WordLength)
        {
            return GuessResult.Incomplete;
        }

        if (!AcceptedWords[WordLength].Contains(CurrentGuess))
        {
            return GuessResult.InvalidWord;
        }

        Guesses.Add(new EvaluatedGuess(CurrentGuess, Evaluate(CurrentGuess, _answer)));
        CurrentGuess = string.Empty;

        if (Guesses[^1].Word == _answer)
        {
            IsWon = true;
            return GuessResult.Won;
        }

        return IsComplete ? GuessResult.Lost : GuessResult.Accepted;
    }

    public string GetLetter(int row, int column)
    {
        if (row < Guesses.Count)
        {
            return Guesses[row].Word[column].ToString();
        }

        if (row == Guesses.Count && column < CurrentGuess.Length)
        {
            return CurrentGuess[column].ToString();
        }

        return string.Empty;
    }

    public LetterMark GetMark(int row, int column)
    {
        return row < Guesses.Count ? Guesses[row].Marks[column] : LetterMark.Empty;
    }

    public LetterMark GetKeyboardMark(char letter)
    {
        var bestMark = LetterMark.Empty;
        foreach (var guess in Guesses)
        {
            for (var index = 0; index < WordLength; index++)
            {
                if (guess.Word[index] == letter && MarkPriority(guess.Marks[index]) > MarkPriority(bestMark))
                {
                    bestMark = guess.Marks[index];
                }
            }
        }

        return bestMark;
    }

    public GameSnapshot CreateSnapshot()
    {
        return new GameSnapshot(WordLength, PuzzleDate.ToString("yyyy-MM-dd"), Guesses.Select(guess => guess.Word).ToArray(), CurrentGuess);
    }

    public bool Restore(GameSnapshot snapshot)
    {
        if (snapshot.WordLength != WordLength
            || snapshot.PuzzleDate != PuzzleDate.ToString("yyyy-MM-dd")
            || snapshot.Guesses is null
            || snapshot.Guesses.Length > MaximumAttempts
            || snapshot.CurrentGuess is null
            || snapshot.CurrentGuess.Length > WordLength
            || snapshot.CurrentGuess.Any(character => character is < 'A' or > 'Z'))
        {
            return false;
        }

        for (var index = 0; index < snapshot.Guesses.Length; index++)
        {
            var guess = snapshot.Guesses[index];
            if (guess is null || guess.Length != WordLength || guess.Any(character => character is < 'A' or > 'Z'))
            {
                return false;
            }

            CurrentGuess = guess;
            var result = SubmitGuess();
            if (result is not (GuessResult.Accepted or GuessResult.Won or GuessResult.Lost))
            {
                return false;
            }

            if (IsComplete && index != snapshot.Guesses.Length - 1)
            {
                return false;
            }
        }

        if (IsComplete && snapshot.CurrentGuess.Length > 0)
        {
            return false;
        }

        CurrentGuess = snapshot.CurrentGuess;
        return true;
    }

    private static LetterMark[] Evaluate(string guess, string answer)
    {
        var marks = Enumerable.Repeat(LetterMark.Absent, guess.Length).ToArray();
        var remaining = new int[26];

        for (var index = 0; index < answer.Length; index++)
        {
            if (guess[index] == answer[index])
            {
                marks[index] = LetterMark.Correct;
            }
            else
            {
                remaining[answer[index] - 'A']++;
            }
        }

        for (var index = 0; index < guess.Length; index++)
        {
            if (marks[index] == LetterMark.Correct)
            {
                continue;
            }

            var letterIndex = guess[index] - 'A';
            if (remaining[letterIndex] > 0)
            {
                marks[index] = LetterMark.Present;
                remaining[letterIndex]--;
            }
        }

        return marks;
    }

    private static int MarkPriority(LetterMark mark) => mark switch
    {
        LetterMark.Correct => 3,
        LetterMark.Present => 2,
        LetterMark.Absent => 1,
        _ => 0
    };

    private static HashSet<string> BuildWordSet(IEnumerable<string> dailyWords, string additionalWords)
    {
        return dailyWords.Concat(additionalWords.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToHashSet(StringComparer.Ordinal);
    }
}
