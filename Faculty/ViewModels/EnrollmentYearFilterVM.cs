using Faculty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faculty.ViewModels
{
    public class EnrollmentYearFilterVM
    {
        public IList<Enrollment> Enrollments { get; set; }
        public int EnrollmentYear { get; set; }
    }
}
