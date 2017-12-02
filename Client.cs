using System;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http.Headers;
using System.Text;

namespace SHIELD_Sharp
{

 /// <summary>
 /// Basic DelegatingHandler that creates an OAuth authorization header based on the OAuthBase
 /// class downloaded from http://oauth.net
 /// </summary>

public class RPCMessageHandler : DelegatingHandler
 {
     public RPCMessageHandler(HttpMessageHandler innerHandler)   : base(innerHandler)
     {

     }
     protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
     {
        request.Headers.Host = (string)Client.OptDict["host"];
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", (string)Client.OptDict["user"] + ":" + (string)Client.OptDict["pass"]);
        return base.SendAsync(request, cancellationToken);
     }
 }

    public class Client
    {
        HttpRequestMessage Base = new HttpRequestMessage();
        public static Dictionary<string, object> OptDict = new Dictionary<string, object>()
        {
            {"host", "localhost" },
            {"port", 20103 },
            {"method", "POST" },
            {"user", "" },
            {"pass", "" },
            {"https", false }
        };
        public async void Send(string command, params string[] args)
        {
            var rpcData = new Dictionary<string, string>()
            {
                {"id", "date" },//FIXME: values
                {"method", command.ToLower() },
                {"params", JsonConvert.SerializeObject(args) }
            };
            var stringContent = new StringContent(JsonConvert.SerializeObject(rpcData), Encoding.UTF8, "application/json");//json-rpc
            //var rpcJson = JsonConvert.SerializeObject(rpcData);
            CancellationTokenSource cancel = new CancellationTokenSource();
            HttpClient client = new HttpClient(new RPCMessageHandler(new HttpClientHandler()));
            var resp = await client.PostAsync("https://" + (string)OptDict["host"] + ":" + ((int)OptDict["port"]).ToString() + "/", stringContent);
            resp.EnsureSuccessStatusCode();
            if (resp.Content != null)
            {
                var stringresp = await resp.Content.ReadAsStringAsync();
                var jsonresp = JObject.Parse(stringresp);
                if (jsonresp["error"] != null)
                {
                    
                }
                
            }

        }

        public void Set(string k, object v)
        {
            OptDict[k] = v;
            if(k.ToLower() == "host")
            {

            }
        }

        public bool IsCommand(string command) => Constants.commands.Any(x => x.ToLower() == command.ToLower());
    }
}
