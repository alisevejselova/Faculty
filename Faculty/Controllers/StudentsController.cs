using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Faculty.Models;
using Faculty.ViewModels;

namespace Faculty.Controllers
{
    public class StudentsController : Controller
    {
        private readonly FacultyContext _context;

        public StudentsController(FacultyContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string StudentIndex, string SearchString)
        {
         

             IQueryable<Student> students = _context.Student.AsQueryable();
            IQueryable<string> IndexQuery = _context.Student.OrderBy(m => m.StudentId).Select(m => m.StudentId).Distinct();
             
            if (!string.IsNullOrEmpty(SearchString))
            {
                students = students.Where(s => s.FullName.ToLower().Contains(SearchString.ToLower()));

            } 

            if (!string.IsNullOrEmpty(StudentIndex))
            {
                students = students.Where(x => x.StudentId == StudentIndex); 
            }
           
             students = students.Include(m => m.Courses).ThenInclude(m => m.Course);


            var FullNameStudentId = new FullNameStudentIdVM
            {

                Indexes = new SelectList(await IndexQuery.ToListAsync()),
                Students = await students.ToListAsync()
                // Students = students.ToList()
            };

            return View(FullNameStudentId);
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
        public async Task<IActionResult> Create([Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemestar,EducationLevel")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var student = _context.Student.Where(m => m.Id == id).Include(m => m.Courses).First();
           
            if (student == null)
            {
                return NotFound();
            }
            CourseStudentVM viewmodel = new CourseStudentVM
            {
                Student = student,
                CourseList = new MultiSelectList(_context.Course.OrderBy(s => s.Title), "Id", "Title"),
                SelectedCourses = student.Courses.Select(sa => sa.CourseId)
            };
            return View(viewmodel);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseStudentVM viewmodel)
        {
            if (id != viewmodel.Student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewmodel.Student);
                    await _context.SaveChangesAsync();
                    IEnumerable<int> listCourses = viewmodel.SelectedCourses; 
                    IQueryable<Enrollment> toBeRemoved = _context.Enrollment.Where
                        (s => !listCourses.Contains(s.CourseId) && s.StudentId == id); 
                    _context.Enrollment.RemoveRange(toBeRemoved);

                    IEnumerable<int> existCourses = _context.Enrollment
                        .Where(s => listCourses.Contains(s.CourseId) && s.StudentId == id).Select(s => s.CourseId); 
                    IEnumerable<int> newCourses = listCourses.Where(s => !existCourses.Contains(s)); 
                    foreach (int courseID in newCourses) 
                        _context.Enrollment.Add(new Enrollment { CourseId= courseID, StudentId = id });

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(viewmodel.Student.Id))
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
            return View(viewmodel);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Student.FindAsync(id);
            _context.Student.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
    }
}
