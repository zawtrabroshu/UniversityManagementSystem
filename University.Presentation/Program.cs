using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using University.Persistence.Configuration;
using System;
using System.IO;
using University.BusinessLogic.Grading;
using System.Text;
using University.Domain.Entities;
using University.BusinessLogic.Services;
using University.Persistence.Storage;
using University.Presentation.Api;
using QuestPDF.Infrastructure;
using University.Presentation.Reports;

QuestPDF.Settings.License = LicenseType.Community;
Console.OutputEncoding = new UTF8Encoding(false);
using (var db = DatabaseFactory.Create())
{
    db.Database.EnsureCreated();

    var connection =
        (SqliteConnection)db.Database.GetDbConnection();

    connection.Open();

    using var command = connection.CreateCommand();

    // Перевірка стовпця AccountUsername у таблиці Students
    command.CommandText = "PRAGMA table_info('Students');";

    bool studentColumnExists = false;

    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            if (reader.GetString(1) == "AccountUsername")
            {
                studentColumnExists = true;
                break;
            }
        }
    }

    if (!studentColumnExists)
    {
        command.CommandText =
            "ALTER TABLE Students ADD COLUMN AccountUsername TEXT NULL;";

        command.ExecuteNonQuery();

        Console.WriteLine(
            "Стовпець AccountUsername успішно додано до Students.");
    }

    // Перевірка стовпця AccountUsername у таблиці Teachers
    command.CommandText = "PRAGMA table_info('Teachers');";

    bool teacherColumnExists = false;

    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            if (reader.GetString(1) == "AccountUsername")
            {
                teacherColumnExists = true;
                break;
            }
        }
    }

    if (!teacherColumnExists)
    {
        command.CommandText =
            "ALTER TABLE Teachers ADD COLUMN AccountUsername TEXT NULL;";

        command.ExecuteNonQuery();

        Console.WriteLine(
            "Стовпець AccountUsername успішно додано до Teachers.");
    }
}

// Введення тексту з підтримкою українських та російських символів
string ReadText()
{
    StringBuilder input = new StringBuilder();

    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (input.Length > 0)
            {
                input.Length--;
                Console.Write("\b \b");
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            input.Append(key.KeyChar);
            Console.Write(key.KeyChar);
        }
    }

    return input.ToString();
}
// Авторизація користувача
AuthApiService authService = new AuthApiService();

string userRole = "";
string currentUsername = "";

while (true)
{
    Console.Clear();

    Console.WriteLine("======================================");
    Console.WriteLine("        АВТОРИЗАЦІЯ В СИСТЕМІ");
    Console.WriteLine("======================================");

    Console.Write("Логін: ");
    string username = ReadText();

    Console.Write("Пароль: ");
    string password = ReadText();

    try
    {
        LoginResponse loginResult =
            await authService.LoginAsync(username, password);

        userRole = loginResult.Role;
        currentUsername = username;

        Console.WriteLine("\nАвторизація успішна!");
        Console.WriteLine($"Роль користувача: {userRole}");

        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nПомилка авторизації: {ex.Message}");
        Console.WriteLine("Натисніть будь-яку клавішу для повторної спроби...");
        Console.ReadKey(true);
    }
}
// Перевірка прав доступу користувача
bool CanAccessMenu(string role, string choice)
{
    if (role == "Admin")
        return true;

    if (role == "Student")
    {
        return new[] { "6", "9", "11", "14", "17" }
            .Contains(choice);
    }

    if (role == "Teacher")
    {
        return new[] { "5", "6", "8", "10", "11", "12", "13", "20" }
            .Contains(choice);
    }

    if (role == "DepartmentHead")
    {
        return new[] { "21", "22", "23", "24", "27" }
            .Contains(choice);
    }

    return false;
}

// Відображення динамічного головного меню
void PrintMainMenu(string role, string username, string displayName)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║              UNIVERSITY MANAGEMENT              ║");
    Console.WriteLine("║          СИСТЕМА УПРАВЛІННЯ НАВЧАННЯМ           ║");
    Console.WriteLine("╚══════════════════════════════════════════════════╝");

    Console.ResetColor();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"  Користувач: {displayName}");
    Console.WriteLine($"  Логін:      {username}");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"  Дата:       {DateTime.Now:dd.MM.yyyy HH:mm}");
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.Green;
    string roleDisplayName = role switch
    {
        "Admin" => "АДМІНІСТРАТОР",
        "Student" => "СТУДЕНТ",
        "Teacher" => "ВИКЛАДАЧ",
        "DepartmentHead" => "ЗАВІДУВАЧ КАФЕДРИ",
        _ => "НЕВІДОМА РОЛЬ"
    };

    Console.WriteLine($"  Роль:       {roleDisplayName}");
    Console.ResetColor();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  ──────────────────────────────────────────────────");
    Console.ResetColor();

    void PrintItem(string number, string title)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  [{number}] ");

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(title);

        Console.ResetColor();
    }

    if (role == "Admin")
    {
        Console.WriteLine("\n  УПРАВЛІННЯ СИСТЕМОЮ");

        PrintItem("1", "Додати студента");
        PrintItem("2", "Додати викладача");
        PrintItem("3", "Додати навчальну дисципліну");
        PrintItem("4", "Переглянути студентів");
        PrintItem("5", "Переглянути викладачів");
        PrintItem("6", "Переглянути дисципліни");
        PrintItem("7", "Призначити дисципліну студенту");
        PrintItem("8", "Призначити дисципліну викладачу");
        PrintItem("9", "Переглянути дисципліни студента");
        PrintItem("10", "Виставити оцінку студенту");
        PrintItem("11", "Переглянути оцінки студента");
        PrintItem("12", "Редагувати оцінку");
        PrintItem("13", "Видалити оцінку");

        Console.WriteLine("\n  АНАЛІТИКА ТА ЗВІТИ");

        PrintItem("14", "Середній бал студента");
        PrintItem("15", "Середній бал за дисципліною");
        PrintItem("16", "Середній бал за семестр");
        PrintItem("17", "Академічна виписка студента");
        PrintItem("18", "Зведена таблиця успішності");
        PrintItem("19", "Статистика успішності групи");
        PrintItem("20", "Звіт викладача");
        PrintItem("21", "Звіт завідувача кафедри");
        PrintItem("22", "ТОП-5 студентів");
        PrintItem("23", "Рейтинг навчальних груп");
        PrintItem("24", "Фільтрація та пошук даних");
        PrintItem("25", "Шкала оцінювання");

        Console.WriteLine("\n  УПРАВЛІННЯ КАФЕДРАМИ");

        PrintItem("26", "Додати кафедру");
        PrintItem("27", "Переглянути кафедри");
        PrintItem("28", "Призначити викладача кафедрі");
        PrintItem("29", "Призначити завідувача кафедри");
        PrintItem("30", "Прив'язати акаунт студента");
        PrintItem("31", "Прив'язати акаунт викладача");
    }
    else if (role == "Student")
    {
        Console.WriteLine("\n  МОЄ НАВЧАННЯ");

        PrintItem("6", "Мої дисципліни");
        PrintItem("9", "Переглянути мої дисципліни");
        PrintItem("11", "Мої оцінки");
        PrintItem("14", "Мій середній бал");
        PrintItem("17", "Моя академічна виписка");
    }
    else if (role == "Teacher")
    {
        Console.WriteLine("\n  РОБОТА ВИКЛАДАЧА");

        PrintItem("5", "Переглянути викладачів");
        PrintItem("6", "Мої дисципліни");
        PrintItem("8", "Призначити дисципліну викладачу");
        PrintItem("10", "Виставити оцінку студенту");
        PrintItem("11", "Переглянути оцінки студента");
        PrintItem("12", "Редагувати оцінку");
        PrintItem("13", "Видалити оцінку");

        Console.WriteLine("\n  МОЯ АНАЛІТИКА");

        PrintItem("20", "Мій звіт викладача");
    }
    else if (role == "DepartmentHead")
    {
        Console.WriteLine("\n  КЕРУВАННЯ КАФЕДРОЮ");

        PrintItem("21", "Звіт моєї кафедри");
        PrintItem("22", "ТОП-5 студентів кафедри");
        PrintItem("23", "Рейтинг навчальних груп");
        PrintItem("24", "Фільтрація студентів");
        PrintItem("27", "Переглянути мою кафедру");
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  ──────────────────────────────────────────────────");
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  [0] Вийти з системи");
    Console.WriteLine("  [99] Завершити роботу програми");
    Console.ResetColor();

    Console.Write("\n  Ваш вибір: ");
}

UniversityService university = new UniversityService();
JsonStorageService storage = new JsonStorageService();
GradingService grading = new GradingService();

string filePath = Path.Combine(
    AppContext.BaseDirectory,
    "university-data.json"
);

// Завантаження даних
UniversityData data = storage.LoadData(filePath);

