
using LogApp;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Database.DAL
{
    public class AccountDAL
    {
        SinglePoint_CloudEntities SinglePoint_CloudEntities = new SinglePoint_CloudEntities();
        private string TC_APIUrl;
        private bool AllowTrainingCourses;
        JsonDeserializer deserial;
        public AccountDAL()
        {
            TC_APIUrl = ConfigurationManager.AppSettings["TC_URL"];
            AllowTrainingCourses = ConfigurationManager.AppSettings["AllowTrainingCourses"].Trim() == "" ? true : false;
            deserial = new JsonDeserializer();
        }
        public List<Accounts> GetAccounts()
        {
            try
            {
                List<Accounts> accounts = SinglePoint_CloudEntities.Accounts.Where(x => x.IsDeleted == false && x.UserID != "gdddemo").OrderBy(x => x.Organization).ToList();
                //accounts.Select(c => { c.EncryptedAccountId = Cryptography.Cryptography.Encrypt(c.AccountId.ToString()); return c; }).ToList();
                accounts.ForEach(i =>
                {
                    i.EncryptedAccountId = Cryptography.Cryptography.Encrypt(i.AccountId.ToString());
                    i.UserLimit = GetAccountUserLimit(i.TrainingCoursesCompanyId);
                    i.CourseIDs = GetCompanyCourses(i.TrainingCoursesCompanyId);
                });
                return accounts;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return null;
            }
        }
        public int AddOrUpdateAccount(Accounts account)
        {
            try
            {


             

                NewCompanyValues values = new NewCompanyValues();
                account.AccountId = account.EncryptedAccountId != "0" ? Convert.ToInt32(Cryptography.Cryptography.Decrypt(account.EncryptedAccountId)) : Convert.ToInt32(account.EncryptedAccountId);
                var regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
                if (account.Password == null)
                { account.Password = ""; }
                var match = Regex.Match(account.Password, regex);
                if (account.Phone == null)
                { account.Phone = ""; }
                string sourceDBName = "";
                string cloneDBName = "";
                Users LoggedInUser = UserDAL.GetCurrentUser();
                int response = 1;
                bool IsUpdate = true;
                //if (IfUsernameAlreadyExist(account.UserName, account.AccountId))
                //{
                //    response = 2;
                //}
                //else

                if (IfUserIDAlreadyExist(account.UserID, account.AccountId))
                {
                    response = 3;
                }
                else if (IfEmailAlreadyExist(account.Email, account.AccountId))
                {
                    response = 4;
                }
                else if (account.AccountId == 0 && !match.Success)
                {
                    response = 6;
                }
                else if (account.AccountId != 0)
                {
                    //Log4Net.WriteLog("account.AccountId:" + account.AccountId, LogType.GENERALLOG);
                    Accounts accountDetail = SinglePoint_CloudEntities.Accounts.Where(x => x.AccountId == account.AccountId).SingleOrDefault();
                    accountDetail.FirstName = account.FirstName;
                    accountDetail.LastName = account.LastName;
                    accountDetail.Organization = account.Organization;
                    accountDetail.UserName = account.UserName;
                    accountDetail.Email = account.Email;
                    accountDetail.Phone = account.Phone;
                    accountDetail.SupportEmails = account.SupportEmails;
                    accountDetail.UserID = account.UserID;
                    accountDetail.AllowRiskManager = account.AllowRiskManager;
                    accountDetail.AllowBusinessCard = account.AllowBusinessCard;
                    accountDetail.AllowFileRepository = account.AllowFileRepository;
                    accountDetail.AllowDAC6 = account.AllowDAC6;
                    accountDetail.AllowTrainingCourses = account.AllowTrainingCourses;
                    accountDetail.UpdatedOn = DateTime.Now;
                    accountDetail.UpdatedBy = LoggedInUser.UserId;
                    accountDetail.Active = account.Active;
                    accountDetail.CompanyLogo = account.CompanyLogo;
                    accountDetail.allowadmincourseTrainingCourses = account.allowadmincourseTrainingCourses;
                    SinglePoint_CloudEntities.Accounts.AddOrUpdate(accountDetail);
                    SinglePoint_CloudEntities.SaveChanges();
                    // Insert User in Training Courses
                    accountDetail.Name = account.Organization + "$$$" + accountDetail.TrainingCoursesCompanyId;
                    //accountDetail.ID = accountDetail.TrainingCoursesCompanyId;
                    accountDetail.UserLimit = account.UserLimit;
                    accountDetail.CourseIDs = account.CourseIDs;

                    accountDetail.UserAdd = new UserAddModel();
                    accountDetail.UserAdd.Email = account.Email;
                    accountDetail.UserAdd.FirstName = account.FirstName;
                    accountDetail.UserAdd.LastName = account.LastName;
                    accountDetail.UserAdd.Password = "Test";
                    accountDetail.UserAdd.ConfirmPassword = "Test";

                    if (AllowTrainingCourses && account.AllowTrainingCourses)
                    {
                        var json = new JavaScriptSerializer().Serialize(accountDetail);
                        //var client = new RestClient(APIUrl + "api/UserService/EditCompany");
                        var client = new RestClient(TC_APIUrl + "api/UserService/CreateCompany");
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", json, ParameterType.RequestBody);
                        IRestResponse APIresponse = client.Execute(request);

                        int StatusCode = (int)APIresponse.StatusCode;
                        if (StatusCode == 200)
                        {
                            values = deserial.Deserialize<NewCompanyValues>(APIresponse);
                            accountDetail.TrainingCoursesCompanyId = values.CompanyId;
                            SinglePoint_CloudEntities.SaveChanges();
                        }
                    }
                    else
                    {
                        Log4Net.WriteLog("Saving inforamtion without AllowTrainingCourses", LogType.GENERALLOG);
                        accountDetail.TrainingCoursesCompanyId = 0;
                        SinglePoint_CloudEntities.SaveChanges();
                    }
                    ActivityLogDAL.LogActivity("Account information has been updated against ID (" + accountDetail.UserID + ")");
                    response = 2;
                }
                else
                {
                    //Log4Net.WriteLog("Account id is 0, inserting new aacount", LogType.GENERALLOG);
                    string password = account.Password;
                    byte[] passwordSalt = Cryptography.Cryptography.GenerateSalt();
                    account.Password = Cryptography.Cryptography.ComputeHash(account.Password, passwordSalt);
                    account.CreatedOn = DateTime.Now;
                    account.CreatedBy = LoggedInUser.UserId;
                    account.UpdatedOn = DateTime.Now;
                    account.UpdatedBy = LoggedInUser.UserId;

                    // Insert User in Training Courses
                    account.Name = account.Organization;
                    account.UserAvailable = account.UserLimit;
                    account.UserAdd = new UserAddModel();
                    account.UserAdd.Email = account.Email;
                    account.UserAdd.FirstName = account.FirstName;
                    account.UserAdd.LastName = account.LastName;
                    account.UserAdd.Password = password;
                    account.UserAdd.ConfirmPassword = password;

                    int CompanyId = 0;
                    if (AllowTrainingCourses && account.AllowTrainingCourses)
                    {
                        var json = new JavaScriptSerializer().Serialize(account);
                        var client = new RestClient(TC_APIUrl + "api/UserService/CreateCompany");
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", json, ParameterType.RequestBody);
                        IRestResponse APIresponse = client.Execute(request);
                        int StatusCode = (int)APIresponse.StatusCode;
                        if (StatusCode == 200)
                        {
                            values = deserial.Deserialize<NewCompanyValues>(APIresponse);
                        }
                    }
                    account.TrainingCoursesCompanyId = values.CompanyId;
                    account.allowadmincourseTrainingCourses = account.allowadmincourseTrainingCourses;
                    account.CompanyLogo = account.CompanyLogo;
                    SinglePoint_CloudEntities.Accounts.AddOrUpdate(account);
                    SinglePoint_CloudEntities.SaveChanges();
                    ActivityLogDAL.LogActivity("New account created with ID (" + account.UserID + ")");
                    IsUpdate = false;
                }

                // Single Point will be create in any case
                sourceDBName = ConfigurationManager.AppSettings["SinglePointDBName"];
              

                LogApp.Log4Net.WriteLog("SourceDBName:" + sourceDBName ,LogType.GENERALLOG);

                cloneDBName = sourceDBName + "_" + account.AccountId;
                CreateDatabasesForClient(sourceDBName, cloneDBName);

                if (account.AllowRiskManager)
                {
                    sourceDBName = ConfigurationManager.AppSettings["RiskManagerDBName"];
                    cloneDBName = sourceDBName + "_" + account.AccountId;
                    CreateDatabasesForClient(sourceDBName, cloneDBName);
                }
                if (account.AllowFileRepository)
                {
                    sourceDBName = ConfigurationManager.AppSettings["FileRepositoryDBName"];
                    cloneDBName = sourceDBName + "_" + account.AccountId;
                    CreateDatabasesForClient(sourceDBName, cloneDBName);
                }
                if (account.AllowBusinessCard)
                {
                    sourceDBName = ConfigurationManager.AppSettings["BusinessCardDBName"];
                    cloneDBName = sourceDBName + "_" + account.AccountId;
                    CreateDatabasesForClient(sourceDBName, cloneDBName);
                }
                if (account.AllowDAC6)
                {
                    sourceDBName = ConfigurationManager.AppSettings["DAC6DBName"];
                    cloneDBName = sourceDBName + "_" + account.AccountId;
                    CreateDatabasesForClient(sourceDBName, cloneDBName);
                }
                AddOrUpdateUserIntoClientDatabase(account, IsUpdate, values);

                //Log4Net.WriteLog("response:" + response, LogType.GENERALLOG);
                return response;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return 5;
            }
        }
        public bool DeleteAccount(string accountId)
        {
            try
            {
                int decryptedAccountId = Convert.ToInt32(Cryptography.Cryptography.Decrypt(accountId));
                Accounts account = SinglePoint_CloudEntities.Accounts.Where(x => x.AccountId == decryptedAccountId).SingleOrDefault();
                string username = account.UserName;
                account.IsDeleted = true;
                account.Active = false;
                SinglePoint_CloudEntities.Accounts.AddOrUpdate(account);
                SinglePoint_CloudEntities.SaveChanges();
                ActivityLogDAL.LogActivity("Account with ID (" + account.UserID + ") has been deleted");
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }

        public bool IfUserIDAlreadyExist(string userID, int accountId)
        {
            try
            {
                bool userExist = false;
                if (accountId != 0)
                {
                    if (SinglePoint_CloudEntities.Accounts.Where(x => x.UserID == userID && x.AccountId != accountId && x.IsDeleted == false).Count() > 0)
                        userExist = true;
                }
                else
                {
                    if (SinglePoint_CloudEntities.Accounts.Where(x => x.UserID == userID && x.IsDeleted == false).Count() > 0)
                        userExist = true;
                }
                return userExist;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return true;
            }
        }
        public bool IfEmailAlreadyExist(string email, int accountID)
        {
            try
            {
                bool userExist = false;
                if (accountID != 0)
                {
                    if (SinglePoint_CloudEntities.Accounts.Where(x => x.Email == email && x.AccountId != accountID && x.IsDeleted == false).Count() > 0)
                        userExist = true;
                }
                else
                {
                    if (SinglePoint_CloudEntities.Accounts.Where(x => x.Email == email && x.IsDeleted == false).Count() > 0)
                        userExist = true;
                }
                return userExist;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return true;
            }
        }
        private bool CreateDatabasesForClient(string sourceDBName, string cloneDBName)
        {
            bool bReturn = false;
            try
            {

                //Log4Net.WriteLog("sourceDBName: " + sourceDBName, LogType.GENERALLOG);
                //Log4Net.WriteLog("cloneDBName: " + cloneDBName, LogType.GENERALLOG);

                string backUpPath = ConfigurationManager.AppSettings["BackupPath"];
                string cloneDBPath = ConfigurationManager.AppSettings["DataPath"];


                //Log4Net.WriteLog("backUpPath: " + backUpPath, LogType.GENERALLOG);
                //Log4Net.WriteLog("cloneDBPath: " + cloneDBPath, LogType.GENERALLOG);

                //bool sourceDBExist = checkDBExist(sourceDBName);
                //bool cloneDBExist = checkDBExist(cloneDBName);




                //if (sourceDBExist == true)
                {
                    //if (cloneDBExist == false)
                    {
                        bReturn = createCloneDatabase(sourceDBName, cloneDBName, backUpPath, cloneDBPath);
                        //Incase of database on same PC
                        //bool backUp = createBackUp(sourceDBName, backUpPath);
                        //if (backUp == true)
                        //{
                        //    bool restore = restoreDatabase(sourceDBName, backUpPath, cloneDBName, cloneDBPath);
                        //    if (restore == true)
                        //    {
                        //        renameDatabaseLogicalFilesName(sourceDBName, cloneDBName);
                        //    }
                        //}
                    }
                }
                //SqlConnection sqlConnection = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString);

                //DataTable dataTable = new DataTable();
                //sqlConnection.Open();
                //SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("EXEC [dbo].[SP_CloneDatabaseForAccount] " +
                //"@UserId = " + account.AccountId +
                //",@AllowRiskManager = " + account.AllowRiskManager +
                //",@AllowFileRepository = " + account.AllowFileRepository +
                //",@AllowBusinessCard = " + account.AllowBusinessCard +
                //",@AllowDAC6 = " + account.AllowDAC6, sqlConnection);
                //sqlDataAdapter.Fill(dataTable);
                //sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
            }
            return bReturn;
        }
        private bool AddOrUpdateUserIntoClientDatabase(Accounts account, bool IsUpdate, NewCompanyValues values)
        {
            try
            {
                if (!IsUpdate)
                {
                    SqlConnection sqlConnection = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString);

                    DataTable dataTable = new DataTable();
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand("[ManageUserForClientDatabase]", sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("CompanyName", account.Organization);
                    sqlCommand.Parameters.AddWithValue("Filter", "Insert");
                    sqlCommand.Parameters.AddWithValue("FirstName", account.FirstName);
                    sqlCommand.Parameters.AddWithValue("LastName", account.LastName);
                    sqlCommand.Parameters.AddWithValue("Email", account.Email);
                    sqlCommand.Parameters.AddWithValue("Phone", account.Phone);
                    sqlCommand.Parameters.AddWithValue("Username", account.UserName);
                    sqlCommand.Parameters.AddWithValue("Password", account.Password);
                    sqlCommand.Parameters.AddWithValue("ProfileImage", "images/user.png");
                    sqlCommand.Parameters.AddWithValue("AccountId", account.AccountId);
                    sqlCommand.Parameters.AddWithValue("AllowRiskManager", !account.AllowRiskManager);
                    sqlCommand.Parameters.AddWithValue("AllowFileRepository", !account.AllowFileRepository);
                    sqlCommand.Parameters.AddWithValue("AllowBusinessCard", !account.AllowBusinessCard);
                    sqlCommand.Parameters.AddWithValue("AllowDAC6", !account.AllowDAC6);
                    sqlCommand.Parameters.AddWithValue("AllowTrainingCourses", true);
                    sqlCommand.Parameters.AddWithValue("tcuserid", values.UserId);
                    int k = sqlCommand.ExecuteNonQuery();
                    //"@Filter = " + "'Insert'" +
                    //",@FirstName = " + account.FirstName +
                    //",@LastName = " + account.LastName +
                    //",@Email = " + account.Email +
                    //",@Phone = " + account.Phone +
                    //",@Username = " + account.UserName +
                    //",@Password = " + account.Password +
                    //",@ProfileImage = " + "'images/user.png'" +
                    //",@AccountId = " + account.AccountId, sqlConnection);
                    //sqlDataAdapter.Fill(dataTable);
                    sqlConnection.Close();
                }
                else
                {
                    SqlConnection sqlConnection = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString);

                    DataTable dataTable = new DataTable();
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand("[ManageUserForClientDatabase]", sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("CompanyName", account.Organization);
                    sqlCommand.Parameters.AddWithValue("Filter", "Update");
                    sqlCommand.Parameters.AddWithValue("FirstName", account.FirstName);
                    sqlCommand.Parameters.AddWithValue("LastName", account.LastName);
                    sqlCommand.Parameters.AddWithValue("Email", account.Email);
                    sqlCommand.Parameters.AddWithValue("Phone", account.Phone);
                    sqlCommand.Parameters.AddWithValue("Username", account.UserName);
                    sqlCommand.Parameters.AddWithValue("AccountId", account.AccountId);
                    sqlCommand.Parameters.AddWithValue("AllowRiskManager", !account.AllowRiskManager);
                    sqlCommand.Parameters.AddWithValue("AllowFileRepository", !account.AllowFileRepository);
                    sqlCommand.Parameters.AddWithValue("AllowBusinessCard", !account.AllowBusinessCard);
                    sqlCommand.Parameters.AddWithValue("AllowDAC6", !account.AllowDAC6);
                    sqlCommand.Parameters.AddWithValue("AllowTrainingCourses", !account.AllowTrainingCourses);
                    sqlCommand.Parameters.AddWithValue("tcuserid", values.UserId);
                    int k = sqlCommand.ExecuteNonQuery();

                    //SqlConnection sqlConnection = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString);

                    //DataTable dataTable = new DataTable();
                    //sqlConnection.Open();
                    //SqlCommand sqlCommand = new SqlCommand("[ManageUserForClientDatabase] ", sqlConnection);
                    //sqlCommand.CommandType = CommandType.StoredProcedure;
                    //sqlCommand.Parameters.AddWithValue("Filter", "Update");
                    //sqlCommand.Parameters.AddWithValue("FirstName", account.FirstName);
                    //sqlCommand.Parameters.AddWithValue("LastName", account.LastName);
                    //sqlCommand.Parameters.AddWithValue("Email", account.Email);
                    //sqlCommand.Parameters.AddWithValue("Phone", account.Phone);
                    //sqlCommand.Parameters.AddWithValue("Username", account.UserName);
                    //sqlCommand.Parameters.AddWithValue("AccountId", account.AccountId);
                    //"@Filter = " + "Update" +
                    //",@FirstName = " + account.FirstName +
                    //",@LastName = " + account.LastName +
                    //",@Email = " + account.Email +
                    //",@Phone = " + account.Phone +
                    //",@Username = " + account.UserName +
                    //",@AccountId = " + account.AccountId, sqlConnection);
                    sqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }
        public Accounts GetAccountDetail(int accountId)
        {
            try
            {
                Accounts accountDetail = SinglePoint_CloudEntities.Accounts.Where(x => x.AccountId == accountId).SingleOrDefault();
                return accountDetail;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return null;
            }
        }
        private int GetAccountUserLimit(int CompanyId)
        {
            int UserLimit = 0;
            try
            {
                if (CompanyId > 0)
                {
                    var client = new RestClient(TC_APIUrl + "api/UserService/GetUserLimit?CompanyId=" + CompanyId);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    JsonDeserializer deserial = new JsonDeserializer();
                    int StatusCode = (int)response.StatusCode;
                    //Console.WriteLine(response.Content);
                    if (StatusCode == 200)
                    {
                        UserLimit = deserial.Deserialize<int>(response);
                    }
                }
                return UserLimit;
            }
            catch (Exception ex)
            {
                Log4Net.WriteLog("Exception Occur while getting user limit of account", LogType.GENERALLOG);
                return UserLimit;
            }

        }
        private List<int> GetCompanyCourses(int CompanyId)
        {
            List<int> CourseIDs = new List<int>();
            try
            {
                if (CompanyId > 0)
                {
                    var client = new RestClient(TC_APIUrl + "api/UserService/GetCoursesList?CompanyId=" + CompanyId);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    int StatusCode = (int)response.StatusCode;
                    //Console.WriteLine(response.Content);
                    if (StatusCode == 200)
                    {
                        var courses = deserial.Deserialize<List<SelectListItem>>(response);
                        CourseIDs = courses.Select(x => Convert.ToInt32(x.Value)).ToList();
                    }
                }
                return CourseIDs;
            }
            catch (Exception ex)
            {
                Log4Net.WriteLog("Exception Occur while getting user limit of account", LogType.GENERALLOG);
                return CourseIDs;
            }

        }
        public DataSet GetData(string commandText)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(commandText);
                SqlDataAdapter adpt = new SqlDataAdapter();
                DataSet ds = new DataSet();
                int result = -5;

                Log4Net.WriteLog("Connection String: " + SinglePoint_CloudEntities.Database.Connection.ConnectionString, LogType.GENERALLOG);

                using (SqlConnection conn = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    Log4Net.WriteLog("Connection Opened.", LogType.GENERALLOG);

                    adpt.SelectCommand = cmd;

                    cmd.Connection = conn;
                    result = adpt.Fill(ds);

                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    return ds;
                }
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return null;
            }
        }

        public bool checkDBExist(string DBName)
        {
            try
            {
                string CheckDBQuery = "SELECT name FROM master.dbo.sysdatabases where name = '" + DBName + "';";

                Log4Net.WriteLog("Query for Db Existance: " + CheckDBQuery, LogType.GENERALLOG);
                DataSet dataSet = GetData(CheckDBQuery);

                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    string dbName = dataSet.Tables[0].Rows[0]["name"].ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log4Net.WriteLog("Exception While checking database existance", LogType.GENERALLOG);
                Log4Net.WriteException(ex);
                return false;
            }
        }

        public bool createCloneDatabase(string sourceDBName, string cloneDBName, string backUpPath, string restorePath)
        {
            try
            {


                SqlCommand sqlCommand = new SqlCommand("[CreateCloneDatabase]");
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("sourceDBName", sourceDBName);
                sqlCommand.Parameters.AddWithValue("cloneDBName", cloneDBName);
                sqlCommand.Parameters.AddWithValue("backupPath", backUpPath);
                sqlCommand.Parameters.AddWithValue("restorePath", restorePath);

                using (SqlConnection conn = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    sqlCommand.Connection = conn;
                    sqlCommand.ExecuteNonQuery();

                    if (conn.State == ConnectionState.Open)
                        conn.Close();

                }
                return true;

            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }

        }

        public bool createBackUp(string sourceDBName, string backUpPath)
        {
            try
            {
                if (Directory.Exists(backUpPath))
                {
                    if (File.Exists(backUpPath + "/" + sourceDBName + ".bak"))
                    {
                        File.Delete(backUpPath + "/" + sourceDBName + ".bak");
                    }

                    string queryBackUp = "BACKUP DATABASE " + sourceDBName + " TO DISK = '" + backUpPath + "/" + sourceDBName + ".bak' ";

                    SqlCommand cmd = new SqlCommand(queryBackUp);
                    using (SqlConnection conn = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString))
                    {
                        if (conn.State != ConnectionState.Open)
                            conn.Open();

                        cmd.Connection = conn;
                        cmd.ExecuteNonQuery();

                        if (conn.State == ConnectionState.Open)
                            conn.Close();

                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }

        }

        public bool restoreDatabase(string sourceDBName, string backUpPath, string cloneDatabaseName, string cloneDatabasePath)
        {
            try
            {
                if (Directory.Exists(cloneDatabasePath))
                {
                    string queryBackUp = "RESTORE DATABASE " + cloneDatabaseName + " " +
                                     "FROM DISK = '" + backUpPath + "/" + sourceDBName + ".bak' " +
                                     "WITH MOVE '" + sourceDBName + "' TO '" + cloneDatabasePath + "/" + cloneDatabaseName + ".mdf', " +
                                     "MOVE '" + sourceDBName + "_log' TO '" + cloneDatabasePath + "/" + cloneDatabaseName + "_log.ldf' ";

                    SqlCommand cmd = new SqlCommand(queryBackUp);
                    using (SqlConnection conn = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString))
                    {
                        if (conn.State != ConnectionState.Open)
                            conn.Open();

                        cmd.Connection = conn;
                        cmd.ExecuteNonQuery();

                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }

        }

        public void renameDatabaseLogicalFilesName(string sourceDBName, string cloneDatabaseName)
        {
            try
            {
                string queryBackUp = "USE [master]; " +
                                     "ALTER DATABASE[" + cloneDatabaseName + "] MODIFY FILE(NAME = " + sourceDBName + ", NEWNAME = " + cloneDatabaseName + "); " +
                                     "ALTER DATABASE[" + cloneDatabaseName + "] MODIFY FILE(NAME = " + sourceDBName + "_log, NEWNAME = " + cloneDatabaseName + "_log); ";

                SqlCommand cmd = new SqlCommand(queryBackUp);
                using (SqlConnection conn = new SqlConnection(SinglePoint_CloudEntities.Database.Connection.ConnectionString))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    cmd.Connection = conn;
                    cmd.ExecuteNonQuery();

                    if (conn.State == ConnectionState.Open)
                        conn.Close();

                }
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex)
;
            }

        }
        public class NewCompanyValues
        {
            public int CompanyId { get; set; }
            public int UserId { get; set; }
        }
    }
}
