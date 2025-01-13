using Microsoft.AspNetCore.Identity;

namespace FlowNetFramework.Persistence.Data.Identity.Models

{
    public class AppGenericUser : IdentityUser
    {
        public string Name { get; set; }

        public string Surname { get; set; }
    }
}
