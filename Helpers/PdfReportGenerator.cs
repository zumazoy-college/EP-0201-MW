using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace EP_0201_MW.Helpers
{
    public class PdfReportGenerator
    {
        public static string GenerateReport(
            string reportTitle,
            string reportDate,
            string reportPeriod,
            List<string[]> tableData,
            List<string> columnHeaders,
            Dictionary<string, string> statistics,
            string responsiblePerson)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                string fileName = $"{reportTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                //string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
                //string reportsFolder = Path.Combine(projectFolder, "Reports");
                //string filePath = Path.Combine(reportsFolder, fileName);

                //// Создаем папку Reports, если её еще нет в проекте
                //if (!Directory.Exists(reportsFolder))
                //{
                //    Directory.CreateDirectory(reportsFolder);
                //}

                // 1. Получаем путь к папке bin/Debug/...
                string exePath = AppDomain.CurrentDomain.BaseDirectory;

                // 2. Поднимаемся на 3-4 уровня вверх до корня проекта
                DirectoryInfo projectDir = Directory.GetParent(exePath).Parent.Parent.Parent;

                string reportsFolder;

                // Если папка проекта найдена
                if (projectDir != null && File.Exists(Path.Combine(projectDir.FullName, "EP-0201-MW.csproj")))
                {
                    reportsFolder = Path.Combine(projectDir.FullName, "reports");
                }
                else
                {
                    // Если .csproj не найден (например, программа уже установлена у клиента) сохраняем просто рядом с .exe
                    reportsFolder = Path.Combine(exePath, "reports");
                }

                string filePath = Path.Combine(reportsFolder, fileName);

                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                }

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Шапка документа
                        page.Header().Column(col =>
                        {
                            col.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("ООО «МастерСклад»")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken4);

                                    c.Item().Text("Отчет")
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Medium);
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text(reportDate)
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Darken2);

                                    c.Item().Text("г. Москва")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Medium);
                                });
                            });

                            col.Item().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        });

                        // Основное содержимое
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // Заголовок отчета
                            col.Item().PaddingBottom(15).Text(reportTitle)
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Black);

                            // Метаданные
                            col.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Дата формирования:")
                                        .FontSize(10)
                                        .SemiBold()
                                        .FontColor(Colors.Grey.Darken2);

                                    c.Item().Text(reportDate)
                                        .FontSize(11)
                                        .FontColor(Colors.Black);
                                });

                                row.ConstantItem(30);

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Период отчета:")
                                        .FontSize(10)
                                        .SemiBold()
                                        .FontColor(Colors.Grey.Darken2);

                                    c.Item().Text(reportPeriod)
                                        .FontSize(11)
                                        .FontColor(Colors.Black);
                                });
                            });

                            // Статистика
                            if (statistics != null && statistics.Count > 0)
                            {
                                col.Item().PaddingVertical(10).Background(Colors.Blue.Darken4)
                                    .Padding(15).Column(statCol =>
                                    {
                                        statCol.Item().PaddingBottom(5).Text("Ключевые показатели")
                                            .FontSize(12)
                                            .Bold()
                                            .FontColor(Colors.White);

                                        statCol.Item().Row(row =>
                                        {
                                            int i = 0;
                                            foreach (var stat in statistics)
                                            {
                                                if (i > 0) row.ConstantItem(20);
                                                row.RelativeItem().Column(c =>
                                                {
                                                    c.Item().Text(stat.Key)
                                                        .FontSize(9)
                                                        .FontColor(Colors.White);

                                                    c.Item().Text(stat.Value)
                                                        .FontSize(14)
                                                        .Bold()
                                                        .FontColor(Colors.White);
                                                });
                                                i++;
                                            }
                                        });
                                    });
                            }

                            // Таблица данных
                            if (tableData != null && tableData.Count > 0)
                            {
                                col.Item().PaddingVertical(15).Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
                                {
                                    // Заголовки столбцов
                                    table.ColumnsDefinition(columns =>
                                    {
                                        for (int i = 0; i < columnHeaders.Count; i++)
                                        {
                                            columns.RelativeColumn();
                                        }
                                    });

                                    // Заголовок таблицы
                                    table.Header(header =>
                                    {
                                        foreach (var headerText in columnHeaders)
                                        {
                                            header.Cell().Background(Colors.Grey.Lighten3)
                                                .Padding(5).Text(headerText)
                                                .FontSize(9)
                                                .SemiBold()
                                                .FontColor(Colors.Black);
                                        }
                                    });

                                    // Данные таблицы
                                    foreach (var rowData in tableData)
                                    {
                                        foreach (var cellData in rowData)
                                        {
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                                .Padding(5).Text(cellData)
                                                .FontSize(8)
                                                .FontColor(Colors.Grey.Darken3);
                                        }
                                    }
                                });
                            }
                            else
                            {
                                col.Item().Padding(20).AlignCenter().Text("Нет данных для отображения")
                                    .FontSize(12)
                                    .Italic()
                                    .FontColor(Colors.Grey.Medium);
                            }

                            // Подпись
                            col.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingTop(15).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Ответственный:")
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Darken1);

                                        c.Item().Text(responsiblePerson)
                                            .FontSize(10)
                                            .SemiBold()
                                            .FontColor(Colors.Black);
                                    });

                                    row.RelativeItem().AlignRight().Column(c =>
                                    {
                                        c.Item().PaddingBottom(20).Text("М.П.")
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Darken1);

                                        c.Item().BorderTop(1).BorderColor(Colors.Grey.Darken1)
                                            .Width(150).PaddingTop(5);

                                        c.Item().AlignCenter().Text("Подпись")
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Darken1);
                                    });
                                });
                        });

                        // Подвал
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                            text.Span("Страница ");
                            text.CurrentPageNumber();
                            text.Span(" из ");
                            text.TotalPages();
                            text.Span($" | Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        });
                    });
                }).GeneratePdf(filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации PDF: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}