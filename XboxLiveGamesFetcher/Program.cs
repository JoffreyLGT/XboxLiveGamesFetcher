using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace XboxLiveGamesFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckNbArguments(args);

            string email = args[0];
            string password = args[1];
            string path = Directory.GetCurrentDirectory();

#if DEBUG
            // The app isn't able to write in the debug folder so we are writing in the root folder of the project
            path = @"../../../";
#endif

            // Get the path of the folder containing the executable
            string exeFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");

            bool headless = true;
            if (args.Length == 3 && args[2].ToLower().CompareTo("false") == 0)
            {
                headless = false;
            }

            XboxDotComScrapper scrapper = new XboxDotComScrapper(exeFolderPath, headless);

            if (!scrapper.Login(email, password))
            {
                Console.WriteLine("Error: invalid credentials. Please check that they are correct and retry.");
                Environment.Exit(1);
            }

            scrapper.FetchGameList();
            scrapper.FetchGamesInfo();

            ExportToFile(scrapper.Games, path);

            scrapper.Exit();
        }

        /// <summary>
        /// Check the number of arguments that are provided and display information to the user if we don't have the information that we need.
        /// </summary>
        /// <param name="args"></param>
        private static void CheckNbArguments(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: missing arguments.");
                Console.WriteLine("The correct syntax is \"XboxLiveGamesFetcher.exe xboxLiveLogin xboxLivePassword headless\". ");
                Console.WriteLine("- xboxLiveLogin = string containing your login email on xbox.com");
                Console.WriteLine("- xboxLivePassword = string containing your password on xbox.com");
                Console.WriteLine("- headless = optional, set false to be able to see what the application is doing");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Export all the informations in a CSV file containing a header.
        /// </summary>
        /// <param name="games">to insert in the Excel file</param>
        private static void ExportToFile(List<Game> games, string path)
        {
            Console.WriteLine("Starting to insert all informations in the file...");
            using (StreamWriter file = new StreamWriter(path + "/XboxLiveGames.csv"))
            {
                string header = "\"Name\";\"Time played in minutes\";\"Cover URL\";\"Info URL\"";
                file.WriteLine(header);
                foreach (Game game in games)
                {
                    if (game.TimePlayed != null && game.TimePlayed.CompareTo("error") != 0)
                    {
                        file.WriteLine("\"" + game.Name + "\";" + game.TimePlayed + ";" + "\"" + game.UrlCover + "\";" + "\"" + game.UrlInfos + "\"");
                    }
                }
            }
            Console.WriteLine("... done!");
        }
    }
}
