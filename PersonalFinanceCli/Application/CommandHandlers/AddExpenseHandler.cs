using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddExpenseHandler
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IClock _clock;

    public AddExpenseHandler(
        ITransactionRepository transactionRepository,
        ICardRepository cardRepository,
        IClock clock)
    {
        _transactionRepository = transactionRepository;
        _cardRepository = cardRepository;
        _clock = clock;
    }

    public Transaction AddExpense(decimal amount, string category, int? cardId, DateOnly? transactionDate, string? note)
{
    if (amount <= 0)
    {
        throw new InvalidOperationException("Amount must be > 0.");
    }

    if (string.IsNullOrWhiteSpace(category))
    {
        throw new InvalidOperationException("Category cannot be empty.");
    }

    var resolvedCardId = ResolveCardId(cardId);

    var transaction = new Transaction
    {
        CardId = resolvedCardId,
        Amount = amount,
        Category = category,
        Date = transactionDate ?? _clock.Today,
        Note = note,
        Type = TransactionType.Expense
    };

    return _transactionRepository.Add(transaction);
}

     private int ResolveCardId(int? cardId)
    {
        if (cardId.HasValue)
        {
            var byId = _cardRepository.GetById(cardId.Value);
            if (byId == null)
            {
                throw new InvalidOperationException("Card not found.");
            }

            return byId.Id;
        }

        var defaultByStore = _cardRepository.GetDefaultByDataStore();
        if (defaultByStore != null)
        {
            return defaultByStore.Id;
        }

        var first = _cardRepository.GetFirst();
        if (first == null)
        {
            throw new InvalidOperationException("No cards available.");
        }

        return first.Id;
    }
}