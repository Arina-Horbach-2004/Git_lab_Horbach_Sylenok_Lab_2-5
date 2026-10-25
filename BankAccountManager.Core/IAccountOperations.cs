namespace BankAccountManager.Core;

/// <summary>Проведення транзакцій та управління операціями рахунку.</summary>
public interface IAccountOperations
{
    void PerformTransaction(decimal amount, string type);
    void ManageOperations();
}