using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// Сервіс надсилання сповіщень.
/// 
namespace BankAccountManager.Core
{
    public class NotificationService: INotification
    {
        public void SendTransactionNotification(string message)
        {
            Console.WriteLine($"Сповіщення про транзакцію: {message}");
        }

        public void SendBalanceChangeNotification(decimal balance)
        {
            Console.WriteLine(
                $"Новий баланс рахунку: {balance} грн");
        }
    }
}
