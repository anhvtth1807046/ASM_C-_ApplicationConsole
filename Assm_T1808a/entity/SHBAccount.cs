using System;
using System.Collections.Generic;

namespace Assm_T1808a.entity
{
    public class SHBAccount
    {
        [IgnoreReflect]
        public string AccountNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public double Balance { get; set; }
        [IgnoreReflect]
        public DateTime CreatedAtMLS { get; set; }
        [IgnoreReflect]
        public DateTime UpdatedAtMLS { get; set; }

        public SHBAccount()
        {
        }
        
        public SHBAccount(string username, string password)
        {
            AccountNumber = Guid.NewGuid().ToString();
            Username = username;
            Password = password;
            Balance = 50000;
            CreatedAtMLS = DateTime.Now;
            UpdatedAtMLS = DateTime.Now;
        }

        public Dictionary<string, string> ValidLoginInformation()
        {
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(Username))
            {
                errors.Add("username", "Tai khoan khong duoc de trong.");
            }            
            if (string.IsNullOrEmpty(Password))
            {
                errors.Add("password", "mat khu khong duoc de trong.");
            }           
            return errors;
        }

        public Dictionary<string, string> CheckValid()
        {
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(Username))
            {
                errors.Add("username", "Tài khoản không được để trống.");
            }
            else if (Username.Length < 6)
            {
                errors.Add("username", "Tài khoản quá ngắn");
            }

            if (string.IsNullOrEmpty(Password))
            {
                errors.Add("password", "Mật khẩu không được để trống.");
            }
            return errors;
        }
    }

    public class IgnoreReflectAttribute : Attribute
    {
    }
}