using System;
using System.Collections.Generic;
using System.Configuration;
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
        
        public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
        {
            SessionStateItemCollection sessionList=new SessionStateItemCollection();
           
            HttpStaticObjectsCollection staticObjectsList=new HttpStaticObjectsCollection();
            SessionStateStoreData SessionStateStoreData = new SessionStateStoreData(sessionList, staticObjectsList, timeout);
            return SessionStateStoreData;
        }

        public override void CreateUninitializedItem(System.Web.HttpContext context, string id, int timeout)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void EndRequest(System.Web.HttpContext context)
        {
             //
        }

        public override SessionStateStoreData GetItem(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            throw new NotImplementedException();
        }

        public override SessionStateStoreData GetItemExclusive(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            throw new NotImplementedException();
        }

        public override void InitializeRequest(System.Web.HttpContext context)
        {
           string  pApplicationName =System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;


            //
            // Get <sessionState> configuration element.
            //

            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(pApplicationName);
            SessionStateSection pConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");
            context.Response.Write(pConfig.CookieName);
            
             


        }

        public override void ReleaseItemExclusive(System.Web.HttpContext context, string id, object lockId)
        {
            throw new NotImplementedException();
        }

        public override void RemoveItem(System.Web.HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            throw new NotImplementedException();
        }

        public override void ResetItemTimeout(System.Web.HttpContext context, string id)
        {
            throw new NotImplementedException();
        }

        public override void SetAndReleaseItemExclusive(System.Web.HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            throw new NotImplementedException();
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            throw new NotImplementedException();
        }
    }
}
