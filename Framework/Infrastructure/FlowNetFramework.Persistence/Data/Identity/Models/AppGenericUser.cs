using Microsoft.AspNetCore.Identity;
using System.Runtime.Serialization;

namespace FlowNetFramework.Persistence.Data.Identity.Models

{
    public class AppGenericUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsEnabled { get; set; }

        [IgnoreDataMember]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}
