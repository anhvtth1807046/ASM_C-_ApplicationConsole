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
            Console.WriteLine("Rut tien");
        }

        public void Deposit()
        {
            Console.WriteLine("GUi tien");
        }

        public void Transfer()
        {
            Console.WriteLine("Chuyen khoan");
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
            Program.CurrentLoggedIn = shbAccount;
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