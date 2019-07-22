using System;
using Assm_T1808a.entity;
using Assm_T1808a.model;

namespace Assm_T1808a.controller
{
    public class BlockChainController : IApplicationController
    {
        private readonly IGiaodichModel _model = new BlockChainModel();

        public void WithDraw()
        {
            Console.Clear();
            Console.Out.Flush();
            // lay lai thong tin moi nhat cua Account;
            Program._BL_CurrentLoggedIn =
                (BlockChain) _model.FindByUsernameAndPassword(Program._BL_CurrentLoggedIn.WalletId, Program._BL_CurrentLoggedIn.Password);
            Console.WriteLine("Rút tiền. \t \t Số dư của bạn: " + Program._BL_CurrentLoggedIn.Balance);
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Vui lòng nhập số tiền bạn muốn rút: ");
            var amount = Double.Parse(Console.ReadLine());
            Console.WriteLine("Lời nhắn: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new BlockChainTransaction()
            {
                TransactionId = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.Withdraw,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Amount = amount,
                SenderWalletId = Program._BL_CurrentLoggedIn.WalletId,
                ReceiverWalletId = Program._BL_CurrentLoggedIn.WalletId,
            };
            Console.WriteLine(_model.UpdateBalance(Program._SHB_CurrentLoggedIn, historyTransaction)
                ? "Giao dịch thành công!"
                : "Giao dịch thất bại, vui lòng thử lại!");
            Program._SHB_CurrentLoggedIn =
                (SHBAccount) _model.FindByUsernameAndPassword(Program._SHB_CurrentLoggedIn.Username,
                    Program._SHB_CurrentLoggedIn.Password);
            Console.WriteLine("Số dư hiện tại: " + Program._SHB_CurrentLoggedIn.Balance);
            Console.WriteLine("Ấn enter để tiếp tục!");
            Console.ReadLine();
        }

        public void Deposit()
        {
            Console.Clear();
            Console.Out.Flush();
            Console.WriteLine("Gửi tiền.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Vui lòng nhập số tiền bạn muốn gửi: ");
            var amount = Double.Parse(Console.ReadLine());
            Console.WriteLine("Lời nhắn: ");
            var content = Console.ReadLine();
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.Deposit,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program._BL_CurrentLoggedIn.WalletId,
                ReceiverAccountNumber = Program._BL_CurrentLoggedIn.WalletId,
                Status = Transaction.ActiveStatus.Done
            };
            Console.WriteLine(_model.UpdateBalance(Program._BL_CurrentLoggedIn, historyTransaction)
                ? "Giao dịch thành công!"
                : "Giao dịch thất bại, và thử lại lần nữa!");
            Program._SHB_CurrentLoggedIn = (SHBAccount) _model.FindByUsernameAndPassword(Program._BL_CurrentLoggedIn.WalletId, Program._BL_CurrentLoggedIn.Password);
            Console.WriteLine("Số tiền hiện tại: " + Program._BL_CurrentLoggedIn.Balance + " vnđ");
            Console.WriteLine("Ấn enter để tiếp tục.");
            Console.ReadLine();
        }

        public void Transfer()
        {
            Console.Clear();
            Console.Out.Flush();
            Program._BL_CurrentLoggedIn = (BlockChain) _model.FindByUsernameAndPassword(Program._BL_CurrentLoggedIn.WalletId, Program._BL_CurrentLoggedIn.Password);
            Console.WriteLine("-------------------------");
            Console.WriteLine("Số dư của bạn :" + Program._BL_CurrentLoggedIn.Balance + " bitcoin");
            Console.WriteLine("-------------------------");
            Console.WriteLine("Vui lòng nhập Wallet Id người nhận.");
            var walletId = Console.ReadLine();
            var account = (BlockChain) _model.GetAccountWithAccountNumber(walletId);
            if (account == null)
            {
                Console.WriteLine($"Khong tim thay tai khoan voi so tai khoan la: {walletId}");
                Console.WriteLine("An enter de tiep tuc!");
                Console.ReadLine();
                return;
            }
            Program._BL_CurrentReceiverAccountNumber = account;
            Console.WriteLine("Thông tin người nhận.");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Wallet Id : " + Program._BL_CurrentReceiverAccountNumber.WalletId);
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Vui lòng nhập số tiền bạn muốn chuyển: ");
            var amount = Double.Parse(Console.ReadLine());
            Console.WriteLine("Lời nhắn: ");
            var content = Console.ReadLine();
            var historyTransaction = new BlockChainTransaction()
            {
                TransactionId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Amount = amount,
                Type = Transaction.TransactionType.Transfer,
                SenderWalletId = Program._BL_CurrentLoggedIn.WalletId,
                ReceiverWalletId = Program._BL_CurrentReceiverAccountNumber.WalletId,
                Status = 1
            };
            Console.WriteLine(_model.UpdateBalanceWhenTransfer(historyTransaction)
                ? "Giao dịch thành công!"
                : "Giao dịch thất bại, và thử lại 1 lần nữa!");
            Program._BL_CurrentLoggedIn = 
                (BlockChain) _model.FindByUsernameAndPassword(Program._BL_CurrentLoggedIn.WalletId, Program._BL_CurrentLoggedIn.Password);
            Console.WriteLine("Số tiền hiện tại: " + Program._BL_CurrentLoggedIn.Balance + " BitCoin");
            Console.WriteLine("Ấn enter để tiếp tục.");
            Console.ReadLine();
        }

        public bool DoLogin()
        {
            Console.Clear();
            Console.Out.Flush();
            // Lấy thông tin từ người dùng nhập vào.
            Console.WriteLine("============= ĐĂNG NHẬP ============");
            Console.WriteLine("Wallet ID: ");
            var username = Console.ReadLine();
            Console.WriteLine("MẬT KHẨU: ");
            var password = Console.ReadLine();
            var account = new BlockChain(username, password);
            
            account = (BlockChain) _model.FindByUsernameAndPassword(username, password);
            if (account == null)
            {
                // Sai thông tin username, trả về thông báo lỗi cụ thể.
                Console.WriteLine("Thông tin tài khoản không hợp lệ, vui lòng thử lại.");
                return false;
            }

            // Login thành công, lưu thông tin vào biến static trong lớp Program.
            Program._BL_CurrentLoggedIn = account;
            return true;
        }

        public bool DoRegister()
        {
            throw new System.NotImplementedException();
        }
    }
}