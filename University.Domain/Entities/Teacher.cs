using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace University.Domain.Entities
{
    public class Teacher
    {
        [JsonInclude]
        public Guid Id { get; private set; }

        [JsonInclude]
        public string FullName { get; private set; } = string.Empty;

        [JsonInclude]
        public string Department { get; private set; } = string.Empty;

        [JsonInclude]
        public TeacherPosition Position { get; private set; }

        [JsonInclude]
        public List<Guid> SubjectIds { get; private set; }
        [JsonInclude]
        public string? AccountUsername { get; private set; }
        // Прив'язування облікового запису до викладача
        public void LinkAccount(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");

            AccountUsername = username;
        }
        public Teacher(string fullName, string department,
                       TeacherPosition position = TeacherPosition.Assistant)
        {
            Id = Guid.NewGuid();
            SubjectIds = new List<Guid>();

            UpdateInfo(fullName, department);
            UpdatePosition(position);
        }

        // Оновлення інформації про викладача
        public void UpdateInfo(string fullName, string department)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("ПІБ викладача не може бути порожнім.");

            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Назва кафедри не може бути порожньою.");

            FullName = fullName;
            Department = department;
        }

        // Оновлення посади викладача
        public void UpdatePosition(TeacherPosition position)
        {
            if (!Enum.IsDefined(typeof(TeacherPosition), position))
                throw new ArgumentException("Некоректна посада викладача.");

            Position = position;
        }

        // Призначення дисципліни викладачу
        public void AssignSubject(Guid subjectId)
        {
            if (subjectId == Guid.Empty)
                throw new ArgumentException("Некоректний ідентифікатор дисципліни.");

            if (!SubjectIds.Contains(subjectId))
                SubjectIds.Add(subjectId);
        }

        // Видалення дисципліни у викладача
        public void RemoveSubject(Guid subjectId)
        {
            SubjectIds.Remove(subjectId);
        }
    }
}
