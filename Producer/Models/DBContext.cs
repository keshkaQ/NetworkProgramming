using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace Producer.Models
{
    public class DBContext : DbContext
    {
        public DbSet<Iphone> Iphones { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=iphoneStore.db");
        }
        public void InitializeData()
        {
            try
            {
                Database.EnsureCreated();
                if (!Iphones.Any())
                {
                    var iphonesList = new List<List<Iphone>>
                    {
                    CreateIphone("iPhone 16 Blue", "iPhone 16", "Blue", 79990, 89990, 109990, "★ 4,9", " - 324 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A18 Bionic\n• Камера: 48 Мп + 12 Мп\n• Аккумулятор: до 22 часов\n• Кнопка действий\n• Цвет: Голубой", "/Images/iphone16_blue.png"),
                    CreateIphone("iPhone 16 Pink", "iPhone 16", "Pink", 79990, 89990, 109990, "★ 4,8", " - 287 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A18 Bionic\n• Камера: 48 Мп + 12 Мп\n• Аккумулятор: до 22 часов\n• Кнопка действий\n• Цвет: Розовый", "/Images/iphone16_pink.png"),
                    CreateIphone("iPhone 16 Green", "iPhone 16", "Green", 79990, 89990, 109990, "★ 4,8", " - 256 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A18 Bionic\n• Камера: 48 Мп + 12 Мп\n• Аккумулятор: до 22 часов\n• Кнопка действий\n• Цвет: Зеленый", "/Images/iphone16_green.png"),
                    CreateIphone("iPhone 16 Black", "iPhone 16", "Black", 79990, 89990, 109990, "★ 4,9", " - 412 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A18 Bionic\n• Камера: 48 Мп + 12 Мп\n• Аккумулятор: до 22 часов\n• Кнопка действий\n• Цвет: Черный", "/Images/iphone16_black.png"),
                    CreateIphone("iPhone 16 White", "iPhone 16", "White", 79990, 89990, 109990, "★ 4,7", " - 198 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A18 Bionic\n• Камера: 48 Мп + 12 Мп\n• Аккумулятор: до 22 часов\n• Кнопка действий\n• Цвет: Белый", "/Images/iphone16_white.png"),
                    CreateIphone("iPhone 16 Pro Natural Titanium", "iPhone 16 Pro", "Natural Titanium", 129990, 144990, 164990, "★ 4,8", " - 156 оценок", "• Диагональ экрана: 6.3\"\n• Процессор: A18 Pro\n• Камера: 48 Мп + 12 Мп + 12 Мп\n• Аккумулятор: до 25 часов\n• Кнопка действий\n• Кнопка съемки\n• Цвет: Натуральный титан", "/Images/iphone16pro_natural.png"),
                    CreateIphone("iPhone 16 Pro Black Titanium", "iPhone 16 Pro", "Black Titanium", 129990, 144990, 164990, "★ 4,9", " - 245 оценок", "• Диагональ экрана: 6.3\"\n• Процессор: A18 Pro\n• Камера: 48 Мп + 12 Мп + 12 Мп\n• Аккумулятор: до 25 часов\n• Кнопка действий\n• Кнопка съемки\n• Цвет: Черный титан", "/Images/iphone16pro_black.png"),
                    CreateIphone("iPhone 16 Pro White Titanium", "iPhone 16 Pro", "White Titanium", 129990, 144990, 164990, "★ 4,8", " - 134 оценок", "• Диагональ экрана: 6.3\"\n• Процессор: A18 Pro\n• Камера: 48 Мп + 12 Мп + 12 Мп\n• Аккумулятор: до 25 часов\n• Кнопка действий\n• Кнопка съемки\n• Цвет: Белый титан", "/Images/iphone16pro_white.png"),
                    CreateIphone("iPhone 15 Blue", "iPhone 15", "Blue", 69990, 79990, 99990, "★ 4,9", " - 249 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A16 Bionic\n• Камера: 48 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Голубой", "/Images/iphone15_blue.png"),
                    CreateIphone("iPhone 15 Yellow", "iPhone 15", "Yellow", 69990, 79990, 99990, "★ 4,8", " - 187 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A16 Bionic\n• Камера: 48 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Желтый", "/Images/iphone15_yellow.png"),
                    CreateIphone("iPhone 15 Green", "iPhone 15", "Green", 69990, 79990, 99990, "★ 4,7", " - 156 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A16 Bionic\n• Камера: 48 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Зеленый", "/Images/iphone15_green.png"),
                    CreateIphone("iPhone 15 Pink", "iPhone 15", "Pink", 69990, 79990, 99990, "★ 4,6", " - 134 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A16 Bionic\n• Камера: 48 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Розовый", "/Images/iphone15_pink.png"),
                    CreateIphone("iPhone 15 Black", "iPhone 15", "Black", 69990, 79990, 99990, "★ 4,9", " - 298 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A16 Bionic\n• Камера: 48 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Черный", "/Images/iphone15_black.png"),
                    CreateIphone("iPhone 15 Pro Blue Titanium", "iPhone 15 Pro", "Blue Titanium", 99990, 114990, 134990, "★ 4,8", " - 187 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A17 Pro\n• Камера: 48 Мп + 12 Мп + 12 Мп\n• Аккумулятор: до 23 часов\n• Цвет: Голубой титан", "/Images/iphone15pro_blue.png"),
                    CreateIphone("iPhone 15 Pro White Titanium", "iPhone 15 Pro", "White Titanium", 99990, 114990, 134990, "★ 4,7", " - 156 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A17 Pro\n• Камера: 48 Мп + 12 Мп + 12 Мп\n• Аккумулятор: до 23 часов\n• Цвет: Белый титан", "/Images/iphone15pro_white.png"),
                    CreateIphone("iPhone 15 Pro Black Titanium", "iPhone 15 Pro", "Black Titanium", 99990, 114990, 134990, "★ 4,9", " - 245 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A17 Pro\n• Камера: 48 Мп + 12 Мп + 12 Мп\n• Аккумулятор: до 23 часов\n• Цвет: Черный титан", "/Images/iphone15pro_black.png"),
                    CreateIphone("iPhone 15 Plus Blue", "iPhone 15 Plus", "Blue", 79990, 89990, 109990, "★ 4,7", " - 156 оценок", "• Диагональ экрана: 6.7\"\n• Процессор: A16 Bionic\n• Камера: 48 Мп\n• Аккумулятор: до 26 часов\n• Цвет: Голубой", "/Images/iphone15plus_blue.png"),
                    CreateIphone("iPhone 15 Plus Green", "iPhone 15 Plus", "Green", 79990, 89990, 109990, "★ 4,7", " - 148 оценок", "• Диагональ экрана: 6.7\"\n• Процессор: A16 Bionic\n• Камера: 48 Мп\n• Аккумулятор: до 26 часов\n• Цвет: Зеленый", "/Images/iphone15plus_green.png"),
                    CreateIphone("iPhone 14 Blue", "iPhone 14", "Blue", 59990, 69990, 84990, "★ 4,6", " - 324 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Голубой", "/Images/iphone14_blue.png"),
                    CreateIphone("iPhone 14 Red", "iPhone 14", "Red", 59990, 69990, 84990, "★ 4,6", " - 298 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Красный", "/Images/iphone14_red.png"),
                    CreateIphone("iPhone 14 Purple", "iPhone 14", "Purple", 59990, 69990, 84990, "★ 4,4", " - 256 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Фиолетовый", "/Images/iphone14_purple.png"),
                    CreateIphone("iPhone 14 Black", "iPhone 14", "Black", 59990, 69990, 84990, "★ 4,7", " - 412 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Черный", "/Images/iphone14_black.png"),
                    CreateIphone("iPhone 14 Yellow", "iPhone 14", "Yellow", 59990, 69990, 84990, "★ 4,5", " - 287 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп\n• Аккумулятор: до 20 часов\n• Цвет: Желтый", "/Images/iphone14_yellow.png"),
                    CreateIphone("iPhone 13 White", "iPhone 13", "White", 49990, 59990, 74990, "★ 4,3", " - 398 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 19 часов\n• Цвет: Белый", "/Images/iphone13_white.png"),
                    CreateIphone("iPhone 13 Black", "iPhone 13", "Black", 49990, 59990, 74990, "★ 4,6", " - 678 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 19 часов\n• Цвет: Черный", "/Images/iphone13_black.png"),
                    CreateIphone("iPhone 13 Pink", "iPhone 13", "Pink", 49990, 59990, 74990, "★ 4,4", " - 345 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 19 часов\n• Цвет: Розовый", "/Images/iphone13_pink.png"),
                    CreateIphone("iPhone 13 Blue", "iPhone 13", "Blue", 49990, 59990, 74990, "★ 4,5", " - 423 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A15 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 19 часов\n• Цвет: Голубой", "/Images/iphone13_blue.png"),
                    CreateIphone("iPhone 12 Black", "iPhone 12", "Black", 39990, 49990, 64990, "★ 4,4", " - 623 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A14 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 17 часов\n• Цвет: Черный", "/Images/iphone12_black.png"),
                    CreateIphone("iPhone 12 White", "iPhone 12", "White", 39990, 49990, 64990, "★ 4,3", " - 487 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A14 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 17 часов\n• Цвет: Белый", "/Images/iphone12_white.png"),
                    CreateIphone("iPhone 12 Red", "iPhone 12", "Red", 39990, 49990, 64990, "★ 4,4", " - 398 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A14 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 17 часов\n• Цвет: Красный", "/Images/iphone12_red.png"),
                    CreateIphone("iPhone 12 Green", "iPhone 12", "Green", 39990, 49990, 64990, "★ 4,3", " - 356 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A14 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 17 часов\n• Цвет: Зеленый", "/Images/iphone12_green.png"),
                    CreateIphone("iPhone 12 Purple", "iPhone 12", "Purple", 39990, 49990, 64990, "★ 4,2", " - 312 оценок", "• Диагональ экрана: 6.1\"\n• Процессор: A14 Bionic\n• Камера: 12 Мп + 12 Мп\n• Аккумулятор: до 17 часов\n• Цвет: Фиолетовый", "/Images/iphone12_purple.png")
                    };
                    var allIphones = iphonesList.SelectMany(x => x).ToList();
                    Iphones.AddRange(allIphones);
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private List<Iphone> CreateIphone(string baseName, string baseModelName, string color, int price128, int price256, int price512, string rating, string reviews, string baseSpecs, string imageSource)
        {
            var oldPrice128 = price128 * 2;
            var oldPrice256 = price256 * 2;
            var oldPrice512 = price512 * 2;

            return new List<Iphone>
            {
                new Iphone
                {
                    Name = $"{baseName} 128GB",
                    BaseModelName = baseModelName,
                    Storage = "128 ГБ",
                    Price = $"{price128:N0} ₽",
                    OldPrice = $"{oldPrice128:N0} ₽",
                    Rating = rating,
                    Reviews = reviews,
                    Specifications = baseSpecs,
                    ImageSource = imageSource,
                    BasePrice = price128,
                    Color = color
                },
                new Iphone
                {
                    Name = $"{baseName} 256GB",
                    BaseModelName = baseModelName,
                    Storage = "256 ГБ",
                    Price = $"{price256:N0} ₽",
                    OldPrice = $"{oldPrice256:N0} ₽",
                    Rating = rating,
                    Reviews = reviews,
                    Specifications = baseSpecs,
                    ImageSource = imageSource,
                    BasePrice = price256,
                    Color = color
                },
                new Iphone
                {
                    Name = $"{baseName} 512GB",
                    BaseModelName = baseModelName,
                    Storage = "512 ГБ",
                    Price = $"{price512:N0} ₽",
                    OldPrice = $"{oldPrice512:N0} ₽",
                    Rating = rating,
                    Reviews = reviews,
                    Specifications = baseSpecs,
                    ImageSource = imageSource,
                    BasePrice = price512,
                    Color = color
                }
            };
        }
    }
}