// Завантаження даних із SQLite
using (var db = DatabaseFactory.Create())
{
    db.Database.EnsureCreated();

    bool databaseIsEmpty =
        !db.Students.Any() &&
        !db.Teachers.Any() &&
        !db.Subjects.Any() &&
        !db.Grades.Any() &&
        !db.Departments.Any();

    // Одноразове перенесення старих даних із JSON
    if (databaseIsEmpty)
    {
        UniversityData oldData = storage.LoadData(filePath);

        db.Students.AddRange(oldData.Students);
        db.Teachers.AddRange(oldData.Teachers);
        db.Subjects.AddRange(oldData.Subjects);
        db.Grades.AddRange(oldData.Grades);
        db.Departments.AddRange(oldData.Departments);

        db.SaveChanges();

        Console.WriteLine("Дані перенесено з JSON до SQLite.");
    }

    // Завантаження студентів
    foreach (var student in db.Students.AsNoTracking().ToList())
        university.AddStudent(student);

    // Завантаження викладачів
    foreach (var teacher in db.Teachers.AsNoTracking().ToList())
        university.AddTeacher(teacher);

    // Завантаження дисциплін
    foreach (var subject in db.Subjects.AsNoTracking().ToList())
        university.AddSubject(subject);

    // Завантаження оцінок
    foreach (var grade in db.Grades.AsNoTracking().ToList())
        university.AddGrade(grade);

    // Завантаження кафедр
    foreach (var department in db.Departments.AsNoTracking().ToList())
        university.AddDepartment(department);
}
// Отримання студента, який увійшов до системи
Student? loggedInStudent = null;

if (userRole == "Student")
{
    loggedInStudent = university.GetStudentByUsername(currentUsername);

    Console.WriteLine(
        $"Вітаємо, {loggedInStudent.FullName}!");
}
// Отримання викладача, який увійшов до системи
Teacher? loggedInTeacher = null;

if (userRole == "Teacher" || userRole == "DepartmentHead")
{
    loggedInTeacher = university.GetTeacherByUsername(currentUsername);

    Console.WriteLine(
        $"Вітаємо, {loggedInTeacher.FullName}!");
}
// Збереження даних

// Збереження даних у SQLite
void SaveData()
{
    using (var db = DatabaseFactory.Create())
    {
        db.Database.EnsureCreated();

        using (var transaction = db.Database.BeginTransaction())
        {
            // Видаляємо старі записи
            db.Grades.RemoveRange(db.Grades.ToList());
            db.Departments.RemoveRange(db.Departments.ToList());
            db.Students.RemoveRange(db.Students.ToList());
            db.Teachers.RemoveRange(db.Teachers.ToList());
            db.Subjects.RemoveRange(db.Subjects.ToList());

            db.SaveChanges();

            // Зберігаємо актуальні дані
            db.Students.AddRange(university.GetAllStudents());
            db.Teachers.AddRange(university.GetAllTeachers());
            db.Subjects.AddRange(university.GetAllSubjects());
            db.Grades.AddRange(university.GetAllGrades());
            db.Departments.AddRange(university.GetAllDepartments());

            db.SaveChanges();

            transaction.Commit();
        }
    }
}


// Головне меню
bool isRunning = true;

