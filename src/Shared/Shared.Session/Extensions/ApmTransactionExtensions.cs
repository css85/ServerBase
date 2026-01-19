using Elastic.Apm.Api;
using Shared.Session.Base;

namespace Shared.Session.Extensions
{
    public static class ApmTransactionExtensions
    {
        public static void SetAppInfo(this ITransaction transaction, AppContextServiceBase appContext)
        {
            transaction.SetLabel("AppId", appContext.AppId);
            transaction.SetLabel("Environment", appContext.Environment);
        }

        public static void SetAppInfo(this ITransaction transaction, SessionServiceBase sessionService)
        {
            transaction.SetAppInfo(sessionService.AppContext);
            transaction.SetLabel("ServiceType", sessionService.ServiceTypeString);
            transaction.SetLabel("SessionType", sessionService.SessionTypeString);
        }
    }
}