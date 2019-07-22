using System;
using Assm_T1808a.entity;
using Assm_T1808a.util;
using MySql.Data.MySqlClient;

namespace Assm_T1808a.model
{
    public class BlockChainModel : IGiaodichModel
    {
        public object FindByUsernameAndPassword(string walletId, string password)
        {
            var _connection = ConnectionHelper.GetConnection();
            var sqlQuery = "select * from BlockChainAccount where WalletId = @walletId and Password = @password";
            var cmd = new MySqlCommand(sqlQuery, _connection);
            cmd.Parameters.AddWithValue("@walletId", walletId);
            cmd.Parameters.AddWithValue("@password", password);
            var result = cmd.ExecuteReader();
            if (result.Read())
            {
                var account = new BlockChain()
                {
                    WalletId = result.GetString("WalletId"),
                    Balance = result.GetDouble("Balance"),
                    Password = result.GetString("Password"),
                    CreatedAt = result.GetDateTime("CreatedAt"),
                    UpdatedAt = result.GetDateTime("UpdatedAt")
                };
                _connection.Close();
                return account;
            }

            _connection.Close();
            return null;
        }

        public bool UpdateBalance(object currentLoggedInAccount, object historyTransaction)
        {
           var account = (BlockChain) currentLoggedInAccount;
            var transactionBl = (BlockChainTransaction) historyTransaction;
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
                var queryBalance = "select balance from `blockchainaccount` where WalletId = @username";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.WalletId);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.
                if (!balanceReader.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SHBModel.CustomError("Tai khoan không hợp lệ.");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDouble("Balance");
                balanceReader.Close();

                // 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw. 
                if (transactionBl.Type != Transaction.TransactionType.Deposit
                    && transactionBl.Type != Transaction.TransactionType.Withdraw)
                {
                    throw new SHBModel.CustomError("Kiểu giao dịch không hợp lệ.");
                }

                // 2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.
                if (transactionBl.Type == Transaction.TransactionType.Withdraw &&
                    transactionBl.Amount > currentBalance)
                {
                    throw new SHBModel.CustomError("Số tiền bạn rút lớn hơn số tiền còn trong tài khoản, vui lòng thử lại!");
                }

                // 3. Update số dư vào tài khoản.
                // 3.1. Tính toán lại số tiền trong tài khoản.
                if (transactionBl.Type == Transaction.TransactionType.Deposit)
                {
                    currentBalance += transactionBl.Amount;
                }
                else
                {
                    currentBalance -= transactionBl.Amount;
                }

                // 3.2. Update số dư vào database.
                var queryUpdateAccountBalance =
                    "update `shbaccount` set balance = @balance where Username = @username ";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.WalletId);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                var updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();

