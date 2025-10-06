using Microsoft.Data.Sqlite;
using ServerApp.Models;

namespace ServerApp.Database
{
    public class RecipeDatabase
    {
        private string _connectionString;

        public RecipeDatabase()
        {
            _connectionString = "Data Source=recipes.db";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Recipes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Instructions TEXT,
                    ImagePath TEXT
                );

                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE
                );

                CREATE TABLE IF NOT EXISTS RecipeProducts (
                    RecipeId INTEGER,
                    ProductId INTEGER,
                    PRIMARY KEY (RecipeId, ProductId),
                    FOREIGN KEY (RecipeId) REFERENCES Recipes(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                );
            ";
            command.ExecuteNonQuery();

            // Добавляем тестовые данные, если база пустая
            SeedTestData();
        }

        private void SeedTestData()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Проверяем, есть ли уже данные
            var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM Recipes", connection);
            var count = (long)checkCommand.ExecuteScalar();

            if (count == 0)
            {
                var products = new[]
                {
                    "Курица", "Сыр", "Колбаса", "Макароны", "Салат", "Гречка",
                    "Фарш", "Лук", "Морковь", "Картофель", "Помидоры", "Огурцы",
                    "Яйца", "Молоко", "Мука", "Сахар", "Соль", "Перец", "Масло",
                    "Рис"
                };

                foreach (var product in products)
                {
                    var insertProductCommand = new SqliteCommand("INSERT OR IGNORE INTO Products (Name) VALUES ($name)", connection);
                    insertProductCommand.Parameters.AddWithValue("$name", product);
                    insertProductCommand.ExecuteNonQuery();
                }
                AddSampleRecipes(connection);
            }
        }

        private void AddSampleRecipes(SqliteConnection connection)
        {
            var recipe1Id = InsertRecipe(connection,
                "Курица с картофелем",
                "Сытное блюдо из курицы и картофеля",
                "1. Нарежьте курицу и картофель кубиками\n2. Обжарьте лук и морковь\n3. Добавьте курицу и обжарьте до золотистой корочки\n4. Добавьте картофель и тушите 30 минут\n5. Подавайте горячим",
                "chicken_potato.jpg");

            LinkRecipeToProducts(connection, recipe1Id, new[] { "Курица", "Картофель", "Лук", "Морковь", "Масло", "Соль", "Перец" });

            var recipe2Id = InsertRecipe(connection,
                "Свежий салат",
                "Легкий и полезный салат",
                "1. Нарежьте помидоры и огурцы\n2. Добавьте соль и перец по вкусу\n3. Заправьте маслом\n4. Подавайте охлажденным",
                "salat.jpg");

            LinkRecipeToProducts(connection, recipe2Id, new[] { "Помидоры", "Огурцы", "Соль", "Перец", "Масло" });

            var recipe3Id = InsertRecipe(connection,
                "Макароны с сыром",
                "Простое и вкусное блюдо",
                "1. Отварите макароны согласно инструкции\n2. Натрите сыр на терке\n3. Смешайте макароны с сыром\n4. Подавайте горячими",
                "macarons.jpg");

            LinkRecipeToProducts(connection, recipe3Id, new[] { "Макароны", "Сыр", "Соль" });

            var recipe4Id = InsertRecipe(connection,
                "Гречка с фаршем",
                "Питательное блюдо для всей семьи",
                "1. Отварите гречку\n2. Обжарьте фарш с луком и морковью\n3. Смешайте с гречкой\n4. Тушите 10 минут",
                "buckwheat1.jpg");

            LinkRecipeToProducts(connection, recipe4Id, new[] { "Гречка", "Фарш", "Лук", "Морковь", "Масло", "Соль", "Перец" });

            var recipe5Id = InsertRecipe(connection,
                "Пышный омлет",
                "Нежный и воздушный омлет",
                "1. Взбейте яйца с молоком и солью\n2. Разогрейте сковороду с маслом\n3. Вылейте яичную смесь\n4. Жарьте на среднем огне 5-7 минут\n5. Подавайте сразу же",
                "omelet.jpg");

            LinkRecipeToProducts(connection, recipe5Id, new[] { "Яйца", "Молоко", "Соль", "Масло" });

            var recipe6Id = InsertRecipe(connection,
                "Жареная картошка с грибами",
                "Ароматная картошка с грибами и луком",
                "1. Нарежьте картофель соломкой\n2. Обжарьте лук до золотистого цвета\n3. Добавьте грибы и обжарьте 5 минут\n4. Добавьте картофель и жарьте до готовности\n5. Посолите и поперчите по вкусу",
                "fried_potato.jpg");

            LinkRecipeToProducts(connection, recipe6Id, new[] { "Картофель", "Лук", "Масло", "Соль", "Перец" });

            var recipe7Id = InsertRecipe(connection,
                "Сырный суп с курицей",
                "Нежный сливочный суп с сыром",
                "1. Отварите курицу до готовности\n2. Нарежьте картофель и морковь\n3. Обжарьте лук\n4. Добавьте овощи в бульон и варите 15 минут\n5. Добавьте натертый сыр и перемешайте до растворения",
                "cheese_soup.jpg");

            LinkRecipeToProducts(connection, recipe7Id, new[] { "Курица", "Сыр", "Картофель", "Морковь", "Лук", "Соль" });

            var recipe8Id = InsertRecipe(connection,
                "Овощное рагу",
                "Полезное рагу из сезонных овощей",
                "1. Нарежьте все овощи кубиками\n2. Обжарьте лук и морковь\n3. Добавьте картофель и тушите 10 минут\n4. Добавьте помидоры и кабачки\n5. Тушите до готовности овощей",
                "vegetable_stew.jpg");

            LinkRecipeToProducts(connection, recipe8Id, new[] { "Картофель", "Морковь", "Лук", "Помидоры", "Масло", "Соль", "Перец" });

            var recipe9Id = InsertRecipe(connection,
                "Тонкие блины",
                "Нежные тонкие блины на молоке",
                "1. Смешайте яйца, молоко и муку\n2. Добавьте соль и сахар\n3. Дайте тесту постоять 20 минут\n4. Жарьте на разогретой сковороде с двух сторон\n5. Подавайте с вареньем или сметаной",
                "pancakes.jpg");

            LinkRecipeToProducts(connection, recipe9Id, new[] { "Мука", "Яйца", "Молоко", "Сахар", "Соль", "Масло" });

            var recipe10Id = InsertRecipe(connection,
                "Домашний плов",
                "Ароматный плов с бараниной",
                "1. Обжарьте мясо до румяной корочки\n2. Добавьте лук и морковь\n3. Засыпьте рис и залейте водой\n4. Тушите на медленном огне 40 минут\n5. Дайте настояться под крышкой 15 минут",
                "plov.jpg");

            LinkRecipeToProducts(connection, recipe10Id, new[] { "Фарш", "Рис", "Лук", "Морковь", "Масло", "Соль", "Перец" });
        }

        private long InsertRecipe(SqliteConnection connection, string name, string description, string instructions, string imagePath = null)
        {
            var command = new SqliteCommand(@"
                INSERT INTO Recipes (Name, Description, Instructions, ImagePath)
                VALUES ($name, $description, $instructions, $imagePath);
                SELECT last_insert_rowid();", connection);

            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$description", description);
            command.Parameters.AddWithValue("$instructions", instructions);
            command.Parameters.AddWithValue("$imagePath", imagePath ?? (object)DBNull.Value);

            return (long)command.ExecuteScalar();
        }

        private void LinkRecipeToProducts(SqliteConnection connection, long recipeId, string[] productNames)
        {
            foreach (var productName in productNames)
            {
                var getProductCommand = new SqliteCommand("SELECT Id FROM Products WHERE Name = $name", connection);
                getProductCommand.Parameters.AddWithValue("$name", productName);
                var productId = getProductCommand.ExecuteScalar();

                if (productId != null)
                {
                    var linkCommand = new SqliteCommand("INSERT OR IGNORE INTO RecipeProducts (RecipeId, ProductId) VALUES ($recipeId, $productId)", connection);
                    linkCommand.Parameters.AddWithValue("$recipeId", recipeId);
                    linkCommand.Parameters.AddWithValue("$productId", productId);
                    linkCommand.ExecuteNonQuery();
                }
            }
        }

        public List<Recipe> FindRecipesByProducts(List<string> requestedProducts)
        {
            var recipes = new List<Recipe>();

            if (requestedProducts.Count == 0)
                return recipes;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var parameters = new List<SqliteParameter>();
            var placeholders = new List<string>();

            for (int i = 0; i < requestedProducts.Count; i++)
            {
                placeholders.Add($"@p{i}");
                parameters.Add(new SqliteParameter($"@p{i}", requestedProducts[i]));
            }

            var command = new SqliteCommand(@"
                SELECT r.Id, r.Name, r.Description, r.Instructions, r.ImagePath
                FROM Recipes r
                WHERE (
                    SELECT COUNT(DISTINCT p.Name)
                    FROM RecipeProducts rp
                    JOIN Products p ON rp.ProductId = p.Id
                    WHERE rp.RecipeId = r.Id AND p.Name IN (" + string.Join(", ", placeholders) + @")
                ) = @productCount
                ORDER BY r.Name;", connection);

            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            command.Parameters.AddWithValue("@productCount", requestedProducts.Count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                recipes.Add(new Recipe
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Instructions = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    ImagePath = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }
            return recipes;
        }

        public List<Recipe> FindRecipesByAnyProducts(List<string> requestedProducts)
        {
            var recipes = new List<Recipe>();

            if (requestedProducts.Count == 0)
                return recipes;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Создаем параметры вида $0, $1, $2...
            var parameters = requestedProducts.Select((_, index) => $"${index}").ToArray();
            var parametersString = string.Join(",", parameters);

            var command = new SqliteCommand($@"
                SELECT DISTINCT r.Id, r.Name, r.Description, r.Instructions, r.ImagePath
                FROM Recipes r
                JOIN RecipeProducts rp ON r.Id = rp.RecipeId
                JOIN Products p ON rp.ProductId = p.Id
                WHERE p.Name IN ({parametersString})
                ORDER BY r.Name;", connection);

            // Добавляем значения параметров
            for (int i = 0; i < requestedProducts.Count; i++)
            {
                command.Parameters.AddWithValue($"${i}", requestedProducts[i]);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                recipes.Add(new Recipe
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Instructions = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    ImagePath = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }

            return recipes;
        }
    }
}