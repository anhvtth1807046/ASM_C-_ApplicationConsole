using System;
using System.Data.SqlTypes;
using Assm_T1808a.entity;
using Assm_T1808a.util;
using MySql.Data.MySqlClient;

namespace Assm_T1808a.model
{
    public class SHBModel : IGiaodichModel
    {
        public object FindByUsernameAndPassword(string username, string password)
        {
            var _connection = ConnectionHelper.GetConnection();
            var sqlQuery = "select * from SHBAccount where Username = @username and Password = @password";
            var cmd = new MySqlCommand(sqlQuery, _connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            var result = cmd.ExecuteReader();
            if (result.Read())
            {
                var account = new SHBAccount
                {
                    Username = result.GetString("Username"),
                    AccountNumber = result.GetString("AccountNumber"),
                    Balance = result.GetDouble("Balance"),
                    Password = result.GetString("Password"),
                    CreatedAtMLS = result.GetDateTime("CreatedAtMLS"),
                    UpdatedAtMLS = result.GetDateTime("UpdatedAtMLS")
                };
                _connection.Close();
                return account;
            }

            _connection.Close();
            return null;
        }

        public bool UpdateBalance(object currentLoggedInAccount, object historyTransaction)
        {
            var account = (SHBAccount) currentLoggedInAccount;
            var transactionShb = (Transaction) historyTransaction;
            var connection = ConnectionHelper.GetConnection();
            var transactionBegin = connection.BeginTransaction(); // Khởi tạo transaction.

            try
            {
                /**
                 * 1. Lấy thông tin số dư mới nhất của tài khoản.
                 * 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw.
                 *     2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.                 
                 * 3. Update số dư vào tài khoản.
                 *     3.1. Tính toán lại số tiền trong tài khoản.
                 *     3.2. Update số tiền vào database.
                 * 4. Lưu thông tin transaction vào bảng transaction.
                 */

                // 1. Lấy thông tin số dư mới nhất của tài khoản.
                var queryBalance = "select balance from `shbaccount` where Username = @username";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.
                if (!balanceReader.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new CustomError("Tai khoan không hợp lệ.");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDouble("Balance");
                balanceReader.Close();

                // 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw. 
                if (transactionShb.Type != Transaction.TransactionType.Deposit
                    && transactionShb.Type != Transaction.TransactionType.Withdraw)
                {
                    throw new CustomError("Kiểu giao dịch không hợp lệ.");
                }

                // 2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.
                if (transactionShb.Type == Transaction.TransactionType.Withdraw &&
                    transactionShb.Amount > currentBalance)
                {
                    throw new CustomError("Số tiền bạn rút lớn hơn số tiền còn trong tài khoản, vui lòng thử lại!");
                }

                // 3. Update số dư vào tài khoản.
                // 3.1. Tính toán lại số tiền trong tài khoản.
                if (transactionShb.Type == Transaction.TransactionType.Deposit)
                {
                    currentBalance += transactionShb.Amount;
                }
                else
                {
                    currentBalance -= transactionShb.Amount;
                }

                // 3.2. Update số dư vào database.
                var queryUpdateAccountBalance =
                    "update `shbaccount` set balance = @balance where Username = @username ";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.Username);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                var updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();

                // 4. Lưu thông tin transaction vào bảng transaction.
                var queryInsertTransaction = SqlStringUtil.InsertString(transactionShb.GetType());
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, connection);
                cmdInsertTransaction.Parameters.AddWithValue("@Id", transactionShb.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@CreatedAt", transactionShb.CreatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@UpdatedAt", transactionShb.UpdatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@Type", transactionShb.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@Amount", transactionShb.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@Content", transactionShb.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@SenderAccountNumber",
                    transactionShb.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@ReceiverAccountNumber",
                    transactionShb.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@Status", transactionShb.Status);
                var insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1)
                {
                    transactionBegin.Commit();
                    return true;
                }
            }
            catch (CustomError)
            {
                transactionBegin.Rollback();
                return false;
            }

            connection.Close();
            return false;
        }

        public bool SaveAccount(object obj)
        {
            var _connection = ConnectionHelper.GetConnection();
            var sqlQuery = SqlStringUtil.InsertString(obj.GetType());
            var shbAccount = (SHBAccount) obj;
            Console.WriteLine(sqlQuery);
            var cmd = new MySqlCommand(sqlQuery, _connection);
            cmd.Parameters.AddWithValue("@Username", shbAccount.Username);
            cmd.Parameters.AddWithValue("@AccountNumber", shbAccount.AccountNumber);
            cmd.Parameters.AddWithValue("@Balance", shbAccount.Balance);
            cmd.Parameters.AddWithValue("@Password", shbAccount.Password);
            cmd.Parameters.AddWithValue("@CreatedAtMLS", shbAccount.CreatedAtMLS);
            cmd.Parameters.AddWithValue("@UpdatedAtMLS", shbAccount.UpdatedAtMLS);

            if (cmd.ExecuteNonQuery() == 1)
            {
                _connection.Close();
                return true;
            }

            _connection.Close();
            return false;
        }

        public object GetAccountWithAccountNumber(string accountNumber)
        {
            var connection = ConnectionHelper.GetConnection();
            var queryString = "select * from `shbaccount` where AccountNumber = @accountNumber";
            var cmd = new MySqlCommand(queryString, connection);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            var reader = cmd.ExecuteReader();
            SHBAccount shbAccount = null;
            if (reader.Read())
            {
                var username = reader.GetString("Username");
                var password = reader.GetString("Password");
                var accountNumber2 = reader.GetString("AccountNumber");
                var balance = reader.GetDouble("Balance");
                var createdAt = reader.GetDateTime("CreatedAtMLS");
                var updatedAt = reader.GetDateTime("UpdatedAtMLS");
                shbAccount = new SHBAccount(accountNumber2, username, password, balance, createdAt, updatedAt);
            }

