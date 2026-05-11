using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddCardHandler
{
    private readonly ICardRepository _cardRepository;

    public AddCardHandler(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public Card AddCard(string name, string currencyInput, decimal? initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Card name cannot be empty.");
        }

        if (!Enum.TryParse<Currency>(currencyInput, true, out var currency))
        {
            throw new InvalidOperationException("Unknown currency. Allowed: RUB, EUR.");
        }
                   
        var isFirstCard = isFirstCard();
        
        var card = new Card
        {
            Name = name,
            Currency = currency,
            InitialBalance = initialBalance ?? 0m,
            IsDefault = isFirstCard
        };

        return _cardRepository.Add(card);
    }

    private bool IsFirstCard()
    {
        return _cardRepository.GetAll().Count == 0;
    }
}
