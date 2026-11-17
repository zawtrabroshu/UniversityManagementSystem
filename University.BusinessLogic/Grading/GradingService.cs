namespace University.BusinessLogic.Grading
{
    public class GradingService
    {
        // Перетворення числової оцiнки у лiтерну
        public string ConvertToLetterGrade(double grade)
        {
            if (grade < 0 || grade > 100)
                throw new ArgumentException("Оцiнка повинна бути вiд 0 до 100.");

            if (grade >= 90) return "A";
            if (grade >= 82) return "B";
            if (grade >= 74) return "C";
            if (grade >= 64) return "D";
            if (grade >= 60) return "E";
            if (grade >= 35) return "Fx";

            return "F";
        }
    }
}
