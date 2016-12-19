using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.OrmLite;

namespace Services.Account
{
    [Route("/account/{id}", "DELETE", Summary = "Delete an Account")]
    public class DeleteAccount : IReturn<DeleteAccountResponse>
    {
        [ApiMember(Name = "Id", Description = "Identifier", ParameterType = "path", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    public class DeleteAccountResponse
    {
        public string Result { get; set; }
    }

    public class AccountDeletedEvent
    {
        public int Id { get; set; }
    }

    public class DeleteAccountService : Service
    {
        private readonly IMessageService _messageService;

        public DeleteAccountService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public DeleteAccountResponse Delete(DeleteAccount request)
        {
            Db.DeleteById<AccountData>(request.Id);
            using (var mqClient = _messageService.CreateMessageQueueClient())
            {
                mqClient.Publish(new AccountDeletedEvent { Id = request.Id });
            }
            return new DeleteAccountResponse();
        }
    }
}