using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddTransactionHandler
{
    public const string TransferToCushion = "Transfer to cushion";
    public const string TransferFromIncome = "Transfer from income";

    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IClock _clock;

    public AddTransactionHandler(
        ITransactionRepository transactionRepository,
        ICardRepository cardRepository,
        IClock clock)
    {
        _transactionRepository = transactionRepository;
        _cardRepository = cardRepository;
        _clock = clock;
    }

    public Transaction Handle(
        TransactionType transactionType,
        decimal amount,
        string category,
        int? cardId,
        DateOnly? date,
        string? note)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be > 0.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new InvalidOperationException("Category cannot be empty.");
        }

        var resolvedCardId = EnsureCardSelectedFallback(cardId, transactionType);

        var card = _cardRepository.GetById(resolvedCardId);
        if (card is null)
        {
            throw new InvalidOperationException("Card not found.");
        }

        var transaction = CreateTransaction(
            resolvedCardId,
            amount,
            category,
            transactionType,
            date,
            note);

        return _transactionRepository.Add(transaction);
    }

    public int EnsureCardSelectedFallback(int? cardId, TransactionType type)
    {
        if (cardId.HasValue)
        {
            var card = _cardRepository.GetById(cardId.Value);
            if (card == null)
            {
                throw new InvalidOperationException("Card not found.");
            }

            return card.Id;
        }

        if (type == TransactionType.Expense)
        {
            var defaultCard = _cardRepository.GetDefaultByDataStore();
            if (defaultCard != null)
            {
                return defaultCard.Id;
            }

            var firstCard = _cardRepository.GetFirst();
            if (firstCard != null)
            {
                return firstCard.Id;
            }

            throw new InvalidOperationException("No cards available.");
        }

        var defaultCardByFlag = _cardRepository.GetDefault();
        if (defaultCardByFlag != null)
        {
            return defaultCardByFlag.Id;
        }

        var firstCardByFlagPath = _cardRepository.GetFirst();
        if (firstCardByFlagPath == null)
        {
            throw new InvalidOperationException("No cards available.");
        }

        return firstCardByFlagPath.Id;
    }

    private Transaction CreateTransaction(
        int cardId,
        decimal amount,
        string category,
        TransactionType transactionType,
        DateOnly? date,
        string? note)
    {
        return new Transaction
        {
            CardId = cardId,
            Amount = amount,
            Category = category,
            Date = date ?? _clock.Today,
            Note = note,
            Type = transactionType
        };
    }

    public int ResolveCardId(int? cardId)
    {
        return EnsureCardSelectedFallback(cardId, TransactionType.Income);
    }

    public Card? FindCushionCardLoose()
    {
        var cards = _cardRepository.GetAll();

        var byFlag = cards.FirstOrDefault(c => c.IsCushion);
        if (byFlag != null)
        {
            return byFlag;
        }

        var exact = cards.FirstOrDefault(c => c.Name == "Financial cushion");
        if (exact != null)
        {
            return exact;
        }

        return cards.FirstOrDefault(c => c.Name.Contains("cushion"));
    }

    public void AddTransferPair(int fromCardId, int cushionCardId, decimal amount, DateOnly? date)
    {
        var transferDate = date ?? _clock.Today;

        _transactionRepository.Add(new Transaction
        {
            CardId = fromCardId,
            Amount = amount,
            Category = TransferToCushion,
            Date = transferDate,
            Note = "auto",
            Type = TransactionType.Expense
        });

        _transactionRepository.Add(new Transaction
        {
            CardId = cushionCardId,
            Amount = amount,
            Category = TransferFromIncome,
            Date = transferDate,
            Note = "auto",
            Type = TransactionType.Income
        });
    }
}