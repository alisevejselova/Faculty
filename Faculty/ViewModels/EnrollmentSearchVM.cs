using Faculty.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faculty.ViewModels
{
    public class EnrollmentSearchVM
    {
        public IList<Enrollment> Enrollments { get; set; }
        public string EnrollmentCourse { get; set; }
       
        public string SearchStudentId { get; set; }
    }
    }

