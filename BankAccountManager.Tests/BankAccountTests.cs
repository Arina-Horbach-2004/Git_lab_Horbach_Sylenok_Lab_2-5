using BankAccountManager.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BankAccountManager.Tests
{
    [TestClass]
    public class BankAccountTests
    {
        // залежний клас (із заглушками Moq)
        private BankAccount account = null!;
        private Mock<INotification> mockNotification = null!;
        private Mock<ITransactionHistory> mockHistory = null!;

        // незалежні класи (без підміни)
        private TransactionHistory history = null!;
        private CreditAccount credit = null!;
        private NotificationService service = null!;

        // перехоплення виведення в консоль для NotificationService
        private StringWriter output = null!;
        private TextWriter originalOutput = null!;

        [TestInitialize]
        public void Setup()
        {
            mockNotification = new Mock<INotification>();
            mockHistory = new Mock<ITransactionHistory>();
            account = new BankAccount(mockNotification.Object, mockHistory.Object);

            history = new TransactionHistory();
            credit = new CreditAccount();
            service = new NotificationService();

            originalOutput = Console.Out;
            output = new StringWriter();
            Console.SetOut(output);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Console.SetOut(originalOutput);
            output.Dispose();
        }

        #region BankAccount (залежний клас, заглушки Moq)

        // ---------- Constructor Injection ----------

        [TestMethod]
        public void Constructor_NullNotification_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new BankAccount(null!, mockHistory.Object));
        }

        [TestMethod]
        public void Constructor_NullHistory_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new BankAccount(mockNotification.Object, null!));
        }

        // ---------- Баланс ----------

        [TestMethod]
        public void Deposit_IncreasesBalance()
        {
            account.Deposit(1000);
            Assert.AreEqual(1000, account.GetBalance());
        }

        [TestMethod]
        public void Withdraw_DecreasesBalance()
        {
            account.Deposit(1000);
            account.Withdraw(300);
            Assert.AreEqual(700, account.GetBalance());
        }

        [TestMethod]
        public void Withdraw_TooMuch_ThrowsAndKeepsBalance()
        {
            account.Deposit(500);

            Assert.ThrowsException<InvalidOperationException>(() => account.Withdraw(1000));
            Assert.AreEqual(500, account.GetBalance());
        }

        [TestMethod]
        public void Deposit_NonPositiveAmount_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => account.Deposit(0));
            Assert.ThrowsException<ArgumentException>(() => account.Deposit(-10));
            Assert.AreEqual(0, account.GetBalance());
        }

        [TestMethod]
        public void Withdraw_NonPositiveAmount_ThrowsArgumentException()
        {
            account.Deposit(500);

            Assert.ThrowsException<ArgumentException>(() => account.Withdraw(0));
            Assert.ThrowsException<ArgumentException>(() => account.Withdraw(-10));
            Assert.AreEqual(500, account.GetBalance());
        }

        // ---------- PerformTransaction ----------

        [TestMethod]
        public void PerformTransaction_Deposit_WorksCorrectly()
        {
            account.PerformTransaction(200, "deposit");
            Assert.AreEqual(200, account.GetBalance());
        }

        [TestMethod]
        public void PerformTransaction_Withdraw_WorksCorrectly()
        {
            account.Deposit(500);
            account.PerformTransaction(100, "withdraw");
            Assert.AreEqual(400, account.GetBalance());
        }

        [TestMethod]
        public void PerformTransaction_TypeInDifferentCase_WorksCorrectly()
        {
            account.PerformTransaction(200, "DEPOSIT");
            Assert.AreEqual(200, account.GetBalance());
        }

        [TestMethod]
        public void PerformTransaction_UnknownType_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(
                () => account.PerformTransaction(100, "transfer"));
            Assert.AreEqual(0, account.GetBalance());
        }

        // ---------- Взаємодія із залежностями ----------

        [TestMethod]
        public void Deposit_CallsBalanceChangeNotification()
        {
            account.Deposit(1000);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1000), Times.Once);
        }

        [TestMethod]
        public void Withdraw_CallsTransactionNotification()
        {
            account.Deposit(500);
            account.Withdraw(200);

            mockNotification.Verify(
                n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Зняття"))),
                Times.Once);
        }

        [TestMethod]
        public void Deposit_AddsOperationToHistory()
        {
            account.Deposit(300);

            mockHistory.Verify(
                h => h.AddOperation(It.Is<string>(s => s.Contains("Поповнення"))),
                Times.Once);
        }

        [TestMethod]
        public void Withdraw_AddsOperationToHistory()
        {
            account.Deposit(500);
            account.Withdraw(200);

            mockHistory.Verify(
                h => h.AddOperation(It.Is<string>(s => s.Contains("Зняття"))),
                Times.Once);
        }

        [TestMethod]
        public void Withdraw_TooMuch_DoesNotWriteHistoryOrNotify()
        {
            Assert.ThrowsException<InvalidOperationException>(() => account.Withdraw(100));

            mockHistory.Verify(h => h.AddOperation(It.IsAny<string>()), Times.Never);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(It.IsAny<decimal>()), Times.Never);
        }

        #endregion

        #region TransactionHistory (незалежний клас)

        [TestMethod]
        public void TransactionHistory_GetHistory_NewHistory_IsEmpty()
        {
            Assert.AreEqual(0, history.GetHistory().Count);
        }

        [TestMethod]
        public void TransactionHistory_AddOperation_AddsRecordToHistory()
        {
            history.AddOperation("Поповнення: 100 грн");

            Assert.AreEqual(1, history.GetHistory().Count);
            Assert.AreEqual("Поповнення: 100 грн", history.GetHistory()[0]);
        }

        [TestMethod]
        public void TransactionHistory_AddOperation_PreservesOrder()
        {
            history.AddOperation("Перша");
            history.AddOperation("Друга");
            history.AddOperation("Третя");

            Assert.AreEqual(3, history.GetHistory().Count);
            Assert.AreEqual("Перша", history.GetHistory()[0]);
            Assert.AreEqual("Друга", history.GetHistory()[1]);
            Assert.AreEqual("Третя", history.GetHistory()[2]);
        }

        [TestMethod]
        public void TransactionHistory_SearchOperations_ReturnsOnlyMatchingRecords()
        {
            history.AddOperation("Поповнення: 500 грн");
            history.AddOperation("Зняття: 200 грн");
            history.AddOperation("Поповнення: 300 грн");

            var result = history.SearchOperations("Поповнення");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Поповнення: 500 грн", result[0]);
            Assert.AreEqual("Поповнення: 300 грн", result[1]);
        }

        [TestMethod]
        public void TransactionHistory_SearchOperations_IsCaseInsensitive()
        {
            history.AddOperation("Поповнення: 500 грн");

            var result = history.SearchOperations("поповнення");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void TransactionHistory_SearchOperations_NoMatches_ReturnsEmptyList()
        {
            history.AddOperation("Поповнення: 500 грн");

            var result = history.SearchOperations("Кредит");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void TransactionHistory_SearchOperations_EmptyHistory_ReturnsEmptyList()
        {
            var result = history.SearchOperations("Поповнення");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void TransactionHistory_SearchOperations_EmptyString_ReturnsAllRecords()
        {
            history.AddOperation("Поповнення: 500 грн");
            history.AddOperation("Зняття: 200 грн");

            var result = history.SearchOperations(string.Empty);

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void TransactionHistory_SearchOperations_Null_ThrowsArgumentNullException()
        {
            history.AddOperation("Поповнення: 500 грн");

            Assert.ThrowsException<ArgumentNullException>(
                () => history.SearchOperations(null!));
        }

        #endregion

        #region CreditAccount (незалежний клас)

        [TestMethod]
        public void CreditAccount_GetCreditBalance_NewAccount_IsZero()
        {
            Assert.AreEqual(0, credit.GetCreditBalance());
        }

        [TestMethod]
        public void CreditAccount_ApplyForCredit_IncreasesCreditBalance()
        {
            credit.ApplyForCredit(5000);

            Assert.AreEqual(5000, credit.GetCreditBalance());
        }

        [TestMethod]
        public void CreditAccount_ApplyForCredit_TwiceAccumulatesDebt()
        {
            credit.ApplyForCredit(5000);
            credit.ApplyForCredit(2000);

            Assert.AreEqual(7000, credit.GetCreditBalance());
        }

        [TestMethod]
        public void CreditAccount_RepayCredit_DecreasesCreditBalance()
        {
            credit.ApplyForCredit(5000);
            credit.RepayCredit(2000);

            Assert.AreEqual(3000, credit.GetCreditBalance());
        }

        [TestMethod]
        public void CreditAccount_RepayCredit_FullAmount_ResetsBalanceToZero()
        {
            credit.ApplyForCredit(5000);
            credit.RepayCredit(5000);

            Assert.AreEqual(0, credit.GetCreditBalance());
        }

        [TestMethod]
        public void CreditAccount_ApplyForCredit_ZeroAmount_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => credit.ApplyForCredit(0));
        }

        [TestMethod]
        public void CreditAccount_ApplyForCredit_NegativeAmount_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => credit.ApplyForCredit(-100));
        }

        [TestMethod]
        public void CreditAccount_RepayCredit_ZeroAmount_ThrowsArgumentException()
        {
            credit.ApplyForCredit(1000);

            Assert.ThrowsException<ArgumentException>(() => credit.RepayCredit(0));
        }

        [TestMethod]
        public void CreditAccount_RepayCredit_NegativeAmount_ThrowsArgumentException()
        {
            credit.ApplyForCredit(1000);

            Assert.ThrowsException<ArgumentException>(() => credit.RepayCredit(-50));
        }

        [TestMethod]
        public void CreditAccount_RepayCredit_MoreThanDebt_ThrowsAndKeepsBalance()
        {
            credit.ApplyForCredit(1000);

            Assert.ThrowsException<InvalidOperationException>(() => credit.RepayCredit(1500));
            Assert.AreEqual(1000, credit.GetCreditBalance());
        }

        [TestMethod]
        public void CreditAccount_RepayCredit_WithoutCredit_ThrowsInvalidOperationException()
        {
            Assert.ThrowsException<InvalidOperationException>(() => credit.RepayCredit(100));
        }

        #endregion

        #region NotificationService (незалежний клас)

        [TestMethod]
        public void NotificationService_SendTransactionNotification_WritesTransactionText()
        {
            service.SendTransactionNotification("Поповнення: 100 грн");

            StringAssert.Contains(output.ToString(), "Сповіщення про транзакцію: Поповнення: 100 грн");
        }

        [TestMethod]
        public void NotificationService_SendBalanceChangeNotification_WritesNewBalanceWithCurrency()
        {
            service.SendBalanceChangeNotification(1500);

            StringAssert.Contains(output.ToString(), "Новий баланс рахунку: 1500 грн");
        }

        [TestMethod]
        public void NotificationService_SendTransactionNotification_EmptyMessage_DoesNotThrow()
        {
            service.SendTransactionNotification(string.Empty);

            StringAssert.Contains(output.ToString(), "Сповіщення про транзакцію:");
        }

        #endregion
    }
}
