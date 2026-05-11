using System.Text.Json;
using PersonalFinanceCli.Domain.Entities;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonDataStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonDataStore(string filePath)
    {
        _filePath = filePath;

        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public DataFile Load()
    {
        if (!File.Exists(_filePath))
        {
            return CreateEmptyDataFile();
        }

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateEmptyDataFile();
        }

        var result = JsonSerializer.Deserialize<DataFile>(json, _options);

        if (result == null)
        {
            return CreateEmptyDataFile();
        }

        result.Cards ??= new List<Card>();
        result.Transactions ??= new List<Transaction>();
        result.DailyLimits ??= new List<DailyLimit>();

        return result;
    }

    public void Save(DataFile data)
    {
        var dir = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(data, _options);

        File.WriteAllText(_filePath, json);
    }

    private DataFile CreateEmptyDataFile()
    {
        var emptyData = new DataFile();

        Save(emptyData);

        return emptyData;
    }
}