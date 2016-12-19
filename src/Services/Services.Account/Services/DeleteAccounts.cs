using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.OrmLite;

namespace Services.Account
{
    [Route("/account", "DELETE", Summary = "Delete Accounts")]
    public class DeleteAccounts : IReturn<DeleteAccountResponse>
    {
        
    }

    public class DeleteAccountsResponse
    {
        public string Result { get; set; }
    }

    public class AccountsDeletedEvent
    {
    }

    public class DeleteAccountsService : Service
    {
        private readonly IMessageService _messageService;

        public DeleteAccountsService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public DeleteAccountsResponse Delete(DeleteAccounts request)
        {
            Db.DeleteAll<AccountData>();
            using (var mqClient = _messageService.CreateMessageQueueClient())
            {
                mqClient.Publish(new AccountsDeletedEvent());
            }
            return new DeleteAccountsResponse();
        }
    }
}