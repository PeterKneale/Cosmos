using ServiceStack;
using ServiceStack.OrmLite;

namespace Services.Account
{
    [Route("/account/{id}", "GET", Summary = "Get an Account")]
    public class GetAccount : IReturn<GetAccountResponse>
    {
        [ApiMember(Name="Id", Description = "Identifier", ParameterType = "path", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    public class GetAccountResponse
    {
        public AccountModel Account { get; set; }
        public string Result { get; set; }
    }

    public class GetAccountService : Service
    {
        public GetAccountResponse Get(GetAccount request)
        {
            using (var transaction = Db.OpenTransaction())
            {
                var data = Db.SingleById<AccountData>(request.Id);
                if (data == null)
                {
                    throw HttpError.NotFound("Account does not exist");
                }
                
                var account = data.ConvertTo<AccountModel>();
                return new GetAccountResponse { Account = account };
            }
        }
    }
}