using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Logging;
using ServiceStack.Data;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;

namespace Services.Account
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://127.0.0.1:8081/")
                .Build();

            host.Run();
        }
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();

            app.UseServiceStack(new AppHost());

            app.Run(context =>
            {
                context.Response.Redirect("/metadata");
                return Task.FromResult(0);
            });
        }
    }


    public class AppHost : AppHostBase
    {
        public AppHost() : base("Account", typeof(GetAccountService).GetAssembly()) { }

        public override void Configure(Container container)
        {
            var log = LogManager.GetLogger(typeof(AppHost));
            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled: true);

            Plugins.Add(new PostmanFeature());
            Plugins.Add(new CorsFeature());

            SetConfig(new HostConfig { DebugMode = true });

            // Rabbit
            var mqServer = new RabbitMqServer("192.168.99.100:32776");
            mqServer.DisablePriorityQueues = true;
            mqServer.RegisterHandler<CreateAccount>(this.ExecuteMessage, noOfThreads: 4);
            mqServer.RegisterHandler<DeleteAccount>(this.ExecuteMessage, noOfThreads: 4);
            mqServer.RegisterHandler<DeleteAccounts>(this.ExecuteMessage, noOfThreads: 4);
            mqServer.RegisterHandler<GetAccount>(this.ExecuteMessage, noOfThreads: 4);
            mqServer.RegisterHandler<GetAccounts>(this.ExecuteMessage, noOfThreads: 4);
            mqServer.RegisterHandler<AccountCreatedEvent>(x => {
                log.Info($"an event occurred! {x.SerializeToString()}");
                return null;
            });
            
            mqServer.Start();
            container.Register<IMessageService>(c => mqServer);
            container.RegisterAs<Bus, IBus>().ReusedWithin(ReuseScope.None);

            // ORMLite
            var dbFactory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);
            dbFactory.AutoDisposeConnection = false;
            dbFactory.OpenDbConnection().CreateTableIfNotExists<AccountData>();
            container.Register<IDbConnectionFactory>(c => dbFactory);

            // Errors
            this.ServiceExceptionHandlers.Add((httpReq, request, exception) =>
            {
                log.Error($"Error: {exception.Message}. {exception.StackTrace}.", exception);
                return null;
            });

            var bus = container.Resolve<IBus>();
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            bus.Send<CreateAccount, CreateAccountResponse>(new CreateAccount{Name="Peter"});
            
        }

    }

    public interface IBus
    {
        void Publish<TEvent>(TEvent message);
        TResponse Send<TRequest, TResponse>(TRequest request);
    }

    public class Bus : IBus
    {
        private ILog _log = LogManager.GetLogger(typeof(Bus));
        private readonly IMessageQueueClient _mqClient;
        private readonly IMessageProducer _mqProducer;
        public Bus(IMessageService messageService)
        {
            _mqClient = messageService.CreateMessageQueueClient();
            _mqProducer = messageService.CreateMessageProducer();
        }

        public void Publish<TEvent>(TEvent message)
        {
            _log.Info($"publishing {message.SerializeToString()}");
            _mqProducer.Publish<TEvent>(message);
        }

        public TResponse Send<TRequest, TResponse>(TRequest request)
        {
            _log.Info($"Sending {request.SerializeToString()}");
            var queue = _mqClient.GetTempQueueName();
            
            _log.Info($"to queue {queue}");
            _mqClient.Publish(new Message<TRequest>(request) { ReplyTo = queue });

            var response = _mqClient.Get<TResponse>(queue);
            _log.Info($" got response {response.SerializeToString()}");
            _mqClient.Ack(response);
            return response.GetBody();
        }
    }
}