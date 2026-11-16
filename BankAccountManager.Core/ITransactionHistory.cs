namespace BankAccountManager.Core;

/// <summary>
/// Інтерфейс для роботи з журналом операцій.
/// </summary>
public interface ITransactionHistory
{
    void AddOperation(string details);

    IReadOnlyList<string> GetHistory();

    IReadOnlyList<string> SearchOperations(string parameter);
}