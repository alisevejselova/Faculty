using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Faculty.Models;
using Faculty.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Faculty.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly FacultyContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private UserManager<AppUser> userManager;

        public EnrollmentsController(FacultyContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userMan)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            userManager = userMan;
        }
        // GET: Enrollments
        [Authorize(Roles = "Admin")]
        
        public async Task<IActionResult> Index(string enrollmentCourse,string searchstring)
        {
            IQueryable<Enrollment> enrollments = _context.Enrollment.AsQueryable();
            enrollments = enrollments.Include(s => s.Course).Include(s => s.Student);

            if (!string.IsNullOrEmpty(enrollmentCourse))
            {
                enrollments = enrollments.Where(s => s.Course.Title.ToLower().Contains(enrollmentCourse.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchstring))
            {
                enrollments = enrollments.Where(s => s.Student.StudentId.ToLower().Contains(searchstring.ToLower()));
            }
         
            var vm = new EnrollmentSearchVM
            {
              
                Enrollments = await enrollments.OrderBy(e => e.Course.Title).Include(s => s.Course).Include(s => s.Student).ToListAsync()
            };
            return View(vm);
        }


        // GET: Enrollments/Details/5
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            AppUser loggedUser = await userManager.GetUserAsync(User);
            if (loggedUser.StudentId != enrollment.StudentId)
            {
                return RedirectToAction("AccessDenied", "Account", null);
            }
            return View(enrollment);
        }

        // GET: Enrollments/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            AppUser loggedUser = await userManager.GetUserAsync(User);
            if ((loggedUser.TeacherId != enrollment.Course.FirstTeacherID) && (loggedUser.TeacherId != enrollment.Course.SecondTeacherID))
            {
                return RedirectToAction("AccessDenied", "Account", null);
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }
        // GET: Enrollments/EditByAdmin/5 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditByAdmin(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            ViewData["StudentName"] = _context.Student.Where(s => s.Id == enrollment.StudentId).Select(s => s.FullName).FirstOrDefault();
            ViewData["CourseName"] = _context.Course.Where(s => s.Id == enrollment.CourseId).Select(s => s.Title).FirstOrDefault();
            return View(enrollment);
        }
        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, string pom, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {  
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (pom.Equals("admin"))
                {
                    return RedirectToAction("Index");
                }
                return RedirectToAction("CourseStudents", new { id = enrollment.CourseId });
               
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);


            if (pom.Equals("admin"))
            {
                return View("EditByAdmin", enrollment);
            }
            else
            {
                return View(enrollment);
            }
        }

        // GET: Enrollments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
       // [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);
            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollment.Any(e => e.Id == id);
        }
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CourseStudents(int? id, int EnrollmentYear)
        {
            if (id == null)
            {
                return NotFound();
            }

            IQueryable<Enrollment> enrollments = _context.Enrollment.Where(e => e.CourseId == id);
            enrollments = enrollments.Include(e => e.Course).Include(e => e.Student);
            if (enrollments != null)
            {
                var anyenrollment = enrollments.First();
                AppUser loggedUser = await userManager.GetUserAsync(User);
                if ((loggedUser.TeacherId != anyenrollment.Course.FirstTeacherID) && (loggedUser.TeacherId != anyenrollment.Course.SecondTeacherID))
                {
                    return RedirectToAction("AccessDenied", "Account", null);
                }
            }

            if (EnrollmentYear != 0)
            {
                enrollments = enrollments.Where(x => x.Year == EnrollmentYear);
            }
            else
            {
                enrollments = enrollments.Where(x => x.Year == DateTime.Now.Year);
            }

            EnrollmentYearFilterVM Vmodel = new EnrollmentYearFilterVM
            {
                Enrollments = await enrollments.ToListAsync()
            };

            ViewData["CourseTitle"] = _context.Course.Where(c => c.Id == id).Select(c => c.Title).FirstOrDefault();
            return View(Vmodel);
        }
        //EDIT BY STUDENT
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EditByStudent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            AppUser loggedUser = await userManager.GetUserAsync(User);
            if (loggedUser.StudentId != enrollment.StudentId)
            {
                return RedirectToAction("AccessDenied", "Account", null);
            }

            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);

            EnrollmentEditVM Vmodel = new EnrollmentEditVM
            {
                Id = enrollment.Id,
                Semester = enrollment.Semester,
                Year = enrollment.Year,
                Grade = enrollment.Grade,
                ProjectUrl = enrollment.ProjectUrl,
                SeminarPoints = enrollment.SeminarPoints,
                ProjectPoints = enrollment.ProjectPoints,
                AdditionalPoints = enrollment.AdditionalPoints,
                ExamPoints = enrollment.ExamPoints,
                FinishDate = enrollment.FinishDate,
                CourseId = enrollment.CourseId,
                StudentId = enrollment.StudentId
            };

            ViewData["StudentFullName"] = _context.Student.Where(s => s.Id == enrollment.StudentId).Select(s => s.FullName).FirstOrDefault();
            ViewData["CourseTitle"] = _context.Course.Where(s => s.Id == enrollment.CourseId).Select(s => s.Title).FirstOrDefault();
            return View(Vmodel);
        }

        // POST: Enrollments/EditByStudent/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EditByStudent(int id, EnrollmentEditVM Vmodel)
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

                    Enrollment enrollment = new Enrollment
                    {
                        Id = Vmodel.Id,
                        Semester = Vmodel.Semester,
                        Year = Vmodel.Year,
                        Grade = Vmodel.Grade,
                        SeminarUrl = uniqueFileName,
                        ProjectUrl = Vmodel.ProjectUrl,
                        SeminarPoints = Vmodel.SeminarPoints,
                        ProjectPoints = Vmodel.ProjectPoints,
                        AdditionalPoints = Vmodel.AdditionalPoints,
                        ExamPoints = Vmodel.ExamPoints,
                        FinishDate = Vmodel.FinishDate,
                        CourseId = Vmodel.CourseId,
                        StudentId = Vmodel.StudentId
                    };

                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(Vmodel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = Vmodel.Id });
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", Vmodel.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", Vmodel.StudentId);
            return View(Vmodel);
        }
        private string UploadedFile(EnrollmentEditVM Vmodel)
        {
            string uniqueFileName = null;

            if (Vmodel.SeminarUrl != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "SeminarFiles");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Vmodel.SeminarUrl.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Vmodel.SeminarUrl.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
       

    }
}
