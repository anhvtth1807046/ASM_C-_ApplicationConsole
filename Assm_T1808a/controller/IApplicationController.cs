namespace Assm_T1808a.controller
{
    public interface IApplicationController
    {
        void WithDraw();
        void Deposit();
        void Transfer();
        bool DoLogin();
        bool DoRegister();
    }
}