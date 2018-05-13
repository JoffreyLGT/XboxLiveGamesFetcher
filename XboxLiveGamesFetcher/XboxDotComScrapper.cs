using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XboxLiveGamesFetcher
{
    class XboxDotComScrapper
    {
        private IWebDriver driver;
        public List<Game> Games { get; set; }

        /// <summary>
        /// Create a new driver and get it ready for the scrapping.
        /// </summary>
        /// <param name="driverPath">path to chromedriver</param>
        /// <param name="headless">false if you want to see the chrome graphical interface and follow easily what is happening</param>
        public XboxDotComScrapper(string driverPath, bool headless = true)
        {
            ChromeOptions options = new ChromeOptions();
            if (headless)
            {
                options.AddArgument("headless");
            }
            options.AddArgument("log-level=3");
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

            IWebDriver driver = new ChromeDriver(driverPath, options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            this.driver = driver;
        }

        /// <summary>
        /// Login on Xbox.com.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="driver"></param>
        /// <returns>true if the login was successfull</returns>
        public bool Login(string email, string password)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            Console.WriteLine("Starting the login on Xbox.com");
            driver.Navigate().GoToUrl("https://login.live.com/login.srf?wa=wsignin1.0&rpsnv=13&rver=6.7.6643.0&wp=MBI_SSL&wreply=https:%2f%2faccount.xbox.com%2fen-US%2fsocial%3fxr%3dshellnav%26SilentAuth%3d1&lc=1033&id=292543&aadredir=1");
            driver.FindElement(By.Id("i0116")).Click();
            driver.FindElement(By.Id("i0116")).Clear();
            driver.FindElement(By.Id("i0116")).SendKeys(email);
            driver.FindElement(By.Id("idSIButton9")).Click();
            try
            {
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("i0118")));
            }
            catch (Exception)
            {
                return false;
            }
            driver.FindElement(By.Id("i0118")).Click();
            driver.FindElement(By.Id("i0118")).Clear();
            driver.FindElement(By.Id("i0118")).SendKeys(password);
            driver.FindElement(By.Id("idSIButton9")).Click();

            try
            {
                driver.FindElement(By.Id("shellmenu_42"));
            }
            catch (Exception)
            {
                return false;
            }
            Console.WriteLine("Success!");
            return true;
        }

        /// <summary>
        /// Fetch the library of games.
        /// </summary>
        public void FetchGameList()
        {
            Console.WriteLine("Fetching the list of the games.");
            #region Open achievment list
            driver.FindElement(By.Id("shellmenu_44")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(By.Id("spa-loading-background")));
            driver.FindElement(By.Id("GamerAchievementsFilter")).Click();
            #endregion

            List<Game> games = new List<Game>();

            IReadOnlyCollection<IWebElement> gameElements = driver.FindElements(By.XPath("//*[@id=\"gamesList\"]/ul/li"));
            foreach (IWebElement gameElement in gameElements)
            {
                IWebElement gameInfo = gameElement.FindElement(By.XPath("xbox-title-item/div/a"));
                Console.Write("Found " + gameInfo.GetAttribute("aria-label") + ", checking if it's a XBL game... ");

                string scoreProgression = gameInfo.FindElement(By.CssSelector("div.recentProgressInfo > div.gamerscoreinfo.c-glyph.glyph-gamerscore")).Text;
                int maximumScore = Convert.ToInt16(scoreProgression.Split('/')[1]);

                if (maximumScore > 0)
                {
                    Game game = new Game(gameInfo.GetAttribute("aria-label"), gameInfo.GetAttribute("href") + "&activetab=main:maintab2");
                    games.Add(game);
                    Console.WriteLine("done, it's an XboxLive game!");
                }
                else
                {
                    Console.WriteLine("done, but ignored: it's not a XboxLive game.");
                }
            }
            Console.WriteLine("Done fetching the list of the games.");
            this.Games = games; ;
        }

        /// <summary>
        /// Fetch the url of each of the games in parameter and get the time played.
        /// </summary>
        public void FetchGamesInfo()
        {
            Console.WriteLine("Starting to fetch the games stats.");
            List<Game> games = this.Games;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            int i = 1;
            int nbGames = games.Count;
            foreach (Game game in games)
            {
                Console.WriteLine("Fetching game " + i.ToString() + " out of " + nbGames);
                driver.Navigate().GoToUrl(game.UrlInfos);
                string timePlayed = string.Empty;
                try
                {
                    timePlayed = driver.FindElement(By.XPath("//*[@id=\"xbox-gamestatistics-xejtod6\"]/div/div[contains(.,\"Minutes Played\")]/div")).Text;
                    game.UrlCover = driver.FindElement(By.CssSelector("#left-side > div > img")).GetAttribute("src");
                }
                catch (Exception)
                {
                    timePlayed = "error";
                    Console.WriteLine("Error when fetching the time played for " + game.Name);
                }

                game.TimePlayed = timePlayed;
                i++;
            }
            Console.WriteLine("All game stats have been fetched.");
            this.Games = games;
        }

        /// <summary>
        /// Close the driver.
        /// </summary>
        public void Exit()
        {
            driver.Quit();
        }
    }
}
