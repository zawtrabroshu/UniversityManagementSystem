using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace University.Domain.Entities
{
    public class Student
    {
        [JsonInclude]
        public Guid Id { get; private set; }
        [JsonInclude]
        public string FullName { get; private set; }
        [JsonInclude]
        public string Group { get; private set; }
        [JsonInclude]
        public int AdmissionYear { get; private set; }
        [JsonInclude]
        public List<Guid> SubjectIds { get; private set; }
        [JsonInclude]
        public string? AccountUsername { get; private set; }

        public Student(string fullName, string group, int admissionYear)
        {
            Id = Guid.NewGuid();
            SubjectIds = new List<Guid>();

            UpdateInfo(fullName, group, admissionYear);
        }

        // Оновлення особистих даних студента
        public void UpdateInfo(string fullName, string group, int admissionYear)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("ПIБ студента не може бути порожнім.");

            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Назва групи не може бути порожньою.");

            if (admissionYear < 2000 || admissionYear > DateTime.Now.Year)
                throw new ArgumentException("Некоректний рік вступу.");

            FullName = fullName;
            Group = group;
            AdmissionYear = admissionYear;
        }

        // Призначення навчальної дисципліни студенту
        public void AssignSubject(Guid subjectId)
        {
            if (subjectId == Guid.Empty)
                throw new ArgumentException("Некоректний ідентифікатор дисципліни.");

            if (!SubjectIds.Contains(subjectId))
                SubjectIds.Add(subjectId);
        }

        // Видалення навчальної дисципліни у студента
        public void RemoveSubject(Guid subjectId)
        {
            SubjectIds.Remove(subjectId);
        }
        // Прив'язування облікового запису до студента
        public void LinkAccount(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");

            AccountUsername = username;
        }
    }

}
