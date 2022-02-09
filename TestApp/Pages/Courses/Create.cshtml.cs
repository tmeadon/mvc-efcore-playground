using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TestApp.Data;
using TestApp.Models;

namespace TestApp.Pages.Courses;

public class CreateModel : DepartmentNamePageModel
{
    private readonly SchoolContext _context;

    public CreateModel(SchoolContext context)
    {
        _context = context;
    }

    public IActionResult OnGet()
    {
        PopulateDepartmentsDropDownList(_context);
        return Page();
    }

    [BindProperty]
    public Course Course { get; set; }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        var emptyCourse = new Course();

        if (await TryUpdateModelAsync<Course>(
            emptyCourse,
            "course", // prefix for form value
            s => s.CourseID, s => s.DepartmentID, s => s.Title, s => s.Credits))
        {
            _context.Courses.Add(emptyCourse);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        // select DepartmentID if TryUpdateModelAsync fails
        PopulateDepartmentsDropDownList(_context, emptyCourse.DepartmentID);
        return Page();
    }
}
