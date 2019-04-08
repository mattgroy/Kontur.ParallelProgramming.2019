using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Cluster
{
    public class ClusterServer
    {
        private const int Running = 1;
        private const int NotRunning = 0;

        private readonly ILog log;
        private HttpListener httpListener;

        private int isRunning = NotRunning;


        private int RequestsCount;

        public ClusterServer(ServerOptions serverOptions, ILog log)
        {
            this.ServerOptions = serverOptions;
            this.log = log;
        }

        public ServerOptions ServerOptions { get; }

        public void Start()
        {
            if (Interlocked.CompareExchange(ref this.isRunning, Running, NotRunning) == NotRunning)
            {
                this.httpListener = new HttpListener
                {
                    Prefixes =
                    {
                        $"http://+:{this.ServerOptions.Port}/{this.ServerOptions.MethodName}/"
                    }
                };

                this.log.InfoFormat(
                    $"Server is starting listening prefixes: {string.Join(";", this.httpListener.Prefixes)}");

                if (this.ServerOptions.Async)
                {
                    this.log.InfoFormat("Press ENTER to stop listening");
                    this.httpListener.StartProcessingRequestsAsync(CreateAsyncCallback(this.ServerOptions));
                }
                else
                {
                    this.httpListener.StartProcessingRequestsSync(CreateSyncCallback(this.ServerOptions));
                }
            }
        }

        public void Stop()
        {
            if (Interlocked.CompareExchange(ref this.isRunning, NotRunning, Running) == Running)
                if (this.httpListener.IsListening)
                    this.httpListener.Stop();
        }

        private Action<HttpListenerContext> CreateSyncCallback(ServerOptions parsedOptions)
        {
            return context =>
            {
                var currentRequestId = Interlocked.Increment(ref this.RequestsCount);
                this.log.InfoFormat("Thread #{0} received request #{1} at {2}",
                    Thread.CurrentThread.ManagedThreadId, currentRequestId, DateTime.Now.TimeOfDay);

                Thread.Sleep(parsedOptions.MethodDuration);

                var encryptedBytes = ClusterHelpers.GetBase64HashBytes(context.Request.QueryString["query"]);
                context.Response.OutputStream.Write(encryptedBytes, 0, encryptedBytes.Length);

                this.log.InfoFormat("Thread #{0} sent response #{1} at {2}",
                    Thread.CurrentThread.ManagedThreadId, currentRequestId,
                    DateTime.Now.TimeOfDay);
            };
        }

        private Func<HttpListenerContext, Task> CreateAsyncCallback(ServerOptions parsedOptions)
        {
            return async context =>
            {
                var currentRequestNum = Interlocked.Increment(ref this.RequestsCount);
                var query = context.Request.QueryString["query"];
                this.log.InfoFormat("Thread #{0} received request '{1}' #{2} at {3}",
                    Thread.CurrentThread.ManagedThreadId, query, currentRequestNum, DateTime.Now.TimeOfDay);

                await Task.Delay(parsedOptions.MethodDuration);
                //				Thread.Sleep(parsedArguments.MethodDuration);

                var encryptedBytes = ClusterHelpers.GetBase64HashBytes(query);
                await context.Response.OutputStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);

                this.log.InfoFormat("Thread #{0} sent response '{1}' #{2} at {3}",
                    Thread.CurrentThread.ManagedThreadId, query, currentRequestNum,
                    DateTime.Now.TimeOfDay);
            };
        }
    }
}