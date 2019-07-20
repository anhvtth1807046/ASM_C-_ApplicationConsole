using System;
using Assm_T1808a.controller;
using Assm_T1808a.entity;

namespace Assm_T1808a.scenes
{
    public class SHBMenu : IMenu
    {
        public void Menu(IApplicationController controller)
        {
            
            Console.Clear();
            Console.Out.Flush();
            while (true)
            {
                Console.WriteLine("================== NGAN HANG SHB ==================");
                Console.WriteLine("------------------------- * -----------------------");
                Console.WriteLine(
                    $"Chao mung {Program.CurrentLoggedIn.GetType().GetProperty("Username")} den voi ngan hang SHB!");
                Console.WriteLine("1. Rut tien \t \t 2. Gui tien");
                Console.WriteLine("3. Chuyen khoan");
                Console.WriteLine("0. Quay lai");
                Console.WriteLine("Vui long nhap lua chon cua ban: ");
                var choice = Int32.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        controller.WithDraw();
                        break;
                    case 2:
                        controller.Deposit();
                        break;
                    case 3:
                        controller.Transfer();
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Lua chon sai vui long chon lai!");
                        break;
                }
            }
        }

        public bool LoginMenu()
        {
            var controller = new  SHBController();
            
            Console.Clear();
            Console.Out.Flush();
            while (true)
            {
                Console.WriteLine("================== NGAN HANG SHB ==================");
                Console.WriteLine("------------------------- * -----------------------");
                Console.WriteLine("1. Dang nhap \t \t 2. Dang ky");
                Console.WriteLine("0. Quay lai");
                Console.WriteLine("Vui long nhap lua chon cua ban: ");
                int choice = Int32.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        if (!controller.DoLogin())
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                        break;
                    case 2:
                        controller.DoRegister();
                        break;
                    case 0:
                        return false;
                    default:
                        Console.WriteLine("Lua chon sai vui long chon lai!");
                        break;
                }

            }
        }
    }
}