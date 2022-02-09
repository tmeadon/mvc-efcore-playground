using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApp.Data;
using TestApp.Models;

namespace TestApp.Pages.Instructors
{
    public class CreateModel : InstructorCoursesPageModel
    {
        private readonly SchoolContext _context;
        private readonly ILogger<InstructorCoursesPageModel> _logger;

        public CreateModel(TestApp.Data.SchoolContext context, ILogger<InstructorCoursesPageModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var instructor = new Instructor();
            instructor.Courses = new List<Course>();
            PopulateAssignedCourseData(_context, instructor);
            return Page();
        }

        [BindProperty]
        public Instructor Instructor { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(string[] selectedCourses)
        {
            var newInstructor = new Instructor();
            await AddCoursesIfAnySelectedAsync(selectedCourses, newInstructor);

            if (await TryAddInstructorToDatabaseAsync(newInstructor))
            {
                return RedirectToPage("./Index");
            }

            PopulateAssignedCourseData(_context, newInstructor);
            return Page();
        }

        private async Task AddCoursesIfAnySelectedAsync(string[] selectedCourses, Instructor newInstructor)
        {
            if (selectedCourses.Length > 0)
            {
                newInstructor.Courses = new List<Course>();
                await _context.Courses.LoadAsync();
                await AddSelectedCoursesToInstructorAsync(selectedCourses, newInstructor);
            }
        }

        private async Task AddSelectedCoursesToInstructorAsync(string[] selectedCourses, Instructor newInstructor)
        {
            foreach (var course in selectedCourses)
            {
                var courseToAdd = await _context.Courses.FindAsync(int.Parse(course));

                if (courseToAdd != null)
                {
                    newInstructor.Courses.Add(courseToAdd);
                }
                else
                {
                    _logger.LogWarning($"Course {course} not found");
                }
            }
        }

        private async Task<bool> TryAddInstructorToDatabaseAsync(Instructor newInstructor)
        {
            try
            {
                if (await TryUpdateModelAsync<Instructor>(
                    newInstructor,
                    "Instructor",
                    i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
                {
                    _context.Instructors.Add(newInstructor);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return false;
        }
    }
}
