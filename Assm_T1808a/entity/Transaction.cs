using System;

namespace Assm_T1808a.entity
{
    public class Transaction
    {
        public enum ActiveStatus
        {
            Processing = 1,
            Done = 2,
            Reject = 0,
            Deleted = -1,
        }

        public enum TransactionType
        {
            Deposit = 1,
            Withdraw = 2,
            Transfer = 3
        }
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public TransactionType Type { get; set; }
        public double Amount { get; set; }
        public string Content { get; set; }
        public string SenderAccountNumber { get; set; }
        public string ReceiverAccountNumber { get; set; }
        public ActiveStatus Status { get; set; }

        public Transaction()
        {
        }
    }
}