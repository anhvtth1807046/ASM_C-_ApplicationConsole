using Assm_T1808a.entity;

namespace Assm_T1808a.model
{
    public interface IGiaodichModel
    {
        object FindByUsernameAndPassword(string username, string password);
        bool UpdateBalance(object currentLoggedInAccount, object transaction);
        bool SaveAccount(object obj);
        object GetAccountWithAccountNumber(string stk);
        bool UpdateBalanceWhenTransfer(object historyTransaction);
    }
}