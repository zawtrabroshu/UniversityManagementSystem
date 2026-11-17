using System;
using System.Collections.Generic;
using System.Linq;
using University.Domain.Entities;

namespace University.BusinessLogic.Services
{
    public class UniversityService
    {
        private readonly List<Student> students = new();
        private readonly List<Teacher> teachers = new();
        private readonly List<Subject> subjects = new();
        private readonly List<Grade> grades = new();
        private readonly List<Department> departments = new();

        // Додавання студента
        public void AddStudent(Student student)
        {
            if (student == null)
                throw new ArgumentNullException("Студент не може бути порожнiм.");

            students.Add(student);
        }
        // Прив'язування облікового запису до студента
        public void LinkStudentAccount(Guid studentId, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");

            string normalizedUsername = username.Trim();

            Student student = students.FirstOrDefault(s => s.Id == studentId);

            if (student == null)
                throw new Exception("Студента не знайдено.");

            if (string.Equals(
                student.AccountUsername,
                normalizedUsername,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Цей обліковий запис вже прив'язаний до студента.");
            }

            bool accountExists = students.Any(s =>
                s.Id != studentId &&
                string.Equals(
                    s.AccountUsername,
                    normalizedUsername,
                    StringComparison.OrdinalIgnoreCase));

            if (accountExists)
                throw new Exception("Цей обліковий запис вже прив'язаний до іншого студента.");

            student.LinkAccount(normalizedUsername);
        }
        // Отримання студента за логіном облікового запису
        public Student GetStudentByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");

            Student? student = students.FirstOrDefault(s =>
                string.Equals(
                    s.AccountUsername,
                    username.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (student == null)
                throw new Exception("Для цього облікового запису студента не знайдено.");

            return student;
        }

        // Прив'язування облікового запису до викладача
        public void LinkTeacherAccount(Guid teacherId, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");

            string normalizedUsername = username.Trim();

            Teacher teacher = teachers.FirstOrDefault(t => t.Id == teacherId);

            if (teacher == null)
                throw new Exception("Викладача не знайдено.");

            if (string.Equals(
                teacher.AccountUsername,
                normalizedUsername,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Цей обліковий запис вже прив'язаний до викладача.");
            }

            bool accountExists = teachers.Any(t =>
                t.Id != teacherId &&
                string.Equals(
                    t.AccountUsername,
                    normalizedUsername,
                    StringComparison.OrdinalIgnoreCase));

            if (accountExists)
                throw new Exception("Цей обліковий запис вже прив'язаний до іншого викладача.");

            teacher.LinkAccount(normalizedUsername);
        }

        // Отримання викладача за логіном облікового запису
        public Teacher GetTeacherByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");

            Teacher? teacher = teachers.FirstOrDefault(t =>
                string.Equals(
                    t.AccountUsername,
                    username.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (teacher == null)
                throw new Exception("Для цього облікового запису викладача не знайдено.");

            return teacher;
        }
        // Додавання викладача
        public void AddTeacher(Teacher teacher)
        {
            if (teacher == null)
                throw new ArgumentNullException("Викладач не може бути порожнiм.");

            teachers.Add(teacher);
        }

        // Додавання навчальної дисциплiни
        public void AddSubject(Subject subject)
        {
            if (subject == null)
                throw new ArgumentNullException("Дисциплiна не може бути порожньою.");

            subjects.Add(subject);
        }

        // Призначення дисциплiни студенту
        public void AssignSubjectToStudent(Guid studentId, Guid subjectId)
        {
            Student student = students.FirstOrDefault(s => s.Id == studentId);

            if (student == null)
                throw new Exception("Студента не знайдено.");

            if (!subjects.Any(s => s.Id == subjectId))
                throw new Exception("Навчальну дисциплiну не знайдено.");

            student.AssignSubject(subjectId);
        }

        // Призначення дисциплiни викладачу
        public void AssignSubjectToTeacher(Guid teacherId, Guid subjectId)
        {
            Teacher teacher = teachers.FirstOrDefault(t => t.Id == teacherId);

            if (teacher == null)
                throw new Exception("Викладача не знайдено.");

            if (!subjects.Any(s => s.Id == subjectId))
                throw new Exception("Навчальну дисциплiну не знайдено.");

            teacher.AssignSubject(subjectId);
        }

        // Додавання оцiнки студенту

        // Додавання оцiнки студенту
        public void AddGrade(Grade grade)
        {
            if (grade == null)
                throw new ArgumentNullException("Оцiнка не може бути порожньою.");

            Student student = students.FirstOrDefault(s => s.Id == grade.StudentId);

            if (student == null)
                throw new Exception("Студента не знайдено.");

            if (!subjects.Any(s => s.Id == grade.SubjectId))
                throw new Exception("Навчальну дисциплiну не знайдено.");

            if (!student.SubjectIds.Contains(grade.SubjectId))
                throw new Exception("Студент не записаний на цю дисциплiну.");

            Teacher teacher = teachers.FirstOrDefault(t => t.Id == grade.TeacherId);

            if (teacher == null)
                throw new Exception("Викладача не знайдено.");

            if (!teacher.SubjectIds.Contains(grade.SubjectId))
                throw new Exception("Викладач не призначений на цю дисциплiну.");

            grades.Add(grade);
        }
        // Додавання оцінки викладачем із перевіркою прав доступу
        public void AddGradeForTeacher(Grade grade, Guid authenticatedTeacherId)
        {
            if (grade == null)
                throw new ArgumentNullException(nameof(grade));

            if (authenticatedTeacherId == Guid.Empty)
                throw new ArgumentException("Некоректний ідентифікатор викладача.");

            if (grade.TeacherId != authenticatedTeacherId)
                throw new Exception("Викладач не може виставляти оцінки від імені іншого викладача.");

            Teacher teacher = teachers.FirstOrDefault(
                t => t.Id == authenticatedTeacherId);

            if (teacher == null)
                throw new Exception("Викладача не знайдено.");

            if (!teacher.SubjectIds.Contains(grade.SubjectId))
                throw new Exception("Викладач не має доступу до цієї дисципліни.");

            AddGrade(grade);
        }

        // Редагування оцiнки
        public void EditGrade(Guid gradeId, double numericGrade, string letterGrade)
        {
            Grade grade = grades.FirstOrDefault(g => g.Id == gradeId);

            if (grade == null)
                throw new Exception("Оцiнку не знайдено.");

            grade.UpdateGrade(numericGrade, letterGrade);
        }

        // Видалення оцiнки
        public void DeleteGrade(Guid gradeId)
        {
            Grade grade = grades.FirstOrDefault(g => g.Id == gradeId);

            if (grade == null)
                throw new Exception("Оцiнку не знайдено.");

            grades.Remove(grade);
        }

        // Додавання кафедри
        // Додавання нової кафедри
        public void AddDepartment(Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            if (string.IsNullOrWhiteSpace(department.Name))
                throw new ArgumentException("Назва кафедри не може бути порожньою.");

            string departmentName = department.Name.Trim();

            if (departments.Any(d =>
                string.Equals(
                    d.Name.Trim(),
                    departmentName,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Така кафедра вже існує.");
            }

            departments.Add(department);
        }

        // Отримання всіх кафедр
        public List<Department> GetAllDepartments()
        {
            return departments.ToList();
        }

        // Призначення викладача на кафедру
        public void AssignTeacherToDepartment(Guid teacherId, Guid departmentId)
        {
            Teacher teacher = teachers.FirstOrDefault(t => t.Id == teacherId);

            if (teacher == null)
                throw new Exception("Викладача не знайдено.");

            Department department = departments.FirstOrDefault(d => d.Id == departmentId);

            if (department == null)
                throw new Exception("Кафедру не знайдено.");

            if (department.TeacherIds.Contains(teacherId))
                throw new Exception("Викладач вже працює на цій кафедрі.");

            // Видаляємо викладача з попередніх кафедр
            foreach (var existingDepartment in departments)
            {
                if (existingDepartment.Id != departmentId)
                    existingDepartment.RemoveTeacher(teacherId);
            }

            department.AssignTeacher(teacherId);

            // Синхронізуємо назву кафедри у викладача
            teacher.UpdateInfo(teacher.FullName, department.Name);
        }

        // Призначення завідувача кафедри
        public void SetDepartmentHead(Guid departmentId, Guid teacherId)
        {
            Department department = departments.FirstOrDefault(
                d => d.Id == departmentId);

            if (department == null)
                throw new Exception("Кафедру не знайдено.");

            Teacher teacher = teachers.FirstOrDefault(
                t => t.Id == teacherId);

            if (teacher == null)
                throw new Exception("Викладача не знайдено.");

            if (!department.TeacherIds.Contains(teacherId))
                throw new Exception(
                    "Викладач не належить до цієї кафедри.");

            department.SetHeadTeacher(teacherId);
        }

        // Отримання викладачів кафедри
        public List<Teacher> GetDepartmentTeachers(Guid departmentId)
        {
            Department department = departments.FirstOrDefault(d => d.Id == departmentId);

            if (department == null)
                throw new Exception("Кафедру не знайдено.");

            return teachers
                .Where(t => department.TeacherIds.Contains(t.Id))
                .ToList();
        }

        // Отримання дисциплін кафедри
        public List<Subject> GetDepartmentSubjects(Guid departmentId)
        {
            Department department = departments.FirstOrDefault(d => d.Id == departmentId);

            if (department == null)
                throw new Exception("Кафедру не знайдено.");

            List<Guid> subjectIds = teachers
                .Where(t => department.TeacherIds.Contains(t.Id))
                .SelectMany(t => t.SubjectIds)
                .Distinct()
                .ToList();

            return subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToList();
        }

        // Отримання середнього балу кафедри
        public double GetDepartmentAverage(Guid departmentId)
        {
            List<Guid> subjectIds = GetDepartmentSubjects(departmentId)
                .Select(s => s.Id)
                .ToList();

            List<Grade> departmentGrades = grades
                .Where(g => subjectIds.Contains(g.SubjectId))
                .ToList();

            if (!departmentGrades.Any())
                return 0;

            return departmentGrades.Average(g => g.NumericGrade);
        }

        // Отримання середнього балу кафедри за семестр
        public double GetDepartmentSemesterAverage(Guid departmentId, int semester)
        {
            if (semester < 1 || semester > 2)
                throw new ArgumentException("Некоректний семестр.");

            List<Guid> subjectIds = GetDepartmentSubjects(departmentId)
                .Where(s => s.Semester == semester)
                .Select(s => s.Id)
                .ToList();

            List<Grade> semesterGrades = grades
                .Where(g => subjectIds.Contains(g.SubjectId))
                .ToList();

            if (!semesterGrades.Any())
                return 0;

            return semesterGrades.Average(g => g.NumericGrade);
        }

        // Кількість студентів кафедри за семестр
        public int GetDepartmentStudentsCount(Guid departmentId, int semester)
        {
            if (semester < 1 || semester > 2)
                throw new ArgumentException("Некоректний семестр.");

            List<Guid> subjectIds = GetDepartmentSubjects(departmentId)
                .Where(s => s.Semester == semester)
                .Select(s => s.Id)
                .ToList();

            return grades
                .Where(g => subjectIds.Contains(g.SubjectId))
                .Select(g => g.StudentId)
                .Distinct()
                .Count();
        }

        // Кількість незадовільних оцінок кафедри за семестр
        public int GetDepartmentFailingGradesCount(Guid departmentId, int semester)
        {
            if (semester < 1 || semester > 2)
                throw new ArgumentException("Некоректний семестр.");

            List<Guid> subjectIds = GetDepartmentSubjects(departmentId)
                .Where(s => s.Semester == semester)
                .Select(s => s.Id)
                .ToList();

            return grades.Count(g =>
                subjectIds.Contains(g.SubjectId) &&
                g.NumericGrade < 60);
        }

        // Отримання топ-5 студентів кафедри
        public List<(Student Student, double Average)> GetDepartmentTopStudents(Guid departmentId)
        {
            List<Guid> subjectIds = GetDepartmentSubjects(departmentId)
                .Select(s => s.Id)
                .ToList();

            return grades
                .Where(g => subjectIds.Contains(g.SubjectId))
                .GroupBy(g => g.StudentId)
                .Select(group =>
                {
                    Student student = students.FirstOrDefault(s => s.Id == group.Key);

                    return new
                    {
                        Student = student,
                        Average = group.Average(g => g.NumericGrade)
                    };
                })
                .Where(x => x.Student != null)
                .OrderByDescending(x => x.Average)
                .Take(5)
                .Select(x => (x.Student!, x.Average))
                .ToList();
        }

        // Отримання списку дисциплiн студента
        public List<Subject> GetStudentSubjects(Guid studentId)
        {
            Student student = students.FirstOrDefault(s => s.Id == studentId);

            if (student == null)
                throw new Exception("Студента не знайдено.");

            return subjects
                .Where(s => student.SubjectIds.Contains(s.Id))
                .ToList();
        }

        // Отримання оцiнок студента за певною дисциплiною
        public List<Grade> GetStudentGrades(Guid studentId, Guid subjectId)
        {
            if (!students.Any(s => s.Id == studentId))
                throw new Exception("Студента не знайдено.");

            return grades
                .Where(g => g.StudentId == studentId &&
                            g.SubjectId == subjectId)
                .ToList();
        }

        // Отримання всiх студентiв
        public List<Student> GetAllStudents()
        {
            return students.ToList();
        }

        // Отримання всiх викладачiв
        public List<Teacher> GetAllTeachers()
        {
            return teachers.ToList();
        }

        // Отримання всiх дисциплiн
        public List<Subject> GetAllSubjects()
        {
            return subjects.ToList();

        }
        // Отримання всiх оцiнок
        public List<Grade> GetAllGrades()
        {
            return grades.ToList();
        }
    }
}