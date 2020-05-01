using Faculty.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faculty.ViewModels
{
    public class StudentsInCourseVM
    {
        public Course Course { get; set; }
        public IEnumerable<int> SelectedStudents { get; set; }
        public IEnumerable<SelectListItem> StudentList { get; set; }
    }
}
