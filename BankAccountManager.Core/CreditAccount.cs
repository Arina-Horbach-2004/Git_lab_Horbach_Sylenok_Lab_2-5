using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public class CreditAccount: ICreditAccount
    {
        private decimal loanBalance;

        public void ApplyForCredit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "Сума кредиту повинна бути більшою за нуль.");

            loanBalance += amount;
        }

        public void RepayCredit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "Сума погашення повинна бути більшою за нуль.");

            if (amount > loanBalance)
                throw new InvalidOperationException(
                    "Сума погашення перевищує залишок кредиту.");

            loanBalance -= amount;
        }

        public decimal GetCreditBalance()
        {
            return loanBalance;
        }

    }
}
