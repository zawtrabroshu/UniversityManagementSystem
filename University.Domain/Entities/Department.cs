using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace University.Domain.Entities
{
    public class Department
    {
        [JsonInclude]
        public Guid Id { get; private set; }

        [JsonInclude]
        public string Name { get; private set; } = string.Empty;

        [JsonInclude]
        public Guid? HeadTeacherId { get; private set; }

        [JsonInclude]
        public List<Guid> TeacherIds { get; private set; }

        public Department(string name)
        {
            Id = Guid.NewGuid();
            TeacherIds = new List<Guid>();

            UpdateInfo(name);
        }

        // Оновлення інформації про кафедру
        public void UpdateInfo(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва кафедри не може бути порожньою.");

            Name = name;
        }

        // Додавання викладача до кафедри
        public void AssignTeacher(Guid teacherId)
        {
            if (teacherId == Guid.Empty)
                throw new ArgumentException("Некоректний ідентифікатор викладача.");

            if (!TeacherIds.Contains(teacherId))
                TeacherIds.Add(teacherId);
        }

        // Видалення викладача з кафедри
        public void RemoveTeacher(Guid teacherId)
        {
            TeacherIds.Remove(teacherId);

            if (HeadTeacherId == teacherId)
                HeadTeacherId = null;
        }

        // Призначення завідувача кафедри
        public void SetHeadTeacher(Guid teacherId)
        {
            if (teacherId == Guid.Empty)
                throw new ArgumentException("Некоректний ідентифікатор викладача.");

            if (!TeacherIds.Contains(teacherId))
                throw new ArgumentException("Викладач не належить до цієї кафедри.");

            HeadTeacherId = teacherId;
        }
    }
}
