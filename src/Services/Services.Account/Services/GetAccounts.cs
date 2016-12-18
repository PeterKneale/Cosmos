using ServiceStack;
using ServiceStack.OrmLite;
using System.Linq;

namespace Services.Account
{
    [Route("/account", "GET", Summary = "Get all Accounts")]
    public class GetAccounts : IReturn<GetAccountsResponse>
    {

    }

    public class GetAccountsResponse
    {
        public AccountModel[] Accounts { get; set; }
        public long Total { get; set; }
        public string Result { get; set; }
    }

    public class GetAccountsService : Service
    {
        public GetAccountsResponse Get(GetAccounts request)
        {
            using (var transaction = Db.OpenTransaction())
            {
                var data = Db.Select<AccountData>();
                var count = Db.Count<AccountData>();

                var accounts = data.Select(x=>x.ConvertTo<AccountModel>()).ToArray();
                return new GetAccountsResponse { Accounts = accounts, Total = count };
            }
        }
    }
}