while (isRunning)
{
    Console.Clear();

    string displayName = userRole switch
    {
        "Student" => loggedInStudent?.FullName ?? currentUsername,
        "Teacher" => loggedInTeacher?.FullName ?? currentUsername,
        _ => currentUsername
    };

    PrintMainMenu(userRole, currentUsername, displayName);

    string choice = ReadText();


    try
    {
        if (!CanAccessMenu(userRole, choice) &&
            choice != "0" &&
            choice != "99")
        {
            Console.WriteLine("У вас немає прав доступу до цього пункту!");
        }
        else
        {
            switch (choice)
            {
                case "1":
                    Console.Write("Введiть ПIБ студента: ");
                    string fullName = ReadText();

                    Console.Write("Введiть групу: ");
                    string group = ReadText();

                    Console.Write("Введiть рiк вступу: ");

                    if (!int.TryParse(ReadText(), out int year))
                    {
                        Console.WriteLine("Некоректний рiк!");
                        break;
                    }

                    university.AddStudent(
                        new Student(fullName, group, year)
                    );

                    Console.WriteLine("Студента успiшно додано!");
                    break;

                case "2":
                    Console.Write("Введiть ПIБ викладача: ");
                    string teacherName = ReadText();

                    Console.Write("Введiть назву кафедри: ");
                    string department = ReadText();

                    university.AddTeacher(
                        new Teacher(teacherName, department)
                    );

                    Console.WriteLine("Викладача успiшно додано!");
                    break;

                case "3":
                    Console.Write("Введiть назву дисциплiни: ");
                    string subjectName = ReadText();

                    Console.Write("Введiть номер семестру: ");

                    if (!int.TryParse(ReadText(), out int semester))
                    {
                        Console.WriteLine("Некоректний семестр!");
                        break;
                    }

                    Console.Write("Введiть кiлькiсть кредитiв: ");

                    if (!int.TryParse(ReadText(), out int credits))
                    {
                        Console.WriteLine("Некоректна кiлькiсть кредитiв!");
                        break;
                    }

                    university.AddSubject(
                        new Subject(subjectName, semester, credits)
                    );

                    Console.WriteLine("Дисциплiну успiшно додано!");
                    break;

                case "4":
                    var students = university.GetAllStudents();

                    Console.WriteLine("\nСПИСОК СТУДЕНТIВ:");

                    foreach (var student in students)
                    {
                        Console.WriteLine(
                            $"{student.FullName} | Група: {student.Group} | Рiк вступу: {student.AdmissionYear}"
                        );
                    }
                    break;

                case "5":
                    var teachers = university.GetAllTeachers();

                    Console.WriteLine("\nСПИСОК ВИКЛАДАЧIВ:");

                    foreach (var teacher in teachers)
                    {
                        Console.WriteLine(
                            $"{teacher.FullName} | Кафедра: {teacher.Department}"
                        );
                    }
                    break;

                case "6":
                    {
                        List<Subject> subjects;

                        if (userRole == "Student")
                        {
                            if (loggedInStudent == null)
                            {
                                Console.WriteLine("Обліковий запис студента не знайдено!");
                                break;
                            }

                            subjects = university.GetAllSubjects()
                                .Where(s => loggedInStudent.SubjectIds.Contains(s.Id))
                                .ToList();

                            Console.WriteLine(
                                $"\nДИСЦИПЛІНИ СТУДЕНТА: {loggedInStudent.FullName}");
                        }
                        else if (userRole == "Teacher")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис викладача не знайдено!");
                                break;
                            }

                            subjects = university.GetAllSubjects()
                                .Where(s => loggedInTeacher.SubjectIds.Contains(s.Id))
                                .ToList();

                            Console.WriteLine(
                                $"\nДИСЦИПЛІНИ ВИКЛАДАЧА: {loggedInTeacher.FullName}");
                        }
                        else
                        {
                            subjects = university.GetAllSubjects();
                            Console.WriteLine("\nСПИСОК ДИСЦИПЛІН:");
                        }

                        if (subjects.Count == 0)
                        {
                            Console.WriteLine("Дисциплін не знайдено!");
                            break;
                        }

                        foreach (var subject in subjects)
                        {
                            Console.WriteLine(
                                $"{subject.Name} | Семестр: {subject.Semester} | Кредити: {subject.Credits}");
                        }

                        break;
                    }

                case "7":
                    var assignmentStudents = university.GetAllStudents();
                    var assignmentSubjects = university.GetAllSubjects();

                    if (assignmentStudents.Count == 0 || assignmentSubjects.Count == 0)
                    {
                        Console.WriteLine("Немає студентiв або дисциплiн!");
                        break;
                    }

                    Console.WriteLine("\nСПИСОК СТУДЕНТIВ:");

                    for (int i = 0; i < assignmentStudents.Count; i++)
                        Console.WriteLine($"{i + 1}. {assignmentStudents[i].FullName}");

                    Console.Write("Оберiть номер студента: ");

                    if (!int.TryParse(ReadText(), out int studentChoice) ||
                        studentChoice < 1 || studentChoice > assignmentStudents.Count)
                    {
                        Console.WriteLine("Некоректний номер!");
                        break;
                    }

                    Console.WriteLine("\nСПИСОК ДИСЦИПЛIН:");

                    for (int i = 0; i < assignmentSubjects.Count; i++)
                        Console.WriteLine($"{i + 1}. {assignmentSubjects[i].Name}");

                    Console.Write("Оберiть номер дисциплiни: ");

                    if (!int.TryParse(ReadText(), out int subjectChoice) ||
                        subjectChoice < 1 || subjectChoice > assignmentSubjects.Count)
                    {
                        Console.WriteLine("Некоректний номер!");
                        break;
                    }

                    university.AssignSubjectToStudent(
                        assignmentStudents[studentChoice - 1].Id,
                        assignmentSubjects[subjectChoice - 1].Id
                    );

                    Console.WriteLine("Дисциплiну успiшно призначено студенту!");
                    break;

                case "8":
                    var teacherSelectionList = university.GetAllTeachers();
                    var teacherSubjects = university.GetAllSubjects();

                    if (teacherSelectionList.Count == 0 || teacherSubjects.Count == 0)
                    {
                        Console.WriteLine("Немає викладачiв або дисциплiн!");
                        break;
                    }

                    Console.WriteLine("\nСПИСОК ВИКЛАДАЧIВ:");

                    for (int i = 0; i < teacherSelectionList.Count; i++)
                        Console.WriteLine($"{i + 1}. {teacherSelectionList[i].FullName}");

                    Console.Write("Оберiть номер викладача: ");

                    if (!int.TryParse(ReadText(), out int teacherChoice) ||
                        teacherChoice < 1 || teacherChoice > teacherSelectionList.Count)
                    {
                        Console.WriteLine("Некоректний номер!");
                        break;
                    }

                    Console.WriteLine("\nСПИСОК ДИСЦИПЛIН:");

                    for (int i = 0; i < teacherSubjects.Count; i++)
                        Console.WriteLine($"{i + 1}. {teacherSubjects[i].Name}");

                    Console.Write("Оберiть номер дисциплiни: ");

                    if (!int.TryParse(ReadText(), out int teacherSubjectChoice) ||
                        teacherSubjectChoice < 1 || teacherSubjectChoice > teacherSubjects.Count)
                    {
                        Console.WriteLine("Некоректний номер!");
                        break;
                    }

                    university.AssignSubjectToTeacher(
                        teacherSelectionList[teacherChoice - 1].Id,
                        teacherSubjects[teacherSubjectChoice - 1].Id
                    );

                    Console.WriteLine("Дисциплiну успiшно призначено викладачу!");
                    break;

                case "9":
                    if (userRole == "Student")
                    {
                        if (loggedInStudent == null)
                        {
                            Console.WriteLine("Обліковий запис студента не знайдено!");
                            break;
                        }

                        var ownSubjects = university.GetStudentSubjects(
                            loggedInStudent.Id);

                        Console.WriteLine(
                            $"\nДИСЦИПЛІНИ СТУДЕНТА: {loggedInStudent.FullName}");

                        if (ownSubjects.Count == 0)
                        {
                            Console.WriteLine("Вам поки не призначено дисциплін.");
                        }
                        else
                        {
                            foreach (var subject in ownSubjects)
                            {
                                Console.WriteLine(
                                    $"- {subject.Name} | Семестр: {subject.Semester} | Кредити: {subject.Credits}");
                            }
                        }

                        break;
                    }

                    var viewStudents = university.GetAllStudents();
                    if (viewStudents.Count == 0)
                    {
                        Console.WriteLine("Список студентiв порожнiй!");
                        break;
                    }

                    Console.WriteLine("\nСПИСОК СТУДЕНТIВ:");

                    for (int i = 0; i < viewStudents.Count; i++)
                        Console.WriteLine($"{i + 1}. {viewStudents[i].FullName}");

                    Console.Write("Оберiть номер студента: ");

                    if (!int.TryParse(ReadText(), out int viewStudentChoice) ||
                        viewStudentChoice < 1 || viewStudentChoice > viewStudents.Count)
                    {
                        Console.WriteLine("Некоректний номер!");
                        break;
                    }

                    var studentSubjects = university.GetStudentSubjects(
                        viewStudents[viewStudentChoice - 1].Id
                    );

                    Console.WriteLine("\nДИСЦИПЛIНИ СТУДЕНТА:");

                    if (studentSubjects.Count == 0)
                        Console.WriteLine("Студент поки не має призначених дисциплiн.");

                    foreach (var subject in studentSubjects)
                        Console.WriteLine($"- {subject.Name}");

                    break;

                case "10":
                    {
                        var gradeStudentsForEntry = university.GetAllStudents();

                        if (gradeStudentsForEntry.Count == 0)
                        {
                            Console.WriteLine("Немає студентів!");
                            break;
                        }

                        Console.WriteLine("\nСПИСОК СТУДЕНТІВ:");

                        for (int i = 0; i < gradeStudentsForEntry.Count; i++)
                            Console.WriteLine($"{i + 1}. {gradeStudentsForEntry[i].FullName}");

                        Console.Write("Оберіть студента: ");

                        if (!int.TryParse(ReadText(), out int gradeStudentChoice10) ||
                            gradeStudentChoice10 < 1 ||
                            gradeStudentChoice10 > gradeStudentsForEntry.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        var selectedStudentForGrade10 =
                            gradeStudentsForEntry[gradeStudentChoice10 - 1];

                        var availableSubjectsForGrade10 =
                            university.GetStudentSubjects(selectedStudentForGrade10.Id);

                        if (userRole == "Teacher")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис викладача не знайдено!");
                                break;
                            }

                            availableSubjectsForGrade10 = availableSubjectsForGrade10
                                .Where(s => loggedInTeacher.SubjectIds.Contains(s.Id))
                                .ToList();
                        }

                        if (availableSubjectsForGrade10.Count == 0)
                        {
                            Console.WriteLine("Немає доступних дисциплін для виставлення оцінки!");
                            break;
                        }

                        Console.WriteLine("\nДИСЦИПЛІНИ:");

                        for (int i = 0; i < availableSubjectsForGrade10.Count; i++)
                            Console.WriteLine(
                                $"{i + 1}. {availableSubjectsForGrade10[i].Name}");

                        Console.Write("Оберіть дисципліну: ");

                        if (!int.TryParse(ReadText(), out int gradeSubjectChoice10) ||
                            gradeSubjectChoice10 < 1 ||
                            gradeSubjectChoice10 > availableSubjectsForGrade10.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        var selectedSubjectForGrade10 =
                            availableSubjectsForGrade10[gradeSubjectChoice10 - 1];

                        Teacher selectedTeacherForGrade10;

                        if (userRole == "Teacher")
                        {
                            selectedTeacherForGrade10 = loggedInTeacher!;

                            Console.WriteLine(
                                $"Викладач: {selectedTeacherForGrade10.FullName}");
                        }
                        else
                        {
                            var availableTeachersForGrade10 = university.GetAllTeachers()
                                .Where(t => t.SubjectIds.Contains(selectedSubjectForGrade10.Id))
                                .ToList();

                            if (availableTeachersForGrade10.Count == 0)
                            {
                                Console.WriteLine(
                                    "Для цієї дисципліни немає призначених викладачів!");
                                break;
                            }

                            Console.WriteLine("\nВИКЛАДАЧІ:");

                            for (int i = 0; i < availableTeachersForGrade10.Count; i++)
                                Console.WriteLine(
                                    $"{i + 1}. {availableTeachersForGrade10[i].FullName}");

                            Console.Write("Оберіть викладача: ");

                            if (!int.TryParse(ReadText(), out int gradeTeacherChoice10) ||
                                gradeTeacherChoice10 < 1 ||
                                gradeTeacherChoice10 > availableTeachersForGrade10.Count)
                            {
                                Console.WriteLine("Некоректний номер!");
                                break;
                            }

                            selectedTeacherForGrade10 =
                                availableTeachersForGrade10[gradeTeacherChoice10 - 1];
                        }

                        Console.Write("Введіть оцінку від 0 до 100: ");

                        if (!double.TryParse(
                            ReadText(),
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double numericGrade10) ||
                            numericGrade10 < 0 ||
                            numericGrade10 > 100)
                        {
                            Console.WriteLine("Оцінка повинна бути від 0 до 100!");
                            break;
                        }

                        string letterGrade10 =
                            grading.ConvertToLetterGrade(numericGrade10);

                        Grade newGrade10 = new Grade(
                            selectedStudentForGrade10.Id,
                            selectedSubjectForGrade10.Id,
                            selectedTeacherForGrade10.Id,
                            numericGrade10,
                            letterGrade10
                        );

                        if (userRole == "Teacher")
                        {
                            university.AddGradeForTeacher(
                                newGrade10,
                                loggedInTeacher!.Id);
                        }
                        else
                        {
                            university.AddGrade(newGrade10);
                        }

                        Console.WriteLine("Оцінку успішно додано!");
                        Console.WriteLine($"Числова оцінка: {numericGrade10}");
                        Console.WriteLine($"Літерна оцінка: {letterGrade10}");

                        break;
                    }

                case "11":
                    {
                        Student chosenStudent;

                        if (userRole == "Student")
                        {
                            if (loggedInStudent == null)
                            {
                                Console.WriteLine("Обліковий запис студента не знайдено!");
                                break;
                            }

                            chosenStudent = loggedInStudent;
                        }
                        else
                        {
                            var gradeViewStudents = university.GetAllStudents();

                            if (gradeViewStudents.Count == 0)
                            {
                                Console.WriteLine("Немає студентів!");
                                break;
                            }

                            Console.WriteLine("\nСПИСОК СТУДЕНТІВ:");

                            for (int i = 0; i < gradeViewStudents.Count; i++)
                                Console.WriteLine($"{i + 1}. {gradeViewStudents[i].FullName}");

                            Console.Write("Оберіть студента: ");

                            if (!int.TryParse(ReadText(), out int selectedGradeStudent) ||
                                selectedGradeStudent < 1 ||
                                selectedGradeStudent > gradeViewStudents.Count)
                            {
                                Console.WriteLine("Некоректний номер!");
                                break;
                            }

                            chosenStudent = gradeViewStudents[selectedGradeStudent - 1];
                        }

                        var studentSubjectsForGrades =
                            university.GetStudentSubjects(chosenStudent.Id);

                        if (studentSubjectsForGrades.Count == 0)
                        {
                            Console.WriteLine("У студента немає дисциплін!");
                            break;
                        }

                        Console.WriteLine($"\nОЦІНКИ СТУДЕНТА: {chosenStudent.FullName}");

                        foreach (var subject in studentSubjectsForGrades)
                        {
                            var subjectGrades = university.GetStudentGrades(
                                chosenStudent.Id,
                                subject.Id
                            );

                            Console.WriteLine($"\nДисципліна: {subject.Name}");

                            if (subjectGrades.Count == 0)
                            {
                                Console.WriteLine("Оцінок поки немає.");
                                continue;
                            }

                            foreach (var grade in subjectGrades)
                            {
                                Console.WriteLine(
                                    $"Бал: {grade.NumericGrade} | Літерна оцінка: {grade.LetterGrade} | Дата: {grade.Date:dd.MM.yyyy}"
                                );
                            }
                        }

                        break;
                    }

                case "12":
                    {
                        var editStudents = university.GetAllStudents();

                        if (editStudents.Count == 0)
                        {
                            Console.WriteLine("Немає студентiв!");
                            break;
                        }

                        Console.WriteLine("\nСПИСОК СТУДЕНТIВ:");

                        for (int i = 0; i < editStudents.Count; i++)
                            Console.WriteLine($"{i + 1}. {editStudents[i].FullName}");

                        Console.Write("Оберiть студента: ");

                        if (!int.TryParse(ReadText(), out int editStudentChoice) ||
                            editStudentChoice < 1 ||
                            editStudentChoice > editStudents.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        var selectedEditStudent = editStudents[editStudentChoice - 1];

                        var gradesToEdit = university.GetAllGrades()
                            .Where(g => g.StudentId == selectedEditStudent.Id)
                            .ToList();

                        if (gradesToEdit.Count == 0)
                        {
                            Console.WriteLine("У студента немає оцiнок!");
                            break;
                        }

                        var editSubjects = university.GetAllSubjects();

                        Console.WriteLine("\nОЦIНКИ СТУДЕНТА:");

                        for (int i = 0; i < gradesToEdit.Count; i++)
                        {
                            var subject = editSubjects
                                .FirstOrDefault(s => s.Id == gradesToEdit[i].SubjectId);

                            Console.WriteLine(
                                $"{i + 1}. {subject?.Name} | Бал: {gradesToEdit[i].NumericGrade} | Лiтерна оцiнка: {gradesToEdit[i].LetterGrade}");
                        }

                        Console.Write("Оберiть оцiнку для редагування: ");

                        if (!int.TryParse(ReadText(), out int editGradeChoice) ||
                            editGradeChoice < 1 ||
                            editGradeChoice > gradesToEdit.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        Console.Write("Введiть нову оцiнку вiд 0 до 100: ");

                        if (!double.TryParse(
                            ReadText(),
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double newNumericGrade) ||
                            newNumericGrade < 0 ||
                            newNumericGrade > 100)
                        {
                            Console.WriteLine("Оцiнка повинна бути вiд 0 до 100!");
                            break;
                        }

                        string newLetterGrade = grading.ConvertToLetterGrade(newNumericGrade);

                        university.EditGrade(
                            gradesToEdit[editGradeChoice - 1].Id,
                            newNumericGrade,
                            newLetterGrade
                        );

                        Console.WriteLine("\nОцiнку успiшно оновлено!");
                        Console.WriteLine($"Нова числова оцiнка: {newNumericGrade}");
                        Console.WriteLine($"Нова лiтерна оцiнка: {newLetterGrade}");

                        break;
                    }

                case "13":
                    var deleteStudents = university.GetAllStudents();

                    if (deleteStudents.Count == 0)
                    {
                        Console.WriteLine("Немає студентiв!");
                        break;
                    }

                    Console.WriteLine("\nСПИСОК СТУДЕНТIВ:");

                    for (int i = 0; i < deleteStudents.Count; i++)
                        Console.WriteLine($"{i + 1}. {deleteStudents[i].FullName}");

                    Console.Write("Оберiть студента: ");

                    if (!int.TryParse(ReadText(), out int deleteStudentChoice) ||
                        deleteStudentChoice < 1 ||
                        deleteStudentChoice > deleteStudents.Count)
                    {
                        Console.WriteLine("Некоректний номер!");
                        break;
                    }

                    var deleteStudent = deleteStudents[deleteStudentChoice - 1];

                    var gradesToDelete = university.GetAllGrades()
                        .Where(g => g.StudentId == deleteStudent.Id)
                        .ToList();

                    if (gradesToDelete.Count == 0)
                    {
                        Console.WriteLine("У студента немає оцiнок!");
                        break;
                    }

                    var allSubjectsForDelete = university.GetAllSubjects();

                    Console.WriteLine("\nОЦIНКИ СТУДЕНТА:");

                    for (int i = 0; i < gradesToDelete.Count; i++)
                    {
                        var subject = allSubjectsForDelete
                            .FirstOrDefault(s => s.Id == gradesToDelete[i].SubjectId);

                        Console.WriteLine(
                            $"{i + 1}. {subject?.Name} | Бал: {gradesToDelete[i].NumericGrade} | Лiтерна оцiнка: {gradesToDelete[i].LetterGrade}"
                        );
                    }

                    Console.Write("Оберiть оцiнку для видалення: ");

                    if (!int.TryParse(ReadText(), out int deleteGradeChoice) ||
                        deleteGradeChoice < 1 ||
                        deleteGradeChoice > gradesToDelete.Count)
                    {
                        Console.WriteLine("Некоректний номер!");
                        break;
                    }

                    Console.Write("Ви впевненi, що хочете видалити оцiнку? (так/нi): ");

                    string confirmation = ReadText().ToLower();

                    if (confirmation == "так" || confirmation == "т")
                    {
                        university.DeleteGrade(
                            gradesToDelete[deleteGradeChoice - 1].Id
                        );

                        Console.WriteLine("Оцiнку успiшно видалено!");
                    }
                    else
                    {
                        Console.WriteLine("Видалення скасовано.");
                    }

                    break;

                case "14":
                    {
                        Student selected;

                        if (userRole == "Student")
                        {
                            if (loggedInStudent == null)
                            {
                                Console.WriteLine("Обліковий запис студента не знайдено!");
                                break;
                            }

                            selected = loggedInStudent;
                        }
                        else
                        {
                            var averageStudents = university.GetAllStudents();

                            if (averageStudents.Count == 0)
                            {
                                Console.WriteLine("Немає студентів!");
                                break;
                            }

                            for (int i = 0; i < averageStudents.Count; i++)
                                Console.WriteLine($"{i + 1}. {averageStudents[i].FullName}");

                            Console.Write("Оберіть студента: ");

                            if (!int.TryParse(ReadText(), out int studentNumber) ||
                                studentNumber < 1 ||
                                studentNumber > averageStudents.Count)
                            {
                                Console.WriteLine("Некоректний номер!");
                                break;
                            }

                            selected = averageStudents[studentNumber - 1];
                        }

                        var studentGrades = university.GetAllGrades()
                            .Where(g => g.StudentId == selected.Id)
                            .ToList();

                        Console.WriteLine($"\nСЕРЕДНІЙ БАЛ: {selected.FullName}");

                        if (studentGrades.Count == 0)
                        {
                            Console.WriteLine("Оцінок немає!");
                            break;
                        }

                        Console.WriteLine(
                            $"Середній бал: {studentGrades.Average(g => g.NumericGrade):F2}");

                        break;
                    }

                case "15":
                    {
                        var allSubjects = university.GetAllSubjects();

                        for (int i = 0; i < allSubjects.Count; i++)
                            Console.WriteLine($"{i + 1}. {allSubjects[i].Name}");

                        Console.Write("Оберiть дисциплiну: ");

                        if (!int.TryParse(ReadText(), out int subjectNumber) ||
                            subjectNumber < 1 || subjectNumber > allSubjects.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        var selected = allSubjects[subjectNumber - 1];

                        var subjectGrades = university.GetAllGrades()
                            .Where(g => g.SubjectId == selected.Id).ToList();

                        if (subjectGrades.Count == 0)
                        {
                            Console.WriteLine("Оцiнок немає!");
                            break;
                        }

                        Console.WriteLine($"Середнiй бал з дисциплiни {selected.Name}: {subjectGrades.Average(g => g.NumericGrade):F2}");
                        break;
                    }

                case "16":
                    {
                        Console.Write("Введiть номер семестру: ");

                        if (!int.TryParse(ReadText(), out int semesterNumber))
                            break;

                        var semesterSubjects = university.GetAllSubjects()
                            .Where(s => s.Semester == semesterNumber)
                            .Select(s => s.Id)
                            .ToList();

                        var semesterGrades = university.GetAllGrades()
                            .Where(g => semesterSubjects.Contains(g.SubjectId))
                            .ToList();

                        if (semesterGrades.Count == 0)
                        {
                            Console.WriteLine("Оцiнок за цей семестр немає!");
                            break;
                        }

                        Console.WriteLine($"Середнiй бал за семестр: {semesterGrades.Average(g => g.NumericGrade):F2}");
                        break;
                    }

                case "17":
                    {
                        Student selected;

                        if (userRole == "Student")
                        {
                            if (loggedInStudent == null)
                            {
                                Console.WriteLine("Обліковий запис студента не знайдено!");
                                break;
                            }

                            selected = loggedInStudent;
                        }
                        else
                        {
                            var transcriptStudents = university.GetAllStudents();

                            if (transcriptStudents.Count == 0)
                            {
                                Console.WriteLine("Студентів поки немає!");
                                break;
                            }

                            for (int i = 0; i < transcriptStudents.Count; i++)
                                Console.WriteLine($"{i + 1}. {transcriptStudents[i].FullName}");

                            Console.Write("Оберіть студента: ");

                            if (!int.TryParse(ReadText(), out int transcriptChoice) ||
                                transcriptChoice < 1 ||
                                transcriptChoice > transcriptStudents.Count)
                            {
                                Console.WriteLine("Некоректний номер!");
                                break;
                            }

                            selected = transcriptStudents[transcriptChoice - 1];
                        }

                        Console.WriteLine($"\nАКАДЕМІЧНА ВИПИСКА: {selected.FullName}");
                        Console.WriteLine($"Група: {selected.Group}");
                        Console.WriteLine($"Рік вступу: {selected.AdmissionYear}");
                        Console.WriteLine("--------------------------------------");

                        var transcriptGrades = university.GetAllGrades()
                            .Where(g => g.StudentId == selected.Id)
                            .OrderBy(g => g.Date)
                            .ToList();

                        if (transcriptGrades.Count == 0)
                        {
                            Console.WriteLine("Оцінок поки немає.");
                            break;
                        }

                        foreach (var grade in transcriptGrades)
                        {
                            var subject = university.GetAllSubjects()
                                .FirstOrDefault(s => s.Id == grade.SubjectId);

                            Console.WriteLine(
                                $"{subject?.Name} | {grade.NumericGrade} | {grade.LetterGrade} | {grade.Date:dd.MM.yyyy}");
                        }

                        Console.WriteLine("--------------------------------------");

                        Console.WriteLine($"Середній бал: {transcriptGrades.Average(g => g.NumericGrade):F2}");

                        // Створення PDF-файлу академічної виписки
                        var transcriptSubjects = university.GetAllSubjects();

                        string pdfPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            $"Akademichna_vypyska_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                        var pdfService = new PdfReportService();

                        pdfService.GenerateStudentReport(
                            selected,
                            transcriptGrades,
                            transcriptSubjects,
                            pdfPath);

                        Console.WriteLine("\nPDF-файл успішно створено!");
                        Console.WriteLine($"Шлях до файлу: {pdfPath}");

                        break;
                    }
                case "18":
                    {
                        var allGrades = university.GetAllGrades();
                        var allSubjectsForReport = university.GetAllSubjects();

                        if (allGrades.Count == 0)
                        {
                            Console.WriteLine("Даних для звiту немає!");
                            break;
                        }

                        Console.WriteLine("\nЗВЕДЕНА УСПIШНIСТЬ:");

                        var yearlyGrades = allGrades
                            .GroupBy(g => g.Date.Year)
                            .OrderBy(g => g.Key);

                        foreach (var yearGroup in yearlyGrades)
                        {
                            Console.WriteLine($"\nНавчальний рiк за датою оцiнки: {yearGroup.Key}");

                            var semesterGroups = yearGroup.GroupBy(g =>
                                allSubjectsForReport.FirstOrDefault(s => s.Id == g.SubjectId)?.Semester ?? 0);

                            foreach (var semesterGroup in semesterGroups)
                            {
                                Console.WriteLine(
                                    $"Семестр {semesterGroup.Key}: середнiй бал {semesterGroup.Average(g => g.NumericGrade):F2}");
                            }
                        }

                        break;
                    }

                case "19":
                    {
                        var allStudentsForStats = university.GetAllStudents();
                        var allGradesForStats = university.GetAllGrades();

                        var groups = allStudentsForStats.GroupBy(s => s.Group);

                        Console.WriteLine("\nСТАТИСТИКА ГРУП:");

                        foreach (var studentGroup in groups)
                        {
                            var groupIds = studentGroup.Select(s => s.Id).ToList();

                            var groupGrades = allGradesForStats
                                .Where(g => groupIds.Contains(g.StudentId))
                                .ToList();

                            Console.WriteLine($"\nГрупа: {studentGroup.Key}");
                            Console.WriteLine($"Кiлькiсть студентiв: {studentGroup.Count()}");

                            if (groupGrades.Count > 0)
                            {
                                Console.WriteLine($"Середнiй бал: {groupGrades.Average(g => g.NumericGrade):F2}");
                                Console.WriteLine($"Кiлькiсть оцiнок: {groupGrades.Count}");
                                Console.WriteLine($"Успiшнi оцiнки: {groupGrades.Count(g => g.NumericGrade >= 60)}");
                            }
                            else
                            {
                                Console.WriteLine("Оцiнок ще немає.");
                            }
                        }

                        break;
                    }

                case "20":
                    {
                        Teacher selected;

                        if (userRole == "Teacher")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис викладача не знайдено!");
                                break;
                            }

                            selected = loggedInTeacher;
                        }
                        else
                        {
                            var reportTeachers = university.GetAllTeachers();

                            if (reportTeachers.Count == 0)
                            {
                                Console.WriteLine("Викладачів поки немає!");
                                break;
                            }

                            for (int i = 0; i < reportTeachers.Count; i++)
                                Console.WriteLine($"{i + 1}. {reportTeachers[i].FullName}");

                            Console.Write("Оберіть викладача: ");

                            if (!int.TryParse(ReadText(), out int reportTeacherChoice20) ||
                                reportTeacherChoice20 < 1 ||
                                reportTeacherChoice20 > reportTeachers.Count)
                            {
                                Console.WriteLine("Некоректний номер!");
                                break;
                            }

                            selected = reportTeachers[reportTeacherChoice20 - 1];
                        }

                        var teacherGrades = university.GetAllGrades()
                            .Where(g => g.TeacherId == selected.Id)
                            .ToList();

                        Console.WriteLine($"\nЗВІТ ВИКЛАДАЧА: {selected.FullName}");
                        Console.WriteLine($"Кафедра: {selected.Department}");
                        Console.WriteLine($"Кількість виставлених оцінок: {teacherGrades.Count}");

                        if (teacherGrades.Count > 0)
                        {
                            Console.WriteLine(
                                $"Середній бал: {teacherGrades.Average(g => g.NumericGrade):F2}");

                            foreach (var subjectId in teacherGrades
                                .Select(g => g.SubjectId)
                                .Distinct())
                            {
                                var subject = university.GetAllSubjects()
                                    .FirstOrDefault(s => s.Id == subjectId);

                                var grades = teacherGrades
                                    .Where(g => g.SubjectId == subjectId)
                                    .ToList();

                                Console.WriteLine(
                                    $"{subject?.Name}: {grades.Count} оцінок, " +
                                    $"середній бал {grades.Average(g => g.NumericGrade):F2}");
                            }
                        }

                        break;
                    }


                case "21":
                    {
                        var allDepartments = university.GetAllDepartments();

                        if (!allDepartments.Any())
                        {
                            Console.WriteLine("Кафедр не знайдено.");
                            break;
                        }

                        Department selectedDepartmentEntity;

                        if (userRole == "DepartmentHead")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис завідувача не знайдено.");
                                break;
                            }

                            var matchedDepartment = allDepartments.FirstOrDefault(d =>
                            d.HeadTeacherId.HasValue &&
                            d.HeadTeacherId.Value == loggedInTeacher.Id);

                            if (matchedDepartment == null)
                            {
                                Console.WriteLine("Ви не призначені завідувачем жодної кафедри.");
                                break;
                            }

                            selectedDepartmentEntity = matchedDepartment;
                        }
                        else
                        {
                            Console.WriteLine("\nСПИСОК КАФЕДР:");

                            for (int i = 0; i < allDepartments.Count; i++)
                                Console.WriteLine($"{i + 1}. {allDepartments[i].Name}");

                            Console.Write("\nОберiть кафедру: ");

                            if (!int.TryParse(ReadText(), out int departmentChoice) ||
                                departmentChoice < 1 ||
                                departmentChoice > allDepartments.Count)
                            {
                                Console.WriteLine("Некоректний вибiр.");
                                break;
                            }

                            selectedDepartmentEntity = allDepartments[departmentChoice - 1];
                        }

                        var departmentTeachers =
                            university.GetDepartmentTeachers(selectedDepartmentEntity.Id);

                        var departmentSubjects =
                            university.GetDepartmentSubjects(selectedDepartmentEntity.Id);

                        var departmentAverage =
                            university.GetDepartmentAverage(selectedDepartmentEntity.Id);
                        var semester1Average =
                            university.GetDepartmentSemesterAverage(selectedDepartmentEntity.Id, 1);

                        var semester2Average =
                            university.GetDepartmentSemesterAverage(selectedDepartmentEntity.Id, 2);

                        var semester1Students =
                            university.GetDepartmentStudentsCount(selectedDepartmentEntity.Id, 1);

                        var semester2Students =
                            university.GetDepartmentStudentsCount(selectedDepartmentEntity.Id, 2);

                        var semester1Fails =
                            university.GetDepartmentFailingGradesCount(selectedDepartmentEntity.Id, 1);

                        var semester2Fails =
                            university.GetDepartmentFailingGradesCount(selectedDepartmentEntity.Id, 2);

                        var departmentTopStudents =
                            university.GetDepartmentTopStudents(selectedDepartmentEntity.Id);

                        var departmentSubjectIds = departmentSubjects
                            .Select(s => s.Id)
                            .ToList();

                        var departmentGrades = university.GetAllGrades()
                            .Where(g => departmentSubjectIds.Contains(g.SubjectId))
                            .ToList();

                        var departmentHead = departmentTeachers.FirstOrDefault(
                            t => selectedDepartmentEntity.HeadTeacherId.HasValue &&
                                 t.Id == selectedDepartmentEntity.HeadTeacherId.Value);

                        Console.WriteLine("\n======================================");
                        Console.WriteLine($"ЗВІТ ЗАВІДУВАЧА КАФЕДРИ");
                        Console.WriteLine($"Кафедра: {selectedDepartmentEntity.Name}");
                        Console.WriteLine("======================================");

                        Console.WriteLine(
                            $"Завідувач: {departmentHead?.FullName ?? "Не призначений"}");

                        Console.WriteLine($"Кількість викладачів: {departmentTeachers.Count}");
                        Console.WriteLine($"Кількість дисциплін: {departmentSubjects.Count}");
                        Console.WriteLine($"Кількість оцінок: {departmentGrades.Count}");

                        if (departmentGrades.Any())
                            Console.WriteLine($"Середній бал кафедри: {departmentAverage:F2}");
                        else
                            Console.WriteLine("Середній бал кафедри: оцінок поки немає.");
                        Console.WriteLine("\nСТАТИСТИКА ЗА СЕМЕСТРАМИ:");

                        Console.WriteLine($"1 семестр:");
                        Console.WriteLine($"  Середній бал: {semester1Average:F2}");
                        Console.WriteLine($"  Кількість студентів: {semester1Students}");
                        Console.WriteLine($"  Незадовільних оцінок: {semester1Fails}");

                        Console.WriteLine($"2 семестр:");
                        Console.WriteLine($"  Середній бал: {semester2Average:F2}");
                        Console.WriteLine($"  Кількість студентів: {semester2Students}");
                        Console.WriteLine($"  Незадовільних оцінок: {semester2Fails}");
                        Console.WriteLine("\nВИКЛАДАЧІ:");

                        foreach (var teacher in departmentTeachers)
                        {
                            int teacherGradeCount = departmentGrades.Count(
                                g => g.TeacherId == teacher.Id);

                            Console.WriteLine(
                                $"- {teacher.FullName} ({teacher.Position}) — {teacherGradeCount} оцінок");
                        }

                        Console.WriteLine("\nДИСЦИПЛІНИ:");

                        foreach (var subject in departmentSubjects)
                        {
                            Console.WriteLine(
                                $"- {subject.Name} | Семестр: {subject.Semester} | Кредити: {subject.Credits}");
                        }

                        Console.WriteLine("\nТОП-5 СТУДЕНТІВ КАФЕДРИ:");

                        if (departmentTopStudents.Any())
                        {
                            for (int i = 0; i < departmentTopStudents.Count; i++)
                            {
                                var item = departmentTopStudents[i];

                                Console.WriteLine(
                                    $"{i + 1}. {item.Student.FullName} — середній бал: {item.Average:F2}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Недостатньо даних для формування рейтингу.");
                        }

                        Console.WriteLine("======================================");

                        break;
                    }


                case "22":
                    {
                        var studentsForRanking = university.GetAllStudents();

                        if (userRole == "DepartmentHead")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис завідувача не знайдено.");
                                break;
                            }

                            var departments = university.GetAllDepartments();

                            var headDepartment = departments.FirstOrDefault(d =>
                                d.HeadTeacherId.HasValue &&
                                d.HeadTeacherId.Value == loggedInTeacher.Id);

                            if (headDepartment == null)
                            {
                                Console.WriteLine("Ви не призначені завідувачем жодної кафедри.");
                                break;
                            }

                            var departmentSubjects =
                                university.GetDepartmentSubjects(headDepartment.Id);

                            var departmentSubjectIds = departmentSubjects
                                .Select(s => s.Id)
                                .ToList();

                            studentsForRanking = studentsForRanking
                                .Where(s => s.SubjectIds.Any(id => departmentSubjectIds.Contains(id)))
                                .ToList();

                            Console.WriteLine($"\nТОП-5 СТУДЕНТІВ КАФЕДРИ: {headDepartment.Name}");
                        }
                        else
                        {
                            Console.WriteLine("\nЗАГАЛЬНИЙ ТОП-5 СТУДЕНТІВ:");
                        }

                        var allGrades22 = university.GetAllGrades();

                        var topStudents = studentsForRanking
                            .Select(s => new
                            {
                                Student = s,
                                Grades = allGrades22
                                    .Where(g => g.StudentId == s.Id)
                                    .ToList()
                            })
                            .Where(x => x.Grades.Count > 0)
                            .OrderByDescending(x => x.Grades.Average(g => g.NumericGrade))
                            .Take(5)
                            .ToList();

                        if (topStudents.Count == 0)
                        {
                            Console.WriteLine("Недостатньо даних для формування рейтингу.");
                            break;
                        }

                        for (int i = 0; i < topStudents.Count; i++)
                        {
                            Console.WriteLine(
                                $"{i + 1}. {topStudents[i].Student.FullName} | " +
                                $"{topStudents[i].Student.Group} | " +
                                $"Середній бал: {topStudents[i].Grades.Average(g => g.NumericGrade):F2}");
                        }

                        break;
                    }


                case "23":
                    {
                        var allStudents = university.GetAllStudents();
                        var allGrades = university.GetAllGrades();

                        if (userRole == "DepartmentHead")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис завідувача не знайдено.");
                                break;
                            }

                            var headDepartment23 = university.GetAllDepartments()
                                .FirstOrDefault(d =>
                                    d.HeadTeacherId.HasValue &&
                                    d.HeadTeacherId.Value == loggedInTeacher.Id);

                            if (headDepartment23 == null)
                            {
                                Console.WriteLine("Ви не призначені завідувачем жодної кафедри.");
                                break;
                            }

                            var departmentSubjects23 =
                                university.GetDepartmentSubjects(headDepartment23.Id);

                            var departmentSubjectIds23 = departmentSubjects23
                                .Select(s => s.Id)
                                .ToList();

                            allStudents = allStudents
                                .Where(s => s.SubjectIds.Any(id =>
                                    departmentSubjectIds23.Contains(id)))
                                .ToList();

                            allGrades = allGrades
                                .Where(g => departmentSubjectIds23.Contains(g.SubjectId))
                                .ToList();

                            Console.WriteLine(
                                $"\nРЕЙТИНГ ГРУП КАФЕДРИ: {headDepartment23.Name}");
                        }
                        else
                        {
                            Console.WriteLine("\nЗАГАЛЬНИЙ РЕЙТИНГ НАВЧАЛЬНИХ ГРУП:");
                        }

                        var groupRanking = allStudents
                            .GroupBy(s => s.Group)
                            .Select(g =>
                            {
                                var studentIds = g.Select(s => s.Id).ToList();

                                var groupGrades = allGrades
                                    .Where(x => studentIds.Contains(x.StudentId))
                                    .ToList();

                                var studentAverages = g
                                    .Select(student =>
                                    {
                                        var studentGrades = groupGrades
                                            .Where(x => x.StudentId == student.Id)
                                            .ToList();

                                        return new
                                        {
                                            StudentId = student.Id,
                                            Average = studentGrades.Any()
                                                ? studentGrades.Average(x => x.NumericGrade)
                                                : 0
                                        };
                                    })
                                    .Where(x => groupGrades.Any(grade =>
                                        grade.StudentId == x.StudentId))
                                    .ToList();

                                return new
                                {
                                    Group = g.Key,
                                    StudentCount = g.Count(),
                                    Grades = groupGrades,
                                    Average = groupGrades.Any()
                                        ? groupGrades.Average(x => x.NumericGrade)
                                        : 0,
                                    ExcellentStudents = studentAverages.Count(x => x.Average >= 90),
                                    FailingStudents = g.Count(student =>
                                        groupGrades.Any(grade =>
                                            grade.StudentId == student.Id &&
                                            grade.NumericGrade < 60))
                                };
                            })
                            .OrderByDescending(g => g.Average)
                            .ToList();

                        Console.WriteLine("========================================");

                        if (groupRanking.Count == 0)
                        {
                            Console.WriteLine("Недостатньо даних для формування рейтингу.");
                            break;
                        }

                        for (int i = 0; i < groupRanking.Count; i++)
                        {
                            var groupResult = groupRanking[i];

                            Console.WriteLine($"\n{i + 1}. Група: {groupResult.Group}");
                            Console.WriteLine($"   Кількість студентів: {groupResult.StudentCount}");
                            Console.WriteLine($"   Кількість оцінок: {groupResult.Grades.Count}");

                            if (groupResult.Grades.Any())
                                Console.WriteLine($"   Середній бал: {groupResult.Average:F2}");
                            else
                                Console.WriteLine("   Середній бал: немає оцінок");

                            Console.WriteLine(
                                $"   Відмінники (середній бал від 90): {groupResult.ExcellentStudents}");

                            Console.WriteLine(
                                $"   Студенти з незадовільними оцінками: {groupResult.FailingStudents}");
                        }

                        Console.WriteLine("\n========================================");

                        break;
                    }



                case "24":
                    {
                        var allStudents = university.GetAllStudents();
                        var allSubjects = university.GetAllSubjects();
                        var allGrades = university.GetAllGrades();

                        if (userRole == "DepartmentHead")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис завідувача не знайдено.");
                                break;
                            }

                            var headDepartment24 = university.GetAllDepartments()
                                .FirstOrDefault(d =>
                                    d.HeadTeacherId.HasValue &&
                                    d.HeadTeacherId.Value == loggedInTeacher.Id);

                            if (headDepartment24 == null)
                            {
                                Console.WriteLine("Ви не призначені завідувачем жодної кафедри.");
                                break;
                            }

                            var departmentSubjects24 =
                                university.GetDepartmentSubjects(headDepartment24.Id);

                            var departmentSubjectIds24 = departmentSubjects24
                                .Select(s => s.Id)
                                .ToList();

                            allSubjects = allSubjects
                                .Where(s => departmentSubjectIds24.Contains(s.Id))
                                .ToList();

                            allStudents = allStudents
                                .Where(s => s.SubjectIds.Any(id =>
                                    departmentSubjectIds24.Contains(id)))
                                .ToList();

                            allGrades = allGrades
                                .Where(g => departmentSubjectIds24.Contains(g.SubjectId))
                                .ToList();

                            Console.WriteLine(
                                $"Доступні дані кафедри: {headDepartment24.Name}");
                        }

                        Console.WriteLine("\nФІЛЬТРАЦІЯ СТУДЕНТІВ");
                        Console.WriteLine("================================");

                        Console.Write("Введіть назву групи (Enter - усі групи): ");
                        string groupFilter = ReadText();

                        Console.Write("Введіть семестр (1 або 2, 0 - усі семестри): ");
                        if (!int.TryParse(ReadText(), out int semesterFilter) ||
                            semesterFilter < 0 || semesterFilter > 2)
                        {
                            Console.WriteLine("Некоректний номер семестру.");
                            break;
                        }

                        Console.Write("Мінімальний середній бал (Enter - без обмеження): ");
                        string minInput = ReadText();

                        double minAverage = 0;

                        if (!string.IsNullOrWhiteSpace(minInput) &&
                            !double.TryParse(minInput, out minAverage))
                        {
                            Console.WriteLine("Некоректне значення мінімального балу.");
                            break;
                        }

                        Console.Write("Максимальний середній бал (Enter - без обмеження): ");
                        string maxInput = ReadText();

                        double maxAverage = 100;

                        if (!string.IsNullOrWhiteSpace(maxInput) &&
                            !double.TryParse(maxInput, out maxAverage))
                        {
                            Console.WriteLine("Некоректне значення максимального балу.");
                            break;
                        }

                        if (minAverage < 0 || maxAverage > 100 ||
                            minAverage > maxAverage)
                        {
                            Console.WriteLine("Некоректний діапазон середнього балу.");
                            break;
                        }

                        var filteredStudents = allStudents
                            .Where(student =>
                                string.IsNullOrWhiteSpace(groupFilter) ||
                                student.Group.Equals(
                                    groupFilter,
                                    StringComparison.OrdinalIgnoreCase))
                            .Select(student =>
                            {
                                var studentSubjects = allSubjects
                                    .Where(subject =>
                                        student.SubjectIds.Contains(subject.Id) &&
                                        (semesterFilter == 0 ||
                                         subject.Semester == semesterFilter))
                                    .ToList();

                                var subjectIds = studentSubjects
                                    .Select(subject => subject.Id)
                                    .ToList();

                                var studentGrades = allGrades
                                    .Where(grade =>
                                        grade.StudentId == student.Id &&
                                        subjectIds.Contains(grade.SubjectId))
                                    .ToList();

                                double average = studentGrades.Any()
                                    ? studentGrades.Average(grade => grade.NumericGrade)
                                    : 0;

                                return new
                                {
                                    Student = student,
                                    Subjects = studentSubjects,
                                    Grades = studentGrades,
                                    Average = average
                                };
                            })
                            .Where(result =>
                                result.Subjects.Any() &&
                                result.Average >= minAverage &&
                                result.Average <= maxAverage)
                            .OrderByDescending(result => result.Average)
                            .ToList();

                        Console.WriteLine("\nРЕЗУЛЬТАТИ ФІЛЬТРАЦІЇ");
                        Console.WriteLine("================================");

                        if (!filteredStudents.Any())
                        {
                            Console.WriteLine("Студентів за заданими критеріями не знайдено.");
                        }
                        else
                        {
                            for (int i = 0; i < filteredStudents.Count; i++)
                            {
                                var result = filteredStudents[i];

                                Console.WriteLine($"\n{i + 1}. {result.Student.FullName}");
                                Console.WriteLine($"   Група: {result.Student.Group}");

                                if (semesterFilter == 0)
                                    Console.WriteLine("   Семестр: усі");
                                else
                                    Console.WriteLine($"   Семестр: {semesterFilter}");

                                Console.WriteLine($"   Кількість дисциплін: {result.Subjects.Count}");
                                Console.WriteLine($"   Кількість оцінок: {result.Grades.Count}");

                                if (result.Grades.Any())
                                    Console.WriteLine($"   Середній бал: {result.Average:F2}");
                                else
                                    Console.WriteLine("   Середній бал: немає оцінок");
                            }

                            Console.WriteLine("\n================================");
                            Console.WriteLine($"Знайдено студентів: {filteredStudents.Count}");
                        }

                        break;
                    }


                case "25":
                    {
                        Console.WriteLine("\nШКАЛА ОЦIНЮВАННЯ:");
                        Console.WriteLine("90-100: A");
                        Console.WriteLine("82-89:  B");
                        Console.WriteLine("74-81:  C");
                        Console.WriteLine("64-73:  D");
                        Console.WriteLine("60-63:  E");
                        Console.WriteLine("35-59:  Fx");
                        Console.WriteLine("0-34:   F");
                        Console.WriteLine("\nПоточна шкала оцiнювання використовується системою автоматично.");
                        break;
                    }

                case "26":
                    {
                        Console.Write("Введiть назву кафедри: ");
                        string departmentName = ReadText();

                        Department newDepartment = new Department(departmentName);

                        university.AddDepartment(newDepartment);

                        Console.WriteLine("Кафедру успiшно додано!");
                        break;
                    }

                case "27":
                    {
                        var allDepartments = university.GetAllDepartments();

                        if (allDepartments.Count == 0)
                        {
                            Console.WriteLine("Кафедр поки немає!");
                            break;
                        }

                        if (userRole == "DepartmentHead")
                        {
                            if (loggedInTeacher == null)
                            {
                                Console.WriteLine("Обліковий запис завідувача не знайдено.");
                                break;
                            }

                            var headDepartment27 = allDepartments.FirstOrDefault(d =>
                                d.HeadTeacherId.HasValue &&
                                d.HeadTeacherId.Value == loggedInTeacher.Id);

                            if (headDepartment27 == null)
                            {
                                Console.WriteLine("Ви не призначені завідувачем жодної кафедри.");
                                break;
                            }

                            allDepartments = allDepartments
                                .Where(d => d.Id == headDepartment27.Id)
                                .ToList();
                        }

                        var allTeachers27 = university.GetAllTeachers();

                        Console.WriteLine("\nСПИСОК КАФЕДР:");

                        foreach (var currentDepartment in allDepartments)
                        {
                            Console.WriteLine($"\nКафедра: {currentDepartment.Name}");
                            Console.WriteLine("Викладачі:");

                            if (currentDepartment.TeacherIds.Count == 0)
                                Console.WriteLine("Викладачів поки немає.");

                            foreach (var teacherId in currentDepartment.TeacherIds)
                            {
                                var teacher = allTeachers27
                                    .FirstOrDefault(t => t.Id == teacherId);

                                if (teacher != null)
                                    Console.WriteLine(
                                        $"- {teacher.FullName} ({teacher.Position})");
                            }

                            var head = allTeachers27.FirstOrDefault(t =>
                                currentDepartment.HeadTeacherId.HasValue &&
                                t.Id == currentDepartment.HeadTeacherId.Value);

                            Console.WriteLine(
                                $"Завідувач: {head?.FullName ?? "Не призначений"}");
                        }

                        break;
                    }

                case "28":
                    {
                        var departmentTeachers = university.GetAllTeachers();
                        var departmentsForAssignment = university.GetAllDepartments();

                        if (departmentTeachers.Count == 0 ||
                            departmentsForAssignment.Count == 0)
                        {
                            Console.WriteLine("Немає викладачiв або кафедр!");
                            break;
                        }

                        Console.WriteLine("\nСПИСОК ВИКЛАДАЧIВ:");

                        for (int i = 0; i < departmentTeachers.Count; i++)
                            Console.WriteLine($"{i + 1}. {departmentTeachers[i].FullName}");

                        Console.Write("Оберiть викладача: ");

                        if (!int.TryParse(ReadText(), out int selectedTeacherNumber) ||
                            selectedTeacherNumber < 1 ||
                            selectedTeacherNumber > departmentTeachers.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        Console.WriteLine("\nСПИСОК КАФЕДР:");

                        for (int i = 0; i < departmentsForAssignment.Count; i++)
                            Console.WriteLine($"{i + 1}. {departmentsForAssignment[i].Name}");

                        Console.Write("Оберiть кафедру: ");

                        if (!int.TryParse(ReadText(), out int selectedDepartmentNumber) ||
                            selectedDepartmentNumber < 1 ||
                            selectedDepartmentNumber > departmentsForAssignment.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        university.AssignTeacherToDepartment(
                            departmentTeachers[selectedTeacherNumber - 1].Id,
                            departmentsForAssignment[selectedDepartmentNumber - 1].Id
                        );

                        Console.WriteLine("Викладача успiшно призначено на кафедру!");
                        break;
                    }

                case "29":
                    {
                        var departmentsForHead = university.GetAllDepartments();

                        if (departmentsForHead.Count == 0)
                        {
                            Console.WriteLine("Кафедр поки немає!");
                            break;
                        }

                        Console.WriteLine("\nСПИСОК КАФЕДР:");

                        for (int i = 0; i < departmentsForHead.Count; i++)
                            Console.WriteLine($"{i + 1}. {departmentsForHead[i].Name}");

                        Console.Write("Оберiть кафедру: ");

                        if (!int.TryParse(ReadText(), out int headDepartmentChoice) ||
                            headDepartmentChoice < 1 ||
                            headDepartmentChoice > departmentsForHead.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        var selectedHeadDepartment =
                            departmentsForHead[headDepartmentChoice - 1];

                        var departmentMembers = university.GetAllTeachers()
                            .Where(t => selectedHeadDepartment.TeacherIds.Contains(t.Id))
                            .ToList();

                        if (departmentMembers.Count == 0)
                        {
                            Console.WriteLine("На кафедрi немає викладачiв!");
                            break;
                        }

                        Console.WriteLine("\nВИКЛАДАЧI КАФЕДРИ:");

                        for (int i = 0; i < departmentMembers.Count; i++)
                            Console.WriteLine($"{i + 1}. {departmentMembers[i].FullName}");

                        Console.Write("Оберiть завiдувача: ");

                        if (!int.TryParse(ReadText(), out int headTeacherChoice) ||
                            headTeacherChoice < 1 ||
                            headTeacherChoice > departmentMembers.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        university.SetDepartmentHead(
                            selectedHeadDepartment.Id,
                            departmentMembers[headTeacherChoice - 1].Id
                        );

                        Console.WriteLine("Завiдувача кафедри успiшно призначено!");
                        break;
                    }

                case "30":
                    {
                        var studentsForLink = university.GetAllStudents();

                        if (studentsForLink.Count == 0)
                        {
                            Console.WriteLine("Студентів поки немає!");
                            break;
                        }

                        Console.WriteLine("\nСПИСОК СТУДЕНТІВ:");

                        for (int i = 0; i < studentsForLink.Count; i++)
                        {
                            string accountStatus = string.IsNullOrWhiteSpace(
                                studentsForLink[i].AccountUsername)
                                ? "Не прив'язаний"
                                : studentsForLink[i].AccountUsername;

                            Console.WriteLine(
                                $"{i + 1}. {studentsForLink[i].FullName} | " +
                                $"Група: {studentsForLink[i].Group} | " +
                                $"Акаунт: {accountStatus}");
                        }

                        Console.Write("\nОберіть студента: ");

                        if (!int.TryParse(ReadText(), out int linkStudentChoice) ||
                            linkStudentChoice < 1 ||
                            linkStudentChoice > studentsForLink.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        var selectedStudentForLink =
                            studentsForLink[linkStudentChoice - 1];

                        Console.Write("Введіть логін облікового запису: ");
                        string accountUsername = ReadText();

                        university.LinkStudentAccount(
                            selectedStudentForLink.Id,
                            accountUsername);

                        Console.WriteLine(
                            $"Обліковий запис успішно прив'язано до студента {selectedStudentForLink.FullName}!");

                        break;
                    }

                case "31":
                    {
                        var teachersForLink = university.GetAllTeachers();

                        if (teachersForLink.Count == 0)
                        {
                            Console.WriteLine("Викладачів поки немає!");
                            break;
                        }

                        Console.WriteLine("\nСПИСОК ВИКЛАДАЧІВ:");

                        for (int i = 0; i < teachersForLink.Count; i++)
                        {
                            string accountStatus = string.IsNullOrWhiteSpace(
                                teachersForLink[i].AccountUsername)
                                ? "Не прив'язаний"
                                : teachersForLink[i].AccountUsername;

                            Console.WriteLine(
                                $"{i + 1}. {teachersForLink[i].FullName} | " +
                                $"Кафедра: {teachersForLink[i].Department} | " +
                                $"Акаунт: {accountStatus}");
                        }

                        Console.Write("\nОберіть викладача: ");

                        if (!int.TryParse(ReadText(), out int linkTeacherChoice31) ||
                            linkTeacherChoice31 < 1 ||
                            linkTeacherChoice31 > teachersForLink.Count)
                        {
                            Console.WriteLine("Некоректний номер!");
                            break;
                        }

                        var selectedTeacherForAccountLink = teachersForLink[linkTeacherChoice31 - 1];

                        Console.Write("Введіть логін облікового запису: ");
                        string accountUsername = ReadText();

                        university.LinkTeacherAccount(
                            selectedTeacherForAccountLink.Id,
                            accountUsername);

                        Console.WriteLine(
                            $"Обліковий запис успішно прив'язано до викладача {selectedTeacherForAccountLink.FullName}!");

                        break;
                    }
                case "99":
                    SaveData();
                    Console.WriteLine("Дані успішно збережено!");
                    Console.WriteLine("Роботу програми завершено. До побачення!");
                    isRunning = false;
                    break;

                case "0":
                    {
                        Console.Clear();
                        Console.WriteLine("======================================");
                        Console.WriteLine("        ВИХІД З ОБЛІКОВОГО ЗАПИСУ");
                        Console.WriteLine("======================================");

                        // Збереження даних перед виходом
                        SaveData();
                        Console.WriteLine("Дані успішно збережено!");

                        // Повторна авторизація
                        while (true)
                        {
                            Console.WriteLine("\nАВТОРИЗАЦІЯ КОРИСТУВАЧА");

                            Console.Write("Логін: ");
                            string username = ReadText();

                            Console.Write("Пароль: ");
                            string password = ReadText();

                            try
                            {
                                LoginResponse loginResult =
                                    await authService.LoginAsync(username, password);

                                userRole = loginResult.Role;
                                currentUsername = username;

                                // Оновлення поточного користувача
                                loggedInStudent = null;
                                loggedInTeacher = null;

                                if (userRole == "Student")
                                {
                                    loggedInStudent =
                                        university.GetStudentByUsername(currentUsername);
                                }
                                else if (userRole == "Teacher" ||
                                         userRole == "DepartmentHead")
                                {
                                    loggedInTeacher =
                                        university.GetTeacherByUsername(currentUsername);
                                }

                                Console.WriteLine("\nАвторизація успішна!");
                                Console.WriteLine($"Роль користувача: {userRole}");

                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"\nПомилка авторизації: {ex.Message}");
                                Console.WriteLine("Спробуйте ще раз.");
                            }
                        }

                        break;
                    }


                default:
                    Console.WriteLine("Невiрний пункт меню!");
                    break;
            }

            // Автоматичне збереження в SQLite
            SaveData();

            if (choice == "0")
                Console.WriteLine("Данi успiшно збережено!");

        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }

    if (isRunning)
    {
        Console.WriteLine("\nНатиснiть будь-яку клавiшу для продовження...");
        Console.ReadKey(true);
    }
}
