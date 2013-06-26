using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Microsoft.AspNet.SignalR.Client.Samples
{
    public class CommonClient
    {
        private TextWriter _traceWriter;

        public CommonClient(TextWriter traceWriter)
        {
            _traceWriter = traceWriter;
        }

        public async Task RunAsync(string url)
        {
            try
            {
                await RunHubConnectionAPI(url);
            }
            catch (HttpClientException httpClientException)
            {
                _traceWriter.WriteLine("HttpClientException: {0}", httpClientException.Response);
                throw;
            }
            catch (Exception exception)
            {
                _traceWriter.WriteLine("Exception: {0}", exception);
                throw;
            }
        }

        private async Task RunHubConnectionAPI(string url)
        {
            var hubConnection = new HubConnection(url);
            hubConnection.TraceWriter = _traceWriter;

            var hubProxy = hubConnection.CreateHubProxy("HubConnectionAPI");
            hubProxy.On<string>("displayMessage", (data) => hubConnection.TraceWriter.WriteLine(data));
            
            await hubConnection.Start();
            hubConnection.TraceWriter.WriteLine("transport.Name={0}", hubConnection.Transport.Name);

            await hubProxy.Invoke("DisplayMessageCaller", "Hello Caller!");

            string joinGroupResponse = await hubProxy.Invoke<string>("JoinGroup", hubConnection.ConnectionId, "CommonClientGroup");
            hubConnection.TraceWriter.WriteLine("joinGroupResponse={0}", joinGroupResponse);
            
            await hubProxy.Invoke("DisplayMessageGroup", "CommonClientGroup", "Hello Group Members!");

            string leaveGroupResponse = await hubProxy.Invoke<string>("LeaveGroup", hubConnection.ConnectionId, "CommonClientGroup");
            hubConnection.TraceWriter.WriteLine("leaveGroupResponse={0}", leaveGroupResponse);

            await hubProxy.Invoke("DisplayMessageGroup", "CommonClientGroup", "Hello Group Members! (caller should not see this message)");

            await hubProxy.Invoke("DisplayMessageCaller", "Hello Caller again!");
        }

        private async Task RunDemo(string url)
        {
            var hubConnection = new HubConnection(url);
            hubConnection.TraceWriter = _traceWriter;

            var hubProxy = hubConnection.CreateHubProxy("demo");
            hubProxy.On<int>("invoke", (i) => 
            {
                int n = hubProxy.GetValue<int>("index");
                hubConnection.TraceWriter.WriteLine("{0} client state index -> {1}", i, n);
            });

            await hubConnection.Start();
            hubConnection.TraceWriter.WriteLine("transport.Name={0}", hubConnection.Transport.Name);

            await hubProxy.Invoke("multipleCalls");
        }

        private async Task RunRawConnection(string serverUrl)
        {
            string url = serverUrl + "raw-connection";

            var connection = new Connection(url);
            connection.TraceWriter = _traceWriter;

            await connection.Start();
            connection.TraceWriter.WriteLine("transport.Name={0}", connection.Transport.Name);

            await connection.Send(new { type = 1, value = "first message" });
            await connection.Send(new { type = 1, value = "second message" });
        }


        private async Task RunStreaming(string serverUrl)
        {
            string url = serverUrl + "streaming-connection";

            var connection = new Connection(url);
            connection.TraceWriter = _traceWriter;

            await connection.Start();
            connection.TraceWriter.WriteLine("transport.Name={0}", connection.Transport.Name);
        }

        private async Task RunBasicAuth(string url)
        {
            var hubConnection = new HubConnection(url);
            hubConnection.TraceWriter = _traceWriter;
            hubConnection.Credentials = new NetworkCredential("username", "password");

            var hubProxy = hubConnection.CreateHubProxy("AuthHub");
            hubProxy.On<string, string>("invoked", (connectionId, date) => hubConnection.TraceWriter.WriteLine("connectionId={0}, date={1}", connectionId, date));

            await hubConnection.Start();            
            hubConnection.TraceWriter.WriteLine("transport.Name={0}", hubConnection.Transport.Name);

            await hubProxy.Invoke("InvokedFromClient");
        }

        private async Task RunWindowsAuth(string url)
        {
            var hubConnection = new HubConnection(url);
            hubConnection.TraceWriter = _traceWriter;

            // Windows Auth is not supported on SL and WindowsStore apps
#if !SILVERLIGHT && !NETFX_CORE
            hubConnection.Credentials = CredentialCache.DefaultCredentials;
#endif
            var hubProxy = hubConnection.CreateHubProxy("AuthHub");
            hubProxy.On<string, string>("invoked", (connectionId, date) => hubConnection.TraceWriter.WriteLine("connectionId={0}, date={1}", connectionId, date));

            await hubConnection.Start();
            hubConnection.TraceWriter.WriteLine("transport.Name={0}", hubConnection.Transport.Name);

            await hubProxy.Invoke("InvokedFromClient");
        }

        private async Task RunHeaderAuthHub(string url)
        {
            var hubConnection = new HubConnection(url);
            hubConnection.TraceWriter = _traceWriter;
            hubConnection.Headers.Add("username", "john");

            var hubProxy = hubConnection.CreateHubProxy("HeaderAuthHub");
            hubProxy.On<string>("display", (msg) => hubConnection.TraceWriter.WriteLine(msg));

            await hubConnection.Start();
            hubConnection.TraceWriter.WriteLine("transport.Name={0}", hubConnection.Transport.Name);
        }   
    }    
}
