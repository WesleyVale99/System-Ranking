using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SistemaGenerais
{
    public class Program
    {
        public static List<Ranking> exceptions = new List<Ranking>();
        public static string NameDB, SenhaDB;
        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "API Console in Database. [Estrutura de patente]";
                WriteYellow("[Sistema] Versão de ranking -Wesley Vale.");
                WriteYellow("");
                Console.Write("[Sistema] Nome da DB: ");
                NameDB = Console.ReadLine();
                Console.Write("[Sistema] Senha da DB: ");
                SenhaDB = Console.ReadLine();
                GetGenerais();
            }
            catch(Exception ex)
            {
                WriteRed(ex.ToString());
            }

            Process.GetCurrentProcess().WaitForExit();
        }
        public static NpgsqlConnectionStringBuilder Retorno()
        {
            NpgsqlConnectionStringBuilder connBuilder = new NpgsqlConnectionStringBuilder
            {
                Database = NameDB,
                Host = "localhost",
                Username = "postgres",
                Password = SenhaDB,
                Port = 5432
            };
            return connBuilder;
        }
        public static void GetGenerais()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Retorno().ConnectionString))
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "SELECT * FROM accounts ORDER BY player_id ASC";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader data = command.ExecuteReader();
                    while (data.Read())
                    {
                        exceptions.Add(new Ranking()
                        {
                            Player_id = data.GetInt32(2),
                            Rank = data.GetInt32(6),
                            Exp = data.GetInt32(8),
                        });
                    }
                    Console.Beep();
                    Algoritm();
                    command.Dispose();
                    data.Close();
                    connection.Dispose();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static void Algoritm()
        {

            int count = 0;
            for (int i = 0; i < exceptions.Count; i++)
            {
                Ranking ranking = exceptions[i];
                if(ranking.Rank < 48)
                {
                    count++;
                    exceptions.Remove(ranking);
                }
            }
            WriteMagenta("Removendo Conta indesejadas: " + count.ToString());
            Console.WriteLine("==============================================================");
            WriteBlue("Contas com o Rank Descrescente: ");
            List<Ranking> rankingsOrdenRank = (from c in exceptions orderby c.Rank descending select c).ToList();
            for (int i = 0; i < rankingsOrdenRank.Count; i++)
            {
                Ranking ranking = rankingsOrdenRank[i];
                WriteBlue("[Player] Id: " + ranking.Player_id + " | Exp: " + ranking.Exp + " | Patente: " + ranking.Rank + "");
            }
            Console.WriteLine("==============================================================");
            WriteBlue("Contas com o Exp Descrescente: ");
            List<Ranking> rankingsOrdenExp = (from c in exceptions orderby c.Exp descending select c).ToList();
            for (int i = 0; i < rankingsOrdenExp.Count; i++)
            {
                Ranking ranking = rankingsOrdenExp[i];
                WriteBlue("[Player] Id: " + ranking.Player_id + " | Exp: " + ranking.Exp + " | Patente: " + ranking.Rank + "");
            }
            Console.WriteLine("==============================================================");
            Console.WriteLine("[Sistema] deseja formatar todo o ranking?  Pressione Enter...");
            Console.ReadLine();
            List<Ranking> InsertAndUpdate = (from c in exceptions orderby c.Exp descending select c).ToList();
            bool OK = false;
            bool fistHero = false;
            bool fistHeroi = false;
            bool fistMarechal = false;
            for (int i = 0; i < InsertAndUpdate.Count; i++)
            {
                Ranking ranking = InsertAndUpdate[i];
                if (ranking.Rank == 51 && !fistHero)
                {
                    fistHero = true;
                    OK = UpdateDB("accounts", "rank", ranking.Rank, "player_id", ranking.Player_id);
                }
                else if (ranking.Rank == 50 && !fistHeroi)
                {
                    fistHeroi = true;
                    OK = UpdateDB("accounts", "rank", ranking.Rank, "player_id", ranking.Player_id);
                }
                else if (ranking.Rank == 49 && !fistMarechal)
                {
                    fistMarechal = true;
                    OK = UpdateDB("accounts", "rank", ranking.Rank, "player_id", ranking.Player_id);
                }
                else if (ranking.Rank == 50 || ranking.Rank == 51 || ranking.Rank == 49)
                {
                    OK = UpdateDB("accounts", "rank", ranking.Rank -= 1, "player_id", ranking.Player_id);
                }
            }    
            if(OK)
                WriteGreen("Atualizado com sucesso.");
        }
        public static bool UpdateDB(string TABELA, string COLUNA, object VALOR, string req1, object valorReq1)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Retorno().ConnectionString))
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@valor", VALOR);
                    command.Parameters.AddWithValue("@req1", valorReq1);
                    command.CommandText = "UPDATE " + TABELA + " SET " + COLUNA + "=@valor WHERE " + req1 + "=@req1";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    connection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error] " + ex.ToString());
                return false;
            }
        }
        public static void WriteBlue(string texto)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(texto);
            Console.ResetColor();
        }
        public static void WriteMagenta(string texto)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(texto);
            Console.ResetColor();
        }
        public static void WriteYellow(string texto)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(texto);
            Console.ResetColor();
        }
        public static void WriteGreen(string texto)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(texto);
            Console.ResetColor();
        }
        public static void WriteRed(string texto)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(texto);
            Console.ResetColor();
        }
    }
}
