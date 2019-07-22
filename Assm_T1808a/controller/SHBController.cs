using System;
using System.Security.Policy;
using Assm_T1808a.entity;
using Assm_T1808a.model;

namespace Assm_T1808a.controller
{
    public class SHBController : IApplicationController
    {
        private readonly IGiaodichModel _model = new SHBModel();

        public void WithDraw()
        {
            Console.Clear();
            Console.Out.Flush();
            // lay lai thong tin moi nhat cua Account;
            Program._SHB_CurrentLoggedIn =
                (SHBAccount) _model.FindByUsernameAndPassword(Program._SHB_CurrentLoggedIn.Username,
                    Program._SHB_CurrentLoggedIn.Password);
            Console.WriteLine("Rút tiền. \t \t Số dư của bạn: " + Program._SHB_CurrentLoggedIn.Balance);
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Vui lòng nhập số tiền bạn muốn rút: ");
            var amount = Double.Parse(Console.ReadLine());
            Console.WriteLine("Lời nhắn: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.Withdraw,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program._SHB_CurrentLoggedIn.AccountNumber,
                ReceiverAccountNumber = Program._SHB_CurrentLoggedIn.AccountNumber,
                Status = Transaction.ActiveStatus.Done
            };
            Console.WriteLine(_model.UpdateBalance(Program._SHB_CurrentLoggedIn, historyTransaction)
                ? "Giao dịch thành công!"
                : "Giao dịch thất bại, vui lòng thử lại!");
            Program._SHB_CurrentLoggedIn = (SHBAccount) _model.FindByUsernameAndPassword(Program._SHB_CurrentLoggedIn.Username, Program._SHB_CurrentLoggedIn.Password);
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
                SenderAccountNumber = Program._SHB_CurrentLoggedIn.AccountNumber,
                ReceiverAccountNumber = Program._SHB_CurrentLoggedIn.AccountNumber,
                Status = Transaction.ActiveStatus.Done
            };
            Console.WriteLine(_model.UpdateBalance(Program._SHB_CurrentLoggedIn, historyTransaction)
                ? "Giao dịch thành công!"
                : "Giao dịch thất bại, và thử lại lần nữa!");
            Program._SHB_CurrentLoggedIn = (SHBAccount) _model.FindByUsernameAndPassword(Program._SHB_CurrentLoggedIn.Username, Program._SHB_CurrentLoggedIn.Password);
            Console.WriteLine("Số tiền hiện tại: " + Program._SHB_CurrentLoggedIn.Balance + " vnđ");
            Console.WriteLine("Ấn enter để tiếp tục.");
            Console.ReadLine();
        }

