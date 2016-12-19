
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.Messaging;

namespace Services.Account
{
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