            reader.Close();
            connection.Close();
            return shbAccount;
        }

        public bool UpdateBalanceWhenTransfer(object historyTransaction_obj)
        {
            /*
             * 1. Kiểm tra số dư mới nhất.
             *     1.1 Người gửi.
             *     1.2 Người nhận.
             * 2. Kiểm tra kiểu transaction type chỉ nhận type là transfer.
             * 3. Tính toán lại số tiền của người gửi và nhận.
             *     3.1 Update số dư người nhận người gửi lên database.
             *         3.1.1 Người gửi.
             *         3.1.2 Người nhận.
             * 4. Lưu transaction vào bảng transaction trên database
             */
            var connection = ConnectionHelper.GetConnection(); // Đảm bảo rằng đã kết nối đến db thành công.
            var transaction = connection.BeginTransaction(); // Khởi tạo transaction.

            var historyTransaction = (Transaction) historyTransaction_obj;

            // 1. Kiểm tra số dư mới nhất.
            try
            {
                // 1.1 Người gửi.
                var queryBalanceAccountFrom =
                    "select balance from `shbaccount` where Username = @usernameFrom";
                var queryBalanceCommand =
                    new MySqlCommand(queryBalanceAccountFrom, connection);
                queryBalanceCommand.Parameters.AddWithValue("@usernameFrom", Program._SHB_CurrentLoggedIn.Username);
                var balanceAccountFrom = queryBalanceCommand.ExecuteReader();
                if (!balanceAccountFrom.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new CustomError("Tai khoan không hợp lệ.");
                }

                // Lấy balance hiện tại của ng gửi.
                var currentBalanceAccountFrom = balanceAccountFrom.GetDouble("Balance");
                balanceAccountFrom.Close();

                // 1.2 Người nhận.
                var queryBalanceAccountTo =
                    "select balance from `shbaccount` where Username = @usernameTo";
                var cmd = new MySqlCommand(queryBalanceAccountTo, connection);
                cmd.Parameters.AddWithValue("@usernameTo", Program._SHB_CurrentReceiverAccountNumber.Username);
                var balanceAccountTo = cmd.ExecuteReader();
                if (!balanceAccountTo.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new CustomError("Tai khoan không hợp lệ.");
                }

                // Lấy balance của người nhận.
                var currentBalanceAccountTo = balanceAccountTo.GetDouble("balance");
                balanceAccountTo.Close();

                // 2. Kiểm tra kiểu transaction type chỉ nhận type là transfer.
                if (historyTransaction.Type != Transaction.TransactionType.Transfer)
                {
                    throw new CustomError("Kiểu giao dịch không hợp lệ!");
                }

                if (historyTransaction.Type == Transaction.TransactionType.Transfer &&
                    historyTransaction.Amount > currentBalanceAccountFrom)
                {
                    throw new CustomError(
                        "Số tiền bạn muốn chuyển lớn hơn số tiền còn trong tài khoản của bạn, vui lòng thử lại.");
                }

                // 3. Tính toán lại số tiền của người gửi và nhận.
                if (historyTransaction.Type == Transaction.TransactionType.Transfer)
                {
                    currentBalanceAccountFrom -= historyTransaction.Amount;
                    currentBalanceAccountTo += historyTransaction.Amount;
                }

                // 3.1 Update số dư người nhận người gửi lên database.
                // 3.1.1 Người gửi.
                var queryUpdateBalanceAccountFrom =
                    "update `shbaccount` set balance = @balanceFrom where username = @usernameFrom";
                var cmdUpdateAccountBalanceFrom =
                    new MySqlCommand(queryUpdateBalanceAccountFrom, connection);
                cmdUpdateAccountBalanceFrom.Parameters.AddWithValue("@usernameFrom",
                    Program._SHB_CurrentLoggedIn.Username);
                cmdUpdateAccountBalanceFrom.Parameters.AddWithValue("@balanceFrom", currentBalanceAccountFrom);
                var updateAccountFromResult = cmdUpdateAccountBalanceFrom.ExecuteNonQuery();

                // 3.1.2 Người nhận.
                var queryUpdateBalanceAccountTo =
                    "update `shbaccount` set balance = @balanceTo where username = @usernameTo";
                var cmdUpdateBalanceAccountTo =
                    new MySqlCommand(queryUpdateBalanceAccountTo, connection);
                cmdUpdateBalanceAccountTo.Parameters.AddWithValue("@usernameTo",
                    Program._SHB_CurrentReceiverAccountNumber.Username);
                cmdUpdateBalanceAccountTo.Parameters.AddWithValue("@balanceTo", currentBalanceAccountTo);
                var updateAccountToResult = cmdUpdateBalanceAccountTo.ExecuteNonQuery();

                // 4. Lưu thông tin transaction vào bảng transaction.
                var queryInsertTransaction = SqlStringUtil.InsertString(historyTransaction.GetType());
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, connection);
                cmdInsertTransaction.Parameters.AddWithValue("@Id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@Type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@Amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@CreatedAt", historyTransaction.CreatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@UpdatedAt", historyTransaction.UpdatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@Content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@SenderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@ReceiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@Status", historyTransaction.Status);
                var insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountFromResult == 1 && updateAccountToResult == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }

                connection.Close();
                return false;
            }
            catch (CustomError)
            {
                transaction.Rollback();
                return false;
            }
        }

        public class CustomError : Exception
        {
            public CustomError(string message) : base(message)
            {
            }
        }
    }
}