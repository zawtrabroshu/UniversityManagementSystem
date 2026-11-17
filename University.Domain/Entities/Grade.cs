using System.Text.Json.Serialization;
using System;

namespace University.Domain.Entities
{
    public class Grade
    {
        [JsonInclude]
        public Guid Id { get; private set; }
        [JsonInclude]
        public Guid StudentId { get; private set; }
        [JsonInclude]
        public Guid SubjectId { get; private set; }
        [JsonInclude]
        public Guid TeacherId { get; private set; }
        [JsonInclude]
        public double NumericGrade { get; private set; }
        [JsonInclude]
        public string LetterGrade { get; private set; }
        [JsonInclude]
        public DateTime Date { get; private set; }

        public Grade(Guid studentId, Guid subjectId, Guid teacherId,
                     double numericGrade, string letterGrade)
        {
            Id = Guid.NewGuid();

            if (studentId == Guid.Empty)
                throw new ArgumentException("Некоректний iдентифiкатор студента.");

            if (subjectId == Guid.Empty)
                throw new ArgumentException("Некоректний iдентифiкатор дисциплiни.");

            if (teacherId == Guid.Empty)
                throw new ArgumentException("Некоректний iдентифiкатор викладача.");

            StudentId = studentId;
            SubjectId = subjectId;
            TeacherId = teacherId;

            UpdateGrade(numericGrade, letterGrade);

            Date = DateTime.Now;
        }

        // Оновлення оцiнки студента
        public void UpdateGrade(double numericGrade, string letterGrade)
        {
            if (numericGrade < 0 || numericGrade > 100)
                throw new ArgumentException("Оцiнка повинна бути вiд 0 до 100.");

            if (string.IsNullOrWhiteSpace(letterGrade))
                throw new ArgumentException("Лiтерна оцiнка не може бути порожньою.");

            NumericGrade = numericGrade;
            LetterGrade = letterGrade;
        }
    }
}
