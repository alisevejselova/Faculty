using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Faculty.Models;
using Faculty.ViewModels;

namespace Faculty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesApiController : ControllerBase
    {
        private readonly FacultyContext _context;

        public CoursesApiController(FacultyContext context)
        {
            _context = context;
        }

        // GET: api/CourseApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourse(string CourseSemester, string CourseProgramme, string SearchString)
        {
            IQueryable<Course> courses = _context.Course.AsQueryable();
            IQueryable<int> semesterQuery = _context.Course.OrderBy(m => m.Semester).Select(m => m.Semester).Distinct();
            IQueryable<string> programmeQuery = _context.Course.OrderBy(m => m.Programme).Select(m => m.Programme).Distinct();

            if (!string.IsNullOrEmpty(SearchString))
            {
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
            return courses.ToList();
        }

        // GET: api/CourseApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } 
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } 
            var course = await _context.Course.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }
        // GET: api/CoursesApi 
        [HttpGet("{id}/GetStudents")]
        public async Task<IActionResult> GetStudentsInCourse([FromRoute] int id) 
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null) { return NotFound(); } 

            var studentCourses = _context.Enrollment.Where(m => m.CourseId == id).ToList(); 
            List<Student> students = new List<Student>(); //prazna lista od studenti
            foreach (var student in studentCourses)
            { 
                Student newstudent = _context.Student.Where(m => m.Id == student.StudentId).FirstOrDefault();
                newstudent.Courses = null; //za da nema problem so ciklicni referenci
                students.Add(newstudent); 
            }
            return Ok(students);  //vrakaj site studenti za toj predmet
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, Course course)
        {
            if (id != course.Id)
            {
                return BadRequest();
            }

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/CourseApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
       [HttpPut("{id}/Students")]
        public async Task<IActionResult> PutCourseStudents([FromRoute] int id, [FromBody]  StudentsInCourseVM viewmodel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } 
            if (id != viewmodel.Course.Id)
            {
                return BadRequest();
            }

            _context.Entry(viewmodel.Course).State = EntityState.Modified;

            try
            {
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
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CourseApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse([FromBody] Course course)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } 
            _context.Course.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourse", new { id = course.Id }, course);
        }

        // DELETE: api/CourseApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Course>> DeleteCourse([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } 
            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Course.Remove(course);
            await _context.SaveChangesAsync();

            return Ok(course);
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
    }
}
