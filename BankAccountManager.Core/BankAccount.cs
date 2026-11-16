using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public class BankAccount : IBankAccount, IAccountOperations
    {
        private decimal balance;

        private readonly INotification notificationService;
        private readonly ITransactionHistory transactionHistory;

        public BankAccount(
            INotification notificationService,
            ITransactionHistory transactionHistory)
        {
            this.notificationService = notificationService
                ?? throw new ArgumentNullException(nameof(notificationService));

            this.transactionHistory = transactionHistory
                ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "Сума поповнення повинна бути більшою за нуль.");

            balance += amount;

            AddTransaction($"Поповнення: {amount} грн");

            notificationService.SendBalanceChangeNotification(balance);
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "Сума зняття повинна бути більшою за нуль.");

            if (amount > balance)
                throw new InvalidOperationException(
                    "Недостатньо коштів!");

            balance -= amount;

            AddTransaction($"Зняття: {amount} грн");

            notificationService.SendBalanceChangeNotification(balance);
        }

        public decimal GetBalance()
        {
            return balance;
        }

        public void PerformTransaction(decimal amount, string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "deposit":
                    Deposit(amount);
                    break;

                case "withdraw":
                    Withdraw(amount);
                    break;

                default:
                    throw new ArgumentException(
                        "Невідомий тип транзакції.");
            }
        }

        public void ManageOperations()
        {
            Console.WriteLine(
                "Операції рахунку керуються через інші методи.");
        }

        private void AddTransaction(string details)
        {
            transactionHistory.AddOperation(details);

            notificationService.SendTransactionNotification(details);
        }
    }

}