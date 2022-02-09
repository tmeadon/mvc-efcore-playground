using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ExampleSchoolApp.Data;
using ExampleSchoolApp.Models;

namespace ExampleSchoolApp.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly SchoolContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(SchoolContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string NameSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public PaginatedList<Student> Students { get; set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else 
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<Student> studentsQueryable = from s in _context.Students select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQueryable = studentsQueryable.Where(s => s.LastName.Contains(searchString)
                                                            || s.FirstMidName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentsQueryable = studentsQueryable.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    studentsQueryable = studentsQueryable.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    studentsQueryable = studentsQueryable.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    studentsQueryable = studentsQueryable.OrderBy(s => s.LastName);
                    break;
            }

            var pageSize = _configuration.GetValue("PageSize", 4);
            Students = await PaginatedList<Student>.CreateAsync(studentsQueryable.AsNoTracking(), pageIndex ?? 1, pageSize);
        }
    }
}
