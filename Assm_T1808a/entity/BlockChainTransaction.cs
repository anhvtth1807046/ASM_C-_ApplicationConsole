using System;

namespace Assm_T1808a.entity
{
    public class BlockChainTransaction
    {
        public string TransactionId { get; set; }
        public string SenderWalletId { get; set; }
        public string ReceiverWalletId { get; set; }
        public double Amount { get; set; }
        public Transaction.TransactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }

        public BlockChainTransaction()
        {
        }
    }
}