using System;

namespace Assm_T1808a.entity
{
    public class BlockChain
    {
        public string WalletId { get; set; }
        public string Password { get; set; }
        public double Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public BlockChain()
        {
        }

        public BlockChain(string walletId, string password)
        {
            WalletId = walletId;
            Password = password;
            Balance = 50000;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
        
        
    }
}