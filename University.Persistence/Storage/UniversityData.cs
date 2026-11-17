using System.Collections.Generic;
using University.Domain.Entities;

namespace University.Persistence.Storage
{
    public class UniversityData
    {
        public List<Student> Students { get; set; } = new();
        public List<Teacher> Teachers { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
    }
}
