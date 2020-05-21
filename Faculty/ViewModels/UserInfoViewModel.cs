using Faculty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacultyMVC.ViewModels
{
    public class UserInfoViewModel
    {
        public AppUser AppUser { get; set; }

        public Task<IList<string>> Roles { get; set; }

    }
}