using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Faculty.Models;
using Faculty.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Faculty.Controllers
{
    public class StudentsController : Controller
    {
        private readonly FacultyContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentsController(FacultyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

        }

        // GET: Students

        public async Task<IActionResult> Index(string StudentIndex, string SearchString)
        {
            IQueryable<Student> students = _context.Student.AsQueryable();
            IQueryable<string> indexQuery = _context.Student.OrderBy(m => m.StudentId).Select(m => m.StudentId).Distinct();

            students = students.Include(s => s.Courses).ThenInclude(s => s.Course);

            if (!string.IsNullOrEmpty(StudentIndex))
            {
                students = students.Where(x => x.StudentId == StudentIndex);
            }

            IEnumerable<Student> dataList = students as IEnumerable<Student>;

            if (!string.IsNullOrEmpty(SearchString))
            {
                dataList = dataList.ToList().Where(s => (s.FullName + " "+s.LastName).ToLower().Contains(SearchString.ToLower()));
            }

            var studentVM = new FullNameStudentIdVM
            {
                Indexes = new SelectList(await indexQuery.ToListAsync()),
                Students = dataList.ToList()
            };

            return View(studentVM);
        }
        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.Include(m => m.Courses)
                .ThenInclude(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["StudentFullName"] = _context.Student.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentFormVM Vmodel)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(Vmodel);

                Student student = new Student
                {   
                    ProfilePicture = uniqueFileName,
                    StudentId = Vmodel.StudentId,
                    FirstName = Vmodel.FirstName,
                    LastName = Vmodel.LastName,
                    EnrollmentDate = Vmodel.EnrollmentDate,
                    AcquiredCredits = Vmodel.AcquiredCredits,
                    CurrentSemestar = Vmodel.CurrentSemestar,
                    EducationLevel = Vmodel.EducationLevel,
                    Courses = Vmodel.Courses,
                };

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        private string UploadedFile(StudentFormVM model)
        {
            string uniqueFileName = null;

            if (model.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfilePicture.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            StudentFormVM Vmodel = new StudentFormVM
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                StudentId = student.StudentId,
                EnrollmentDate = student.EnrollmentDate,
                AcquiredCredits = student.AcquiredCredits,
                CurrentSemestar = student.CurrentSemestar,
                EducationLevel = student.EducationLevel,
                Courses = student.Courses
            };
            ViewData["StudentFullName"] = _context.Student.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();

            return View(Vmodel);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, StudentFormVM Vmodel)
        {
            if (id != Vmodel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadedFile(Vmodel);

                    Student student = new Student
                    {
                        Id = Vmodel.Id,
                        FirstName = Vmodel.FirstName,
                        LastName = Vmodel.LastName,
                        ProfilePicture = uniqueFileName,
                        EnrollmentDate = Vmodel.EnrollmentDate,
                        CurrentSemestar = Vmodel.CurrentSemestar,
                        AcquiredCredits = Vmodel.AcquiredCredits,
                        StudentId = Vmodel.StudentId,
                        EducationLevel = Vmodel.EducationLevel,
                        Courses = Vmodel.Courses
                    };

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(Vmodel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(Vmodel);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["StudentFullName"] = _context.Student.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Student.FindAsync(id);

            //delete image file from the folder
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", student.ProfilePicture);
            FileInfo file = new FileInfo(path);
            if (file.Exists)//check file exsit or not
            {
                file.Delete();
            }

            _context.Student.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
        // GET: Students/StudentCourses/2
        public async Task<IActionResult> StudentCourses(int? id)
        {
            IQueryable<Course> courses = _context.Course.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher).AsQueryable();

            IQueryable<Enrollment> enrollments = _context.Enrollment.AsQueryable();

            enrollments = enrollments.Where(s => s.StudentId == id); //ogrencinin tum enrollmentleri

            IEnumerable<int> enrollmentsCoursesId = enrollments.Select(e => e.CourseId).Distinct();  //tum kurs id ler

            courses = courses.Where(s => enrollmentsCoursesId.Contains(s.Id)); //student uygun olan course al

            courses = courses.Include(c => c.Students).ThenInclude(c => c.Student);

            ViewData["StudentFullName"] = _context.Student.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();
            ViewData["StudentId"] = id;
            return View(courses);
        }
    }
}
