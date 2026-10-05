namespace Bielik.Core;

public sealed record PromptIdea(string Id, string Category, string Title, string Subtitle, string Prompt);

public static class PromptCatalog
{
    public static IReadOnlyList<PromptIdea> All { get; } =
    [
        new("learn", "ODKRYWAJ", "Wyjaśnij mi to", "Trudne rzeczy, prostymi słowami",
            "Wyjaśnij w trzech krótkich zdaniach, jak działa model językowy. Użyj porównania z codziennego życia."),
        new("write", "TWÓRZ", "Znajdź dobre słowa", "Od pustej kartki do pierwszego szkicu",
            "Napisz krótki, serdeczny e-mail z podziękowaniem za pomoc przy projekcie. Bez korporacyjnego żargonu."),
        new("plan", "PLANUJ", "Ułóż dobry plan", "Małe kroki, większe możliwości",
            "Zaproponuj pięć prostych kroków na rozpoczęcie nauki języka polskiego. Odpowiedz zwięźle."),
        new("code", "BUDUJ", "Pomyśl w kodzie", "Pomoc w programowaniu, bez chmury",
            "Napisz krótką funkcję w C#, która sprawdza, czy liczba jest parzysta. Wyjaśnij w jednym zdaniu, jak działa.")
    ];
}
