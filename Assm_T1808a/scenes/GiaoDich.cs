using System;
using Assm_T1808a.controller;

namespace Assm_T1808a.scenes
{
    public class GiaoDich
    {
        public void Menu()
        {
            while (true)
            {
                IApplicationController controller = null;
                IMenu menu = null;
                Console.WriteLine("============= GIAO DICH =============");
                Console.WriteLine("1. Ngan hang SHB");
                Console.WriteLine("2. Block Chain");
                Console.WriteLine("0. Thoat");
                Console.WriteLine("Vui long nhap lua chon cua ban: ");

                var choice = Int32.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        controller = new SHBController();
                        menu = new SHBMenu();
                        break;
                    case 2:
                        controller = new BlockChainController();
                        menu = new BlockChainMenu();
                        break;
                    case 0:
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine("Lua co sai vui long chon lai!");
                        break;
                }

                // check neu menu ko null thi chay tiep. ALT + ENTER may no tu sinh code chu e ko code :D
                menu?.Menu(controller);
            }
        }
    }
}