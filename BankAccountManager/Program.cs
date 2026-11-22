using BankAccountManager.Core;

var notification = new NotificationService();

var history = new TransactionHistory();

var account = new BankAccount(
    notification,
    history);

account.Deposit(1000);

account.Withdraw(300);

Console.WriteLine(
    $"Баланс: {account.GetBalance()} грн");

Console.WriteLine("Історія операцій:");

foreach (var operation in history.GetHistory())
{
    Console.WriteLine(operation);
}