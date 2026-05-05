using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddIncomeHandler
{
    private readonly AddTransactionHandler _addTransactionHandler;

    public AddIncomeHandler(AddTransactionHandler addTransactionHandler)
    {
        _addTransactionHandler = addTransactionHandler;
    }

    public Transaction Addincome(decimal amount, string category, int? cardId, DateOnly? transactionDate, string? note)
    {
        var incomeTransaction = _addTransactionHandler.Handle(
             TransactionType.Income,
             amount,
             category,
             cardId,
             transactionDate,
             note
    );
             return incomeTransaction;
    }
}