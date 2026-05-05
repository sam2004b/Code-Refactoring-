using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonLimitRepository : ILimitRepository
{
    private readonly JsonDataStore _store;

    public JsonLimitRepository(JsonDataStore store)
    {
        _store = store;
    }

    public DailyLimit? GetByDate(DateOnly date)
    {
         var data = _store.Load();

        return data.DailyLimits
            .FirstOrDefault(limit => limit.Date == date);
    }

    public DailyLimit Upsert(DateOnly date, decimal amount, Currency currency)
    {
        var data = _store.Load();

        var existingLimit = data.DailyLimits
        .FirstOrDefault(x => x.Date == date);
        if (existingLimit is null)
        {
            existingLimit = new DailyLimit
            {
                Id = data.DailyLimits.Count == 0 
                ? 1 
                : data.DailyLimits.Max(x => x.Id) + 1,
                
                Date = date,
                Amount = amount,
                Currency = currency
            };
            
            data.DailyLimits.Add(existingLimit);
        }
        else
        {
            existingLimit.Amount = amount;
            existingLimit.Currency = currency;
        }

        _store.Save(data);
        
        return existingLimit;
    }
}
