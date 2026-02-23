using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace KalolCommunity.Api
{
    public class StateInsertGenerator
    {
        public static void GenerateInserts()
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets<StateInsertGenerator>()
                .Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

            // Read JSON file with UTF-8 encoding
            string jsonFilePath = "CountryState.json";
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine($"Error: {jsonFilePath} not found!");
                return;
            }

            string jsonContent = File.ReadAllText(jsonFilePath, Encoding.UTF8);
            var countryStateData = JsonSerializer.Deserialize<CountryStateData>(jsonContent);

            StringBuilder sqlStatements = new StringBuilder();
            sqlStatements.AppendLine("-- ==========================================");
            sqlStatements.AppendLine("-- SQL INSERT Statements for TblStates");
            sqlStatements.AppendLine($"-- Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sqlStatements.AppendLine("-- ==========================================");
            sqlStatements.AppendLine();
            sqlStatements.AppendLine("SET IDENTITY_INSERT TblStates OFF;");
            sqlStatements.AppendLine("GO");
            sqlStatements.AppendLine();

            int totalStates = 0;
            int countriesProcessed = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Database connection established.");

                    foreach (var country in countryStateData.data)
                    {
                        // Get CountryId from TblCountries using iso2 as CountryCode
                        string query = "SELECT CountryId FROM TblCountries WHERE CountryCode = @CountryCode";
                        
                        int? countryId = null;
                        
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@CountryCode", country.iso2);
                            
                            var result = command.ExecuteScalar();
                            
                            if (result != null)
                            {
                                countryId = Convert.ToInt32(result);
                            }
                        }

                        if (countryId.HasValue && country.states != null && country.states.Count > 0)
                        {
                            sqlStatements.AppendLine($"-- ==========================================");
                            sqlStatements.AppendLine($"-- States for: {country.name}");
                            sqlStatements.AppendLine($"-- Country Code: {country.iso2}");
                            sqlStatements.AppendLine($"-- Country ID: {countryId}");
                            sqlStatements.AppendLine($"-- Total States: {country.states.Count}");
                            sqlStatements.AppendLine($"-- ==========================================");
                            
                            foreach (var state in country.states)
                            {
                                // Escape single quotes in state names
                                string stateName = state.name.Replace("'", "''");
                                string stateCode = state.state_code?.Replace("'", "''") ?? "";
                                
                                string insertStatement = $"INSERT INTO TblStates (StateName, CountryId, StateCode) " +
                                                        $"VALUES ('{stateName}', {countryId}, '{stateCode}');";
                                
                                sqlStatements.AppendLine(insertStatement);
                                totalStates++;
                            }
                            
                            sqlStatements.AppendLine();
                            countriesProcessed++;
                            
                            Console.WriteLine($"Processed: {country.name} ({country.states.Count} states)");
                        }
                        else if (!countryId.HasValue)
                        {
                            sqlStatements.AppendLine($"-- WARNING: Country '{country.name}' (iso2: {country.iso2}) not found in TblCountries");
                            sqlStatements.AppendLine();
                            Console.WriteLine($"WARNING: Country '{country.name}' not found in database");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                sqlStatements.AppendLine($"-- ERROR: {ex.Message}");
            }

            sqlStatements.AppendLine();
            sqlStatements.AppendLine("-- ==========================================");
            sqlStatements.AppendLine($"-- SUMMARY");
            sqlStatements.AppendLine($"-- Countries Processed: {countriesProcessed}");
            sqlStatements.AppendLine($"-- Total INSERT Statements: {totalStates}");
            sqlStatements.AppendLine("-- ==========================================");

            // Save to file with UTF-8 encoding
            string outputFilePath = "InsertStates.sql";
            File.WriteAllText(outputFilePath, sqlStatements.ToString(), Encoding.UTF8);

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("SUCCESS: SQL INSERT statements generated!");
            Console.WriteLine($"Countries processed: {countriesProcessed}");
            Console.WriteLine($"Total states: {totalStates}");
            Console.WriteLine($"Output file: {outputFilePath}");
            Console.WriteLine("========================================");
        }
    }

    public class CountryStateData
    {
        public bool error { get; set; }
        public string msg { get; set; }
        public List<CountryData> data { get; set; }
    }

    public class CountryData
    {
        public string name { get; set; }
        public string iso3 { get; set; }
        public string iso2 { get; set; }
        public List<StateData> states { get; set; }
    }

    public class StateData
    {
        public string name { get; set; }
        public string state_code { get; set; }
    }

}