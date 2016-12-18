using ServiceStack;
using ServiceStack.OrmLite;

namespace Services.Account
{
    [Route("/account", "POST", Summary = "Create an Account")]
    public class CreateAccount : IReturn<CreateAccountResponse>
    {
        [ApiMember(Name="Name", Description = "Name", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Name { get; set; }
    }

    public class CreateAccountResponse
    {
        public AccountModel Account { get; set; }
        public string Result { get; set; }
    }

    public class CreateAccountService : Service
    {
        public CreateAccountResponse Post(CreateAccount request)
        {
            var id = Db.Insert(new AccountData { Name = request.Name }, selectIdentity: true);
            
            var data = Db.SingleById<AccountData>(id);
            var model = data.ConvertTo<AccountModel>();
            return new CreateAccountResponse { Account = model };
        }
    }
}