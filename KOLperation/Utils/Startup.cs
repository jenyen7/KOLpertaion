using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(KOLperation.Utils.Startup))]

namespace KOLperation.Utils
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 如需如何設定應用程式的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkID=316888
            //app.UseCors(CorsOptions.AllowAll);
            //app.MapSignalR("/signalr", new HubConfiguration { EnableJSONP = true, EnableDetailedErrors = true });
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                map.RunSignalR(new HubConfiguration { EnableJSONP = true, EnableDetailedErrors = true });
            });
        }
    }
}