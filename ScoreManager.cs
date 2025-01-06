// ScoreManager.cs
using System.Data.SqlClient;

namespace Staircases
{
    public class ScoreManager
    {
        private readonly string connectionString;

        public ScoreManager(string connectionString)
        {
            this.connectionString = connectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            
            var command = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Scores')
                CREATE TABLE Scores (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Score INT NOT NULL,
                    Timestamp DATETIME DEFAULT GETDATE()
                )", connection);
            
            command.ExecuteNonQuery();
        }

        public void SaveScore(int score)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            
            var command = new SqlCommand(
                "INSERT INTO Scores (Score) VALUES (@Score)", connection);
            command.Parameters.AddWithValue("@Score", score);
            
            command.ExecuteNonQuery();
        }

        public List<(int Score, DateTime Timestamp)> GetTopScores(int count = 10)
        {
            var scores = new List<(int Score, DateTime Timestamp)>();
            
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            
            var command = new SqlCommand(
                "SELECT TOP (@Count) Score, Timestamp FROM Scores ORDER BY Score DESC",
                connection);
            command.Parameters.AddWithValue("@Count", count);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                scores.Add((
                    reader.GetInt32(0),
                    reader.GetDateTime(1)
                ));
            }
            
            return scores;
        }
    }
}