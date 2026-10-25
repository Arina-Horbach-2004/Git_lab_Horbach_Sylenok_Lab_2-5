namespace BankAccountManager.Core;

/// <summary>Базові операції з банківським рахунком.</summary>
public interface IBankAccount
{
    void Deposit(decimal amount);
    void Withdraw(decimal amount);
    decimal GetBalance();
}