        public void Transfer()
        {
            Console.Clear();
            Console.Out.Flush();
            Program._SHB_CurrentLoggedIn = (SHBAccount) _model.FindByUsernameAndPassword(Program._SHB_CurrentLoggedIn.Username, Program._SHB_CurrentLoggedIn.Password);
            Console.WriteLine("-------------------------");
            Console.WriteLine("Số dư của bạn :" + Program._SHB_CurrentLoggedIn.Balance + " vnđ");
            Console.WriteLine("-------------------------");
            Console.WriteLine("Vui lòng nhập số tài khoản người nhận.");
            var stk = Console.ReadLine();
            var account = (SHBAccount) _model.GetAccountWithAccountNumber(stk);
            if (account == null)
            {
                Console.WriteLine($"Khong tim thay tai khoan voi so tai khoan la: {stk}");
                Console.WriteLine("An enter de tiep tuc!");
                Console.ReadLine();
                return;
            }
            Program._SHB_CurrentReceiverAccountNumber = account;
            Console.WriteLine("Thông tin người nhận.");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Họ tên: " + Program._SHB_CurrentReceiverAccountNumber.Username);
            Console.WriteLine("Số tài khoản: " + Program._SHB_CurrentReceiverAccountNumber.AccountNumber);
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Vui lòng nhập số tiền bạn muốn chuyển: ");
            var amount = Double.Parse(Console.ReadLine());
            Console.WriteLine("Lời nhắn: ");
            var content = Console.ReadLine();
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.Transfer,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program._SHB_CurrentLoggedIn.AccountNumber,
                ReceiverAccountNumber = Program._SHB_CurrentReceiverAccountNumber.AccountNumber,
                Status = Transaction.ActiveStatus.Done
            };
            Console.WriteLine(_model.UpdateBalanceWhenTransfer(historyTransaction)
                ? "Giao dịch thành công!"
                : "Giao dịch thất bại, và thử lại 1 lần nữa!");
            Program._SHB_CurrentLoggedIn = (SHBAccount) _model.FindByUsernameAndPassword(Program._SHB_CurrentLoggedIn.Username, Program._SHB_CurrentLoggedIn.Password);
            Console.WriteLine("Số tiền hiện tại: " + Program._SHB_CurrentLoggedIn.Balance + " vnđ");
            Console.WriteLine("Ấn enter để tiếp tục.");
            Console.ReadLine();
        }

        public bool DoLogin()
        {
            Console.Clear();
            Console.Out.Flush();
            // Lấy thông tin từ người dùng nhập vào.
            Console.WriteLine("============= ĐĂNG NHẬP ============");
            Console.WriteLine("TÀI KHOẢN: ");
            var username = Console.ReadLine();
            Console.WriteLine("MẬT KHẨU: ");
            var password = Console.ReadLine();
            var shbAccount = new SHBAccount(username, password);
            // Bắt đầu kiểm tra Valid user và password length khác null và lớn hơn 0.
            var errors = shbAccount.ValidLoginInformation();
            if (errors.Count > 0)
            {
                Console.WriteLine("Vui lòng kiểm tra lại.");
                foreach (var messagErrorsValue in errors.Values)
                {
                    Console.Error.WriteLine(messagErrorsValue);
                }

                Console.ReadLine();
                return false;
            }

            shbAccount = (SHBAccount) _model.FindByUsernameAndPassword(username, password);
            if (shbAccount == null)
            {
                // Sai thông tin username, trả về thông báo lỗi cụ thể.
                Console.WriteLine("Thông tin tài khoản không hợp lệ, vui lòng thử lại.");
                return false;
            }

            // Login thành công, lưu thông tin vào biến static trong lớp Program.
            Program._SHB_CurrentLoggedIn = shbAccount;
            return true;
        }

        public bool DoRegister()
        {
            Console.Clear();
            Console.Out.Flush();
            Console.WriteLine("Nhập thông tin tài khoản.");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Tài khoản: ");
            var username = Console.ReadLine();
            Console.WriteLine("Mật khẩu: ");
            var password = Console.ReadLine();
            var shbAccount = new SHBAccount(username, password);
            var errors = shbAccount.CheckValid();
            if (errors.Count == 0 && _model.FindByUsernameAndPassword(username, password) == null)
            {
                if (!_model.SaveAccount(shbAccount))
                {
                    return false;
                }

                Console.WriteLine("Đăng ký thành công.");
                Console.WriteLine("--------------------------------");
                int countLoop = 0;
                while (true)
                {
                    if (countLoop > 1)
                    {
                        Console.Clear();
                        Console.Out.Flush();
                    }

                    Console.WriteLine("Bạn có muốn đăng nhập không? Y/N");
                    Console.WriteLine("Vui lòng nhập lựa chọn của bạn:");
                    var choice = Console.ReadLine();
                    if (choice != null && choice.Equals("N"))
                    {
                        DoLogin();
                        break;
                    }

                    if (choice != null && choice.Equals("Y"))
                    {
                        return false;
                    }

                    Console.WriteLine("Lựa chọn không hợp lệ, vui lòng thử lại.");
                }
            }
            else
            {
                Console.Error.WriteLine("Thông tin tài khoản không hợp lệ, vui lòng thử lại.");
                foreach (var messagErrorsValue in errors.Values)
                {
                    Console.Error.WriteLine(messagErrorsValue);
                }

                Console.ReadLine();
            }

            return false;
        }
    }
}