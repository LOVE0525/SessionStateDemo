using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace SessionStateFactory
{
    public class MySQLSessionState : SessionStateStoreProviderBase
    {

        private SessionStateSection pConfig = null;
        private string ApplicationName = "Web";
        /// <summary>
        /// 2.
        /// 采用当前请求的 HttpContext 实例和当前会话的Timeout 值作为输入，
        /// 并返回带有空ISessionStateItemCollection 对象的新的SessionStateStoreData 对象、一个HttpStaticObjectsCollection 集合和指定的 Timeout值。
        /// 使用 GetSessionStaticObjects 方法可以检索 ASP.NET 应用程序的 HttpStaticObjectsCollection 实例。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(),SessionStateUtility.GetSessionStaticObjects(context),timeout);
        }
        /// <summary>
        /// 从会话数据存储区中检索会话的值和信息，并在请求持续期间锁定数据存储区中的会话项数据。
        /// GetItemExclusive 方法设置几个输出参数值，这些参数值将数据存储区中当前会话状态项的状态通知给执行调用的 SessionStateModule。

///如果数据存储区中未找到任何会话项数据，则GetItemExclusive 方法将 locked 输出参数设置为false，并返回 null。
///这将导致 SessionStateModule调用 CreateNewStoreData 方法来为请求创建一个新的SessionStateStoreData 对象。

///如果在数据存储区中找到会话项数据但该数据已锁定，则GetItemExclusive 方法将 locked 输出参数设置为true，
///将 lockAge 输出参数设置为当前日期和时间与该项锁定日期和时间的差，将 lockId 输出参数设置为从数据存储区中检索的锁定标识符，并返回 null。
///这将导致SessionStateModule 隔半秒后再次调用GetItemExclusive 方法，以尝试检索会话项信息和获取对数据的锁定。如果 lockAge 输出参数的设置值超过ExecutionTimeout 值，
///SessionStateModule 将调用ReleaseItemExclusive 方法以清除对会话项数据的锁定，然后再次调用 GetItemExclusive 方法。

///如果 regenerateExpiredSessionId 属性设置为 true，则 actionFlags 参数用于其 Cookieless 属性为 true 的会话。
///actionFlags 值设置为 InitializeItem (1) 则指示会话数据存储区中的项是需要初始化的新会话。通过调用CreateUninitializedItem 方法可以创建会话数据存储区中未初始化的项。
///如果会话数据存储区中的项已经初始化，则 actionFlags 参数设置为零。

        ///如果提供程序支持无 Cookie 会话，请将 actionFlags 输出参数设置为当前项从会话数据存储区中返回的值。
        ///如果被请求的会话存储项的 actionFlags 参数值等于InitializeItem 枚举值 (1)，
        ///则 GetItemExclusive 方法在设置 actionFlags out 参数之后应将数据存储区中的值设置为零。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        public override void CreateUninitializedItem(System.Web.HttpContext context, string id, int timeout)
        {
            OdbcConnection conn = OpenConn();
            OdbcCommand cmd = new OdbcCommand("INSERT INTO Sessions " +
              " (SessionId, ApplicationName, Created, Expires, " +
              "  LockDate, LockId, Timeout, Locked, SessionItems, Flags) " +
              " Values(?, ?, ?, ?, ?, ? , ?, ?, ?, ?)", conn);
            cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
              255).Value = ApplicationName;
            cmd.Parameters.Add("@Created", OdbcType.DateTime).Value
              = DateTime.Now;
            cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value
              = DateTime.Now.AddMinutes((Double)timeout);
            cmd.Parameters.Add("@LockDate", OdbcType.DateTime).Value
              = DateTime.Now;
            cmd.Parameters.Add("@LockId", OdbcType.Int).Value = 0;
            cmd.Parameters.Add("@Timeout", OdbcType.Int).Value = timeout;
            cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = false;
            cmd.Parameters.Add("@SessionItems", OdbcType.VarChar, 0).Value = "";
            cmd.Parameters.Add("@Flags", OdbcType.Int).Value = 1;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                    throw e;
            }
            finally
            {
                conn.Close();
            }
        }

        public override void Dispose()
        {
            
        }

        public override void EndRequest(System.Web.HttpContext context)
        {
            //
        }
        /// <summary>
        /// 除了不尝试锁定数据存储区中的会话项以外，此方法与GetItemExclusive 方法执行的操作相同。GetItem 方法在 EnableSessionState 属性设置为 ReadOnly 时调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <param name="locked"></param>
        /// <param name="lockAge"></param>
        /// <param name="lockId"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public override SessionStateStoreData GetItem(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actions);
        }

      

        public override SessionStateStoreData GetItemExclusive(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(true, context, id, out locked,out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// 初始化 配置信息   1.
        /// </summary>
        /// <param name="context"></param>
        public override void InitializeRequest(System.Web.HttpContext context)
        {

            string pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;

            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(pApplicationName);
            pConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");
            context.Response.Write(pConfig.CookieName);



        }

        /// <summary>
        /// 重置Session 失效时间  
        /// 采用当前请求的 HttpContext 实例、当前请求的SessionID 值以及当前请求的锁定标识符作为输入，并释放对会话数据存储区中的项的锁定。
        /// 在调用 GetItem 或GetItemExclusive 方法，并且数据存储区指定被请求项已锁定，但锁定时间已超过 ExecutionTimeout 值时会调用此方法。
        /// 此方法清除锁定，释放该被请求项以供其他请求使用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <param name="lockId"></param>
        public override void ReleaseItemExclusive(System.Web.HttpContext context, string id, object lockId)
        {
            OdbcConnection conn = OpenConn();
            OdbcCommand cmd =
              new OdbcCommand("UPDATE Sessions SET Locked = 0, Expires = ? " +
              "WHERE SessionId = ? AND ApplicationName = ? AND LockId = ?", conn);
            cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value =
              DateTime.Now.AddMinutes(pConfig.Timeout.Minutes);
            cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
              255).Value = ApplicationName;
            cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                    throw e;
            }
            finally
            {
                conn.Close();
            }      
        }
        /// <summary>
        /// 此方法在 Abandon 方法被调用时调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <param name="lockId"></param>
        /// <param name="item"></param>
        public override void RemoveItem(System.Web.HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            OdbcConnection conn = OpenConn();
            OdbcCommand cmd = new OdbcCommand("DELETE * FROM Sessions " +
              "WHERE SessionId = ? AND ApplicationName = ? AND LockId = ?", conn);
            cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
              255).Value = ApplicationName;
            cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                    throw e;
            }
            finally
            {
                conn.Close();
            } 
        }

        public override void ResetItemTimeout(System.Web.HttpContext context, string id)
        {
            OdbcConnection conn = OpenConn();
            OdbcCommand cmd =
              new OdbcCommand("UPDATE Sessions SET Expires = ? " +
              "WHERE SessionId = ? AND ApplicationName = ?", conn);
            cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value
              = DateTime.Now.AddMinutes(pConfig.Timeout.Minutes);
            cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
              255).Value = ApplicationName;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
              
                    throw e;
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 采用当前请求的 HttpContext 实例、当前请求的SessionID 值、包含要存储的当前会话值的SessionStateStoreData 对象、
        /// 当前请求的锁定标识符以及指示要存储的数据是属于新会话还是现有会话的值作为输入。
       ///如果 newItem 参数为 true，则SetAndReleaseItemExclusive 方法使用提供的值将一个新项插入到数据存储区中。
       ///否则，数据存储区中的现有项使用提供的值进行更新，并释放对数据的任何锁定。
          ///请注意，只有与提供的 SessionID 值和锁定标识符值匹配的当前应用程序的会话数据才会更新。
         ///调用 SetAndReleaseItemExclusive 方法后，SessionStateModule 调用 ResetItemTimeout 方法来更新会话项数据的过期日期和时间。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="lockId"></param>
        /// <param name="newItem"></param>
        public override void SetAndReleaseItemExclusive(System.Web.HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            // Serialize the SessionStateItemCollection as a string.
             string sessItems = Serialize((SessionStateItemCollection)item.Items);
             OdbcConnection conn = OpenConn();
             OdbcCommand deleteCmd = null;
             OdbcCommand cmd = null;
             if(newItem)//
             {
                 
                 // OdbcCommand to clear an existing expired session if it exists.
                  deleteCmd = new OdbcCommand("DELETE FROM Sessions WHERE SessionId = ? AND ApplicationName = ? AND Expires < ?", conn);
                 deleteCmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id; // session id
                 deleteCmd.Parameters.Add
                   ("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
                 deleteCmd.Parameters.Add
                   ("@Expires", OdbcType.DateTime).Value = DateTime.Now;

                 // OdbcCommand to insert the new session item.
                 cmd = new OdbcCommand("INSERT INTO Sessions " +
                   " (SessionId, ApplicationName, Created, Expires, " +
                   "  LockDate, LockId, Timeout, Locked, SessionItems, Flags) " +
                   " Values(?, ?, ?, ?, ?, ? , ?, ?, ?, ?)", conn);
                 cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
                 cmd.Parameters.Add
                   ("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
                 cmd.Parameters.Add
                   ("@Created", OdbcType.DateTime).Value = DateTime.Now;
                 cmd.Parameters.Add
                   ("@Expires", OdbcType.DateTime).Value = DateTime.Now.AddMinutes((Double)item.Timeout);
                 cmd.Parameters.Add
                   ("@LockDate", OdbcType.DateTime).Value = DateTime.Now;
                 cmd.Parameters.Add("@LockId", OdbcType.Int).Value = 0;
                 cmd.Parameters.Add
                   ("@Timeout", OdbcType.Int).Value = item.Timeout;
                 cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = false;
                 cmd.Parameters.Add
                   ("@SessionItems", OdbcType.VarChar, sessItems.Length).Value = sessItems;
                 cmd.Parameters.Add("@Flags", OdbcType.Int).Value = 0;
               
             }
             else
             {
                 // OdbcCommand to update the existing session item.
                 cmd = new OdbcCommand(
                   "UPDATE Sessions SET Expires = ?, SessionItems = ?, Locked = ? " +
                   " WHERE SessionId = ? AND ApplicationName = ? AND LockId = ?", conn);
                 cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value =
                   DateTime.Now.AddMinutes((Double)item.Timeout);
                 cmd.Parameters.Add("@SessionItems",
                   OdbcType.VarChar, sessItems.Length).Value = sessItems;
                 cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = false;
                 cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
                 cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
                   255).Value = ApplicationName;
                 cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;

             }

             try
             {
                   if (deleteCmd != null)
                   deleteCmd.ExecuteNonQuery();
                   cmd.ExecuteNonQuery();
             }
             catch (Exception)
             {
                 
                 throw;
             }
            finally
             {
                 conn.Close();
             }
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        private OdbcConnection OpenConn()
        {
            ConnectionStringSettings accessCon = System.Configuration.ConfigurationManager.ConnectionStrings["MySQLSessionStateStr"];
            OdbcConnection conn = new OdbcConnection(accessCon.ConnectionString);
            conn.Open();
            return conn;
        }


        //
        // Serialize is called by the SetAndReleaseItemExclusive method to 
        // convert the SessionStateItemCollection into a Base64 string to    
        // be stored in an Access Memo field.
        //
        private string Serialize(SessionStateItemCollection items)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            if (items != null)
                items.Serialize(writer);

            writer.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        //
        // DeSerialize is called by the GetSessionStoreItem method to 
        // convert the Base64 string stored in the Access Memo field to a 
        // SessionStateItemCollection.
        //

        private SessionStateStoreData Deserialize(HttpContext context,
          string serializedItems, int timeout)
        {
            MemoryStream ms =
              new MemoryStream(Convert.FromBase64String(serializedItems));

            SessionStateItemCollection sessionItems =
              new SessionStateItemCollection();

            if (ms.Length > 0)
            {
                BinaryReader reader = new BinaryReader(ms);
                sessionItems = SessionStateItemCollection.Deserialize(reader);
            }

            return new SessionStateStoreData(sessionItems,
              SessionStateUtility.GetSessionStaticObjects(context),
              timeout);
        }

        //
        // GetSessionStoreItem is called by both the GetItem and 
        // GetItemExclusive methods. GetSessionStoreItem retrieves the 
        // session data from the data source. If the lockRecord parameter
        // is true (in the case of GetItemExclusive), then GetSessionStoreItem
        // locks the record and sets a new LockId and LockDate.
        //
        private SessionStateStoreData GetSessionStoreItem(bool lockRecord,HttpContext context,string id, out bool locked,out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            // Initial values for return value and out parameters.
            SessionStateStoreData item = null;
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = false;
            actionFlags = 0;

            // ODBC database connection.
            OdbcConnection conn = OpenConn();
            // OdbcCommand for database commands.
            OdbcCommand cmd = null;
            // DataReader to read database record.
            OdbcDataReader reader = null;
            // DateTime to check if current session item is expired.
            DateTime expires;
            // String to hold serialized SessionStateItemCollection.
            string serializedItems = "";
            // True if a record is found in the database.
            bool foundRecord = false;
            // True if the returned session item is expired and needs to be deleted.
            bool deleteData = false;
            // Timeout value from the data store.
            int timeout = 0;

            try
            {
                conn.Open();

                // lockRecord is true when called from GetItemExclusive and
                // false when called from GetItem.
                // Obtain a lock if possible. Ignore the record if it is expired.
                if (lockRecord)
                {
                    cmd = new OdbcCommand(
                      "UPDATE Sessions SET" +
                      " Locked = ?, LockDate = ? " +
                      " WHERE SessionId = ? AND ApplicationName = ? AND Locked = ? AND Expires > ?", conn);
                    cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = true;
                    cmd.Parameters.Add("@LockDate", OdbcType.DateTime).Value
                      = DateTime.Now;
                    cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
                      255).Value = ApplicationName;
                    cmd.Parameters.Add("@Locked", OdbcType.Int).Value = false;
                    cmd.Parameters.Add
                      ("@Expires", OdbcType.DateTime).Value = DateTime.Now;

                    if (cmd.ExecuteNonQuery() == 0)
                        // No record was updated because the record was locked or not found.
                        locked = true;
                    else
                        // The record was updated.

                        locked = false;
                }

                // Retrieve the current session item information.
                cmd = new OdbcCommand(
                  "SELECT Expires, SessionItems, LockId, LockDate, Flags, Timeout " +
                  "  FROM Sessions " +
                  "  WHERE SessionId = ? AND ApplicationName = ?", conn);
                cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
                cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
                  255).Value = ApplicationName;

                // Retrieve session item data from the data source.
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                while (reader.Read())
                {
                    expires = reader.GetDateTime(0);

                    if (expires < DateTime.Now)
                    {
                        // The record was expired. Mark it as not locked.
                        locked = false;
                        // The session was expired. Mark the data for deletion.
                        deleteData = true;
                    }
                    else
                        foundRecord = true;

                    serializedItems = reader.GetString(1);
                    lockId = reader.GetInt32(2);
                    lockAge = DateTime.Now.Subtract(reader.GetDateTime(3));
                    actionFlags = (SessionStateActions)reader.GetInt32(4);
                    timeout = reader.GetInt32(5);
                }
                reader.Close();


                // If the returned session item is expired, 
                // delete the record from the data source.
                if (deleteData)
                {
                    cmd = new OdbcCommand("DELETE FROM Sessions " +
                      "WHERE SessionId = ? AND ApplicationName = ?", conn);
                    cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar,
                      255).Value = ApplicationName;

                    cmd.ExecuteNonQuery();
                }

                // The record was not found. Ensure that locked is false.
                if (!foundRecord)
                    locked = false;

                // If the record was found and you obtained a lock, then set 
                // the lockId, clear the actionFlags,
                // and create the SessionStateStoreItem to return.
                if (foundRecord && !locked)
                {
                    lockId = (int)lockId + 1;

                    cmd = new OdbcCommand("UPDATE Sessions SET" +
                      " LockId = ?, Flags = 0 " +
                      " WHERE SessionId = ? AND ApplicationName = ?", conn);
                    cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;
                    cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;

                    cmd.ExecuteNonQuery();

                    // If the actionFlags parameter is not InitializeItem, 
                    // deserialize the stored SessionStateItemCollection.
                    if (actionFlags == SessionStateActions.InitializeItem)
                        item = CreateNewStoreData(context, pConfig.Timeout.Minutes);
                    else
                        item = Deserialize(context, serializedItems, timeout);
                }
            }
            catch (OdbcException e)
            {
             
                    throw e;
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }

            return item;
        }


    }
}
