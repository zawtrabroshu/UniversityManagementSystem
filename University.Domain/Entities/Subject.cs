using System.Text.Json.Serialization;
using System;

namespace University.Domain.Entities
{
    public class Subject
    {
        [JsonInclude]
        public Guid Id { get; private set; }
        [JsonInclude]
        public string Name { get; private set; }
        [JsonInclude]
        public int Semester { get; private set; }
        [JsonInclude]
        public int Credits { get; private set; }

        public Subject(string name, int semester, int credits)
        {
            Id = Guid.NewGuid();

            UpdateInfo(name, semester, credits);
        }

        // Оновлення інформацii про навчальну дисциплiну
        public void UpdateInfo(string name, int semester, int credits)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва дисциплiни не може бути порожньою.");

            if (semester < 1 || semester > 2)
                throw new ArgumentException("Семестр повинен бути вiд 1 до 2.");

            if (credits <= 0)
                throw new ArgumentException("Кiлькiсть кредитiв повинна бути бiльшою за нуль.");

            Name = name;
            Semester = semester;
            Credits = credits;
        }
    }
}
