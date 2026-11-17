using Microsoft.EntityFrameworkCore;
using University.Persistence.Data;

namespace University.Persistence.Configuration
{
    public static class DatabaseFactory
    {
        public static UniversityDbContext Create()
        {
            var options = new DbContextOptionsBuilder<UniversityDbContext>()
                .UseSqlite("Data Source=university.db")
                .Options;

            return new UniversityDbContext(options);
        }
    }
}
