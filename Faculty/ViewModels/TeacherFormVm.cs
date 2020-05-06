using Faculty.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Faculty.ViewModels
{
    public class TeacherFormVm
    {
        public  IFormFile ProfilePicture { get; set; }
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(50)]
        public string Degree { get; set; }

        [StringLength(25)]
        [Display(Name = "Academic Rank")]
        public string AcademicRank { get; set; }

        [StringLength(10)]
        [Display(Name = "Office Number")]
        public string OfficeNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hire Date")]
        public DateTime? HireDate { get; set; }
        public virtual ICollection<Course> FirstCourses { get; set; }
        public virtual ICollection<Course> SecondCourses { get; set; }

        public string FullName
        {
            get
            {
                return String.Format("{0} {1}", FirstName, LastName);
            }
        }
    }
}
