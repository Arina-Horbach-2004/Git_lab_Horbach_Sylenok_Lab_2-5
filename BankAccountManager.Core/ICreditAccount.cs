using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// Інтерфейс управління кредитним рахунком.

namespace BankAccountManager.Core
{
    public interface ICreditAccount
    {
        void ApplyForCredit(decimal amount);

        void RepayCredit(decimal amount);

        decimal GetCreditBalance();
    }
}
