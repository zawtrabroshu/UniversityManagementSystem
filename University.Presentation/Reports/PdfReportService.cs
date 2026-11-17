using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using University.Domain.Entities;

namespace University.Presentation.Reports
{
    public class PdfReportService
    {
        public void GenerateStudentReport(
            Student student,
            List<Grade> grades,
            List<Subject> subjects,
            string filePath)
        {
            double average = grades.Count > 0
                ? grades.Average(g => g.NumericGrade)
                : 0;

            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(11));

                    page.Header()
                        .AlignCenter()
                        .Text("АКАДЕМІЧНА ВИПИСКА")
                        .FontSize(20)
                        .Bold();

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text($"ПІБ студента: {student.FullName}");
                        column.Item().Text($"Група: {student.Group}");
                        column.Item().Text($"Рік вступу: {student.AdmissionYear}");

                        column.Item().PaddingTop(10).Text("ОЦІНКИ З ДИСЦИПЛІН")
                            .Bold()
                            .FontSize(14);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Дисципліна");
                                header.Cell().Element(HeaderStyle).Text("Бал");
                                header.Cell().Element(HeaderStyle).Text("Оцінка");
                                header.Cell().Element(HeaderStyle).Text("Дата");
                            });

                            foreach (var grade in grades)
                            {
                                var subject = subjects.FirstOrDefault(
                                    s => s.Id == grade.SubjectId);

                                table.Cell().Element(CellStyle)
                                    .Text(subject?.Name ?? "Невідома дисципліна");

                                table.Cell().Element(CellStyle)
                                    .Text(grade.NumericGrade.ToString());

                                table.Cell().Element(CellStyle)
                                    .Text(grade.LetterGrade.ToString());

                                table.Cell().Element(CellStyle)
                                    .Text(grade.Date.ToString("dd.MM.yyyy"));
                            }
                        });

                        column.Item().PaddingTop(15)
                            .Text($"Середній бал: {average:F2}")
                            .Bold()
                            .FontSize(14);
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Сторінка ");
                        text.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(filePath);
        }

        private static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Padding(5);
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }
}
