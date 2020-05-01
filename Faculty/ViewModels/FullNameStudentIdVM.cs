using Faculty.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faculty.ViewModels
{
    public class FullNameStudentIdVM
    {
        public IList<Student> Students{ get; set; }
        public SelectList Indexes { get; set; }
        public string StudentIndex { get; set; }
        public string SearchString { get; set; }
    }
}
