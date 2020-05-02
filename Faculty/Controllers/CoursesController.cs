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
    public class CoursesController : Controller
    {
        private readonly FacultyContext _context;

        public CoursesController(FacultyContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string CourseSemester , string CourseProgramme, string SearchString)
        {
            IQueryable<Course> courses = _context.Course.AsQueryable();
            IQueryable<int> semesterQuery = _context.Course.OrderBy(m => m.Semester).Select(m => m.Semester).Distinct();
            IQueryable<string> programmeQuery = _context.Course.OrderBy(m => m.Programme).Select(m => m.Programme).Distinct();
           
            if (!string.IsNullOrEmpty(SearchString)) { 
                courses = courses.Where(s => s.Title.ToLower().Contains(SearchString.ToLower())); 
            }
            int CourseSemesterID = Convert.ToInt32(CourseSemester);
            if (CourseSemesterID != 0)
            {
                courses = courses.Where(x => x.Semester == CourseSemesterID);
            }
            if (!string.IsNullOrEmpty(CourseProgramme))
            {
                courses = courses.Where(x => x.Programme == CourseProgramme);
            }
            
          courses =courses.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher)
                .Include(c => c.Students).ThenInclude(c => c.Student);


            var TitleSemesterProgramme = new TitleSemesterProgrammeVM
            { 
                Semesters = new SelectList(await semesterQuery.ToListAsync()),
                Programmes = new SelectList(await programmeQuery.ToListAsync()),
                Courses = await courses.ToListAsync() };

            return View(TitleSemesterProgramme);
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .Include(c => c.Students).ThenInclude(c => c.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewData["FirstTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName");
            ViewData["SecondTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semestar,Programme,EducationLevel,FirstTeacherID,SecondTeacherID")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirstTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", course.FirstTeacherID);
            ViewData["SecondTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", course.SecondTeacherID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = _context.Course.Where(m => m.Id == id).Include(m => m.Students).First();

            if (course == null)
            {
                return NotFound();
            }
            StudentsInCourseVM viewmodel = new StudentsInCourseVM
            {
                Course = course,
                StudentList = new MultiSelectList(_context.Student.OrderBy(s => s.StudentId), "Id", "FullName"),
                SelectedStudents = course.Students.Select(sa => sa.StudentId)
            };

            ViewData["FirstTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", course.FirstTeacherID);
            ViewData["SecondTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", course.SecondTeacherID);
            return View(viewmodel);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentsInCourseVM viewmodel)
        {
            if (id != viewmodel.Course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewmodel.Course);
                    await _context.SaveChangesAsync();
                    IEnumerable<int> listStudents = viewmodel.SelectedStudents;
                    IQueryable<Enrollment> toBeRemoved = _context.Enrollment.Where
                        (s => !listStudents.Contains(s.StudentId) && s.CourseId == id);
                    _context.Enrollment.RemoveRange(toBeRemoved);

                    IEnumerable<int> existStudents = _context.Enrollment
                        .Where(s => listStudents.Contains(s.StudentId) && s.CourseId == id).Select(s => s.StudentId);
                    IEnumerable<int> newStudents = listStudents.Where(s => !existStudents.Contains(s));
                    foreach (int studentID in newStudents)
                        _context.Enrollment.Add(new Enrollment { StudentId = studentID, CourseId = id });
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(viewmodel.Course.Id))
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
            ViewData["FirstTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName",viewmodel.Course.FirstTeacherID);
            ViewData["SecondTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName",viewmodel.Course.SecondTeacherID);
            return View(viewmodel);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
    }
}
