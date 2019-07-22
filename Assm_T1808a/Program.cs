using System;
using System.Data.SqlTypes;
using System.Reflection;
using System.Text;
using Assm_T1808a.entity;
using Assm_T1808a.scenes;
using Assm_T1808a.util;

namespace Assm_T1808a
{
    class Program
    {
        public static SHBAccount _SHB_CurrentLoggedIn;
        public static SHBAccount _SHB_CurrentReceiverAccountNumber;
        public static BlockChain _BL_CurrentLoggedIn;
        public static BlockChain _BL_CurrentReceiverAccountNumber;


        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var giaoDichMenu = new GiaoDich();
            giaoDichMenu.Menu();
        }
    }
}