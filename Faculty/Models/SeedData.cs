using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Faculty.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new FacultyContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<FacultyContext>>()))
            {
                // Look for any movies.
                if (context.Student.Any() || context.Teacher.Any() || context.Course.Any())
                {
                    return;   // DB has been seeded
                }

                context.Student.AddRange(

                    new Student
                    { /*Id = 1, */
                        StudentId = "1/2017",
                        FirstName = "Rob",
                        LastName = "Reiner",
                        EnrollmentDate = DateTime.Parse("2020-3-6"),
                        AcquiredCredits = 150,
                        CurrentSemestar = 5,
                        EducationLevel = "Associate degree"
                    },
                     new Student
                     { /*Id = 2, */
                         StudentId = "2/2016",
                         FirstName = "Billy",
                         LastName = "Crystal",
                         EnrollmentDate = DateTime.Parse("2020-3-12"),
                         AcquiredCredits = 170,
                         CurrentSemestar = 7,
                         EducationLevel = "Bachelor's degree"
                     },
                      new Student
                      { /*Id = 3, */
                          StudentId = "3/2015",
                          FirstName = "Dan",
                          LastName = "Murray",
                          EnrollmentDate = DateTime.Parse("2020-5-12"),
                          AcquiredCredits = 200,
                          CurrentSemestar = 8,
                          EducationLevel = "Master's degree"
                      }
                );
                context.SaveChanges();

                context.Teacher.AddRange(
                   new Teacher
                   { /*Id = 1, */
                       FirstName = "Howard",
                       LastName = "Reiner",
                       Degree = "Doctoral Degree",
                       AcademicRank = "Associate Professor",
                       OfficeNumber = "312",
                       HireDate = DateTime.Parse("2015-9-2"),
                   },
                   new Teacher
                   { /*Id = 2, */
                       FirstName = "Alen",
                       LastName = "Belly",
                       Degree = "Professional Degree",
                       AcademicRank = "Full Professor",
                       OfficeNumber = "251",
                       HireDate = DateTime.Parse("2010-10-10")
                   },
                    new Teacher
                    { /*Id = 3, */
                        FirstName = "John",
                        LastName = "Kerry",
                        Degree = "Master's Degree",
                        AcademicRank = "Assistant Professor",
                        OfficeNumber = "100",
                        HireDate = DateTime.Parse("2018-4-11")
                    }


               );
                context.SaveChanges();


                context.Course.AddRange(
                    new Course
                    { /*Id = 1, */
                        Title = "Computer Communication Technologies",
                        Credits = 6,
                        Semester = 5,
                        Programme = "Computer Technologies and Engineering",
                        EducationLevel = "Associate degree",
                        FirstTeacherID = context.Teacher.Single(d => d.FirstName == "Howard" && d.LastName == "Reiner").Id,
                        SecondTeacherID = context.Teacher.Single(d => d.FirstName == "Alen" && d.LastName == "Belly").Id

                    },
                    new Course
                    { /*Id = 2, */
                        Title = "Virtualization and Cloud Systems",
                        Credits = 6,
                        Semester = 8,
                        Programme = "Computer Technologies and Engineering",
                        EducationLevel = "Master's degree",
                        FirstTeacherID = context.Teacher.Single(d => d.FirstName == "John" && d.LastName == "Kerry").Id,
                        SecondTeacherID = context.Teacher.Single(d => d.FirstName == "Howard" && d.LastName == "Reiner").Id
                    },
                      new Course
                      { /*Id = 3, */
                          Title = "Android Programming",
                          Credits = 6,
                          Semester = 7,
                          Programme = "Computer Technologies and Engineering",
                          EducationLevel = "Bachelor's degree",
                          FirstTeacherID = context.Teacher.Single(d => d.FirstName == "Alen" && d.LastName == "Belly").Id,
                          SecondTeacherID = context.Teacher.Single(d => d.FirstName == "Howard" && d.LastName == "Reiner").Id
                      }

                );
                context.SaveChanges();


                context.SaveChanges();

                context.Enrollment.AddRange(
                    new Enrollment
                    {
                        CourseId = 1,
                        StudentId = 2,
                        Semester = 5,
                        Year = 2,
                        Grade = 4,
                        SeminarUrl = "https://",
                        ProjectUrl = "https://",
                        ExamPoints = 90,
                        SeminarPoints = 20,
                        ProjectPoints = 10,
                        AdditionalPoints = 5,
                        FinishDate = DateTime.Parse("2018-4-11")
                    },
                                       new Enrollment
                                       {
                                           CourseId = 3,
                                           StudentId = 3,
                                           Semester = 7,
                                           Year = 3,
                                           Grade = 5,
                                           SeminarUrl = "https://",
                                           ProjectUrl = "https://",
                                           ExamPoints = 95,
                                           SeminarPoints = 25,
                                           ProjectPoints = 20,
                                           AdditionalPoints = 10,
                                           FinishDate = DateTime.Parse("2017-5-11")
                                       },
                                      new Enrollment
                                      {
                                          CourseId = 3,
                                          StudentId = 2,
                                          Semester = 7,
                                          Year = 3,
                                          Grade = 5,
                                          SeminarUrl = "https://",
                                          ProjectUrl = "https://",
                                          ExamPoints = 95,
                                          SeminarPoints = 25,
                                          ProjectPoints = 20,
                                          AdditionalPoints = 10,
                                          FinishDate = DateTime.Parse("2017-5-11")
                                      },
                           new Enrollment
                           {
                               CourseId = 2,
                               StudentId = 1,
                               Semester = 7,
                               Year = 3,
                               Grade = 5,
                               SeminarUrl = "https://",
                               ProjectUrl = "https://",
                               ExamPoints = 95,
                               SeminarPoints = 25,
                               ProjectPoints = 20,
                               AdditionalPoints = 10,
                               FinishDate = DateTime.Parse("2017-5-11")
                           },
                  new Enrollment
                  {
                      CourseId = 1,
                      StudentId = 1,
                      Semester = 8,
                      Year = 4,
                      Grade = 6,
                      SeminarUrl = "https://",
                      ProjectUrl = "https://",
                      ExamPoints = 100,
                      SeminarPoints = 50,
                      ProjectPoints = 25,
                      AdditionalPoints = 15,
                      FinishDate = DateTime.Parse("2015-6-11")
                  }
                );

                context.SaveChanges();
            }
        }
    }
}
