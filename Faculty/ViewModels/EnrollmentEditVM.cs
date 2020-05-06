using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace Faculty.ViewModels
{
    public class EnrollmentEditVM
    {

        public int Id { get; set; }

        [Range(1, 8)]
        public int? Semester { get; set; }

       
        public int? Year { get; set; }

        [Range(1, 100)]
        public int? Grade { get; set; }

        [Display(Name = "Seminar Url")]
        
        public IFormFile SeminarUrl { get; set; }
        
        [Display(Name = "Project Url")]
      
        public string ProjectUrl { get; set; }


        [Range(1, 200)]
        [Display(Name = "Exam Points")]
        public int? ExamPoints { get; set; }

        [Range(1, 200)]
        [Display(Name = "Seminar Points")]
        public int? SeminarPoints { get; set; }

        [Range(1, 200)]
        [Display(Name = "Project Points")]
        public int? ProjectPoints { get; set; }

        [Range(1, 50)]
        [Display(Name = "Additional Points")]
       
        public int? AdditionalPoints { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Finish Date")]
        public DateTime? FinishDate { get; set; }

        public int CourseId { get; set; }
        public int StudentId { get; set; }
    }
}









