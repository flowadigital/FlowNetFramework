﻿using Microsoft.AspNetCore.Identity;

namespace FlowNetFramework.Persistence.Data.Identity.Models

{
    public class AppUser : IdentityUser<int>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string ImageUrl { get; set; }
    }
}
