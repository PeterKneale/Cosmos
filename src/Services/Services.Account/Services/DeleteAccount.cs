using ServiceStack;
using ServiceStack.OrmLite;

namespace Services.Account
{
    [Route("/account/{id}", "DELETE", Summary = "Delete an Account")]
    public class DeleteAccount : IReturn<DeleteAccountResponse>
    {
        [ApiMember(Name="Id", Description = "Identifier", ParameterType = "path", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    public class DeleteAccountResponse
    {
        public string Result { get; set; }
    }

    public class DeleteAccountService : Service
    {
        public DeleteAccountResponse Delete(DeleteAccount request)
        {
            var id = Db.DeleteById<AccountData>(request.Id);
            return new DeleteAccountResponse();
        }
    }
}