using ServiceStack;
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

    public class DeleteAccountsService : Service
    {
        public DeleteAccountsResponse Delete(DeleteAccounts request)
        {
            var id = Db.DeleteAll<AccountData>();
            return new DeleteAccountsResponse();
        }
    }
}