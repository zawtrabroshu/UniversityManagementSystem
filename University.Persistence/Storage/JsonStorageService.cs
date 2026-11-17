using System.IO;
using System.Text.Json;

namespace University.Persistence.Storage
{
    public class JsonStorageService
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        // Збереження даних у JSON-файл
        public void SaveData(UniversityData data, string filePath)
        {
            string json = JsonSerializer.Serialize(data, options);

            File.WriteAllText(filePath, json);
        }

        // Завантаження даних з JSON-файлу
        public UniversityData LoadData(string filePath)
        {
            if (!File.Exists(filePath))
                return new UniversityData();

            string json = File.ReadAllText(filePath);

            UniversityData data =
                JsonSerializer.Deserialize<UniversityData>(json, options);

            return data ?? new UniversityData();
        }
    }
}
