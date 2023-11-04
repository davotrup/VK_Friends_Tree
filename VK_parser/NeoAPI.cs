using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace VK_parser
{
    public class NeoAPI
    {
        public IDriver driver { get; set; }
        IAsyncSession session { get; set; }
        public NeoAPI() 
        {
            driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "11032001"));
            session = driver.AsyncSession();
        }
         ~NeoAPI() 
        {
            session.CloseAsync();
            driver.CloseAsync();
        }
        public async Task AddNode(int userid, string firstname, string lastname, int parent)
        {
            string query;
            if (parent == -1)
            {
                query = "CREATE (n:Node {userid: $userId, firstname: $firstName, lastname: $lastName}) RETURN n";
            }
            else
            {
                query = "MATCH(a:Node {userid: $Parent}) CREATE (n:Node {userid: $userId, firstname: $firstName, lastname: $lastName, parent: $Parent})-[:friend]->(a)";
            }
            var parameters = new { userId = userid, firstName = firstname, lastName = lastname, Parent = parent };

            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    // В этом блоке можно выполнять операции в рамках одной транзакции
                    var response = await tx.RunAsync(query, parameters);
                    await response.ConsumeAsync(); // Обработать результаты запроса
                });
                
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }

    }
}
