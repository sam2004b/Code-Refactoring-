using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonTransactionRepository : ITransactionRepository
{
    private readonly JsonDataStore _store;

    public JsonTransactionRepository(JsonDataStore store)
    {
        _store = store;
    }

    public IReadOnlyList<Transaction> GetAll()
    {
       var data = _store.Load();

        return data.Transactions
            .OrderBy(transaction => transaction.Id)
            .ToList();
    }

    public Transaction Add(Transaction transaction)
    {
        var data = _store.Load();
        
        transaction.Id = data.Transactions.Count == 0 
        ? 1 
        : data.Transactions.Max(t => t.Id) + 1;
        
        data.Transactions.Add(transaction);
        
        _store.Save(data);
        
        return transaction;
    }
}
