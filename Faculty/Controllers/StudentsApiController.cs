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
    public class StudentsApiController : ControllerBase
    {
        private readonly FacultyContext _context;

        public StudentsApiController(FacultyContext context)
        {
            _context = context;
        }

        // GET: api/StudentsApi
        [HttpGet]
        public List<Student> GetStudent(string StudentIndex, string SearchString)
        {
            IQueryable<Student> students = _context.Student.AsQueryable();
            IQueryable<string> indexQuery = _context.Student.OrderBy(m => m.StudentId).Select(m => m.StudentId).Distinct();


            if (!string.IsNullOrEmpty(StudentIndex))
            {
                students = students.Where(x => x.StudentId == StudentIndex);
            }

            IEnumerable<Student> dataList = students as IEnumerable<Student>;

            if (!string.IsNullOrEmpty(SearchString))
            {
                dataList = dataList.ToList().Where(s => (s.FullName + " " + s.LastName).ToLower().Contains(SearchString.ToLower()));
            }
            return students.ToList();
        }

        // GET: api/StudentsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var student = await _context.Student.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }
        // GET: api/StudentsApi 
        [HttpGet("{id}/GetCourses")]
        public async Task<IActionResult> GetCoursesInStudent([FromRoute] int id)
        {
            var student = await _context.Course.FindAsync(id);
            if (student == null) { return NotFound(); }

            var courseStudents = _context.Enrollment.Where(m => m.StudentId == id).ToList();
            List<Course> courses = new List<Course>(); //prazna lista od predmeti
            foreach (var course in courseStudents)
            {
                Course newcourse = _context.Course.Where(m => m.Id == course.CourseId).FirstOrDefault();
                newcourse.Students = null; //za da nema problem so ciklicni referenci
                courses.Add(newcourse);
            }
            return Ok(courses);  //vrakaj site studenti za toj predmet
        }

        // PUT: api/StudentsApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent( int id,  CourseStudentVM viewmodel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != viewmodel.Student.Id)
            {
                return BadRequest();
            }

            _context.Entry(viewmodel.Student).State = EntityState.Modified;

            try
            {
                
                await _context.SaveChangesAsync();
                IEnumerable<int> listCourses= viewmodel.SelectedCourses;
                IQueryable<Enrollment> toBeRemoved = _context.Enrollment.Where
                    (s => !listCourses.Contains(s.CourseId) && s.StudentId == id);
                _context.Enrollment.RemoveRange(toBeRemoved);

                IEnumerable<int> existCourses = _context.Enrollment
                    .Where(s => listCourses.Contains(s.CourseId) && s.StudentId == id).Select(s => s.CourseId);
                IEnumerable<int> newCourses = listCourses.Where(s => !existCourses.Contains(s));
                foreach (int courseID in newCourses)
                    _context.Enrollment.Add(new Enrollment { CourseId = courseID, StudentId = id });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        // POST: api/StudentsApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Student.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.Id }, student);
        }

        // DELETE: api/StudentsApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Student>> DeleteStudent([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Student.Remove(student);
            await _context.SaveChangesAsync();

            return Ok(student);
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
    }
}
