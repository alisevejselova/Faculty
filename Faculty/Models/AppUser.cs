using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Faculty.Models
{
    public class AppUser : IdentityUser
    {
        public int TeacherId { get; set; }

        public int StudentId { get; set; }

        public string Role { get; set; }

    }
}