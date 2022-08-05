using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Areas.Identity.Data
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        [StringLength(50, ErrorMessage = "The Last Name cannot be too long.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The Last Name cannot be too long.")]
        public string LastName { get; set; }
    }
}
