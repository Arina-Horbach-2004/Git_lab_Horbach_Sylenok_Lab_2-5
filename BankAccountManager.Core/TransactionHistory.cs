namespace BankAccountManager.Core;

/// <summary>
/// Реалізація журналу операцій.
/// </summary>
public class TransactionHistory : ITransactionHistory
{
    private readonly List<string> operations = new();

    public void AddOperation(string details)
    {
        operations.Add(details);
    }

    public IReadOnlyList<string> GetHistory()
    {
        return operations.AsReadOnly();
    }

    public IReadOnlyList<string> SearchOperations(string parameter)
    {
        return operations
            .Where(operation =>
                operation.Contains(
                    parameter,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}