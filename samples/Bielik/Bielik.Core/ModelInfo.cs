namespace Bielik.Core;

public static class ModelInfo
{
    public const string Id = "hf.co/speakleash/Bielik-11B-v2.6-Instruct-GGUF:Q4_K_M";
    public const string Name = "Bielik 11B";
    public const string Version = "v2.6 Instruct";
    public const string Quantization = "Q4_K_M";
    public const string DefaultEndpoint = "http://127.0.0.1:11434";
    public const string PullCommand = "ollama pull " + Id;
    public const string ModelCard = "https://huggingface.co/speakleash/Bielik-11B-v2.6-Instruct-GGUF";
    public const string SystemPrompt =
        "Jesteś Bielikiem, pomocnym polskojęzycznym asystentem. " +
        "Odpowiadaj po polsku, zwięźle i naturalnie, chyba że użytkownik poprosi o inny język lub format. " +
        "Nie wymyślaj faktów, źródeł ani dostępu do internetu. Gdy czegoś nie wiesz, powiedz to wprost.";
}