                // 4. Lưu thông tin transaction vào bảng transaction.
                var queryInsertTransaction = SqlStringUtil.InsertString(transactionBl.GetType());
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, connection);
                cmdInsertTransaction.Parameters.AddWithValue("@TransactionId", transactionBl.TransactionId);
                cmdInsertTransaction.Parameters.AddWithValue("@CreatedAt", transactionBl.CreatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@UpdatedAt", transactionBl.UpdatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@Type", transactionBl.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@Amount", transactionBl.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@SenderWalletId",
                    transactionBl.SenderWalletId);
                cmdInsertTransaction.Parameters.AddWithValue("@ReceiverWalletId",
                    transactionBl.ReceiverWalletId);
                cmdInsertTransaction.Parameters.AddWithValue("@Status", transactionBl.Status);
                var insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1)
                {
                    transactionBegin.Commit();
                    return true;
                }
            }
            catch (SHBModel.CustomError)
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
            var account = (BlockChain) obj;
            Console.WriteLine(sqlQuery);
            var cmd = new MySqlCommand(sqlQuery, _connection);
            cmd.Parameters.AddWithValue("@WalletId", account.WalletId);
            cmd.Parameters.AddWithValue("@Balance", account.Balance);
            cmd.Parameters.AddWithValue("@Password", account.Password);
            cmd.Parameters.AddWithValue("@CreatedAt", account.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", account.UpdatedAt);

            if (cmd.ExecuteNonQuery() == 1)
            {
                _connection.Close();
                return true;
            }

            _connection.Close();
            return false;
        }

        public object GetAccountWithAccountNumber(string walletId)
        {
            var connection = ConnectionHelper.GetConnection();
            var queryString = "select * from `BlockChainAccount` where WalletId = @walletId";
            var cmd = new MySqlCommand(queryString, connection);
            cmd.Parameters.AddWithValue("@walletId", walletId);
            var reader = cmd.ExecuteReader();
            BlockChain account = null;
            if (reader.Read())
            {
                var _walletId = reader.GetString("WalletId");
                var password = reader.GetString("Password");
                var balance = reader.GetDouble("Balance");
                var createdAt = reader.GetDateTime("CreatedAt");
                var updatedAt = reader.GetDateTime("UpdatedAt");
                account = new BlockChain
                {
                    WalletId = _walletId,
                    Password = password,
                    Balance = balance,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt
                };
            }

            reader.Close();
            connection.Close();
            return account;
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

            var historyTransaction = (BlockChainTransaction) historyTransaction_obj;

            // 1. Kiểm tra số dư mới nhất.
            try
            {
                // 1.1 Người gửi.
                var queryBalanceAccountFrom =
                    "select balance from `BlockChainAccount` where WalletId = @walletIdFrom";
                var queryBalanceCommand =
                    new MySqlCommand(queryBalanceAccountFrom, connection);
                queryBalanceCommand.Parameters.AddWithValue("@walletIdFrom", Program._BL_CurrentLoggedIn.WalletId);
                var balanceAccountFrom = queryBalanceCommand.ExecuteReader();
                if (!balanceAccountFrom.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SHBModel.CustomError("Tai khoan không hợp lệ.");
                }

                // Lấy balance hiện tại của ng gửi.
                var currentBalanceAccountFrom = balanceAccountFrom.GetDouble("Balance");
                balanceAccountFrom.Close();

                // 1.2 Người nhận.
                var queryBalanceAccountTo =
                    "select balance from `BlockChainAccount` where WalletId = @walletIdTo";
                var cmd = new MySqlCommand(queryBalanceAccountTo, connection);
                cmd.Parameters.AddWithValue("@walletIdTo", Program._BL_CurrentReceiverAccountNumber.WalletId);
                var balanceAccountTo = cmd.ExecuteReader();
                if (!balanceAccountTo.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SHBModel.CustomError("Tai khoan không hợp lệ.");
                }

                // Lấy balance của người nhận.
                var currentBalanceAccountTo = balanceAccountTo.GetDouble("balance");
                balanceAccountTo.Close();

                // 2. Kiểm tra kiểu transaction type chỉ nhận type là transfer.
                if (historyTransaction.Type != Transaction.TransactionType.Transfer)
                {
                    throw new SHBModel.CustomError("Kiểu giao dịch không hợp lệ!");
                }

                if (historyTransaction.Type == Transaction.TransactionType.Transfer &&
                    historyTransaction.Amount > currentBalanceAccountFrom)
                {
                    throw new SHBModel.CustomError(
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
                    "update `BlockChainAccount` set balance = @balanceFrom where WalletId = @usernameFrom";
                var cmdUpdateAccountBalanceFrom =
                    new MySqlCommand(queryUpdateBalanceAccountFrom, connection);
                cmdUpdateAccountBalanceFrom.Parameters.AddWithValue("@usernameFrom",
                    Program._SHB_CurrentLoggedIn.Username);
                cmdUpdateAccountBalanceFrom.Parameters.AddWithValue("@balanceFrom", currentBalanceAccountFrom);
                var updateAccountFromResult = cmdUpdateAccountBalanceFrom.ExecuteNonQuery();

                // 3.1.2 Người nhận.
                var queryUpdateBalanceAccountTo =
                    "update `BlockChainAccount` set balance = @balanceTo where WalletId = @usernameTo";
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
                cmdInsertTransaction.Parameters.AddWithValue("@TransactionId", historyTransaction.TransactionId);
                cmdInsertTransaction.Parameters.AddWithValue("@Type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@Amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@CreatedAt", historyTransaction.CreatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@UpdatedAt", historyTransaction.UpdatedAt);
                cmdInsertTransaction.Parameters.AddWithValue("@SenderWalletId",
                    historyTransaction.SenderWalletId);
                cmdInsertTransaction.Parameters.AddWithValue("@ReceiverWalletId",
                    historyTransaction.ReceiverWalletId);
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
            catch (SHBModel.CustomError)
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}