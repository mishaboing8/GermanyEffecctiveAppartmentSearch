using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;

namespace KateLivingPlace
{
	public class ApartFactory {
        //driver and it's status
        static List<Tuple<WebDriver, string>> drivers = new List<Tuple<WebDriver, string>>();
        static int objectsChecked = 0;
        static int ObjectsFound = 0;


        class PricePerSqMeterComparer<AbstractApart> : IComparer<AbstactApart>
        {
            public int Compare(AbstactApart? x, AbstactApart? y)
            {
                return x.getPricePerSqMeter().CompareTo(y.getPricePerSqMeter());
            }
        }

        public static async Task<SortedSet<AbstactApart>> produceApart(string url, WebDriver driver)
        {
            objectsChecked = 0;
            ObjectsFound = 0;

            var set = new SortedSet<AbstactApart>();

            if (!drivers.Any(t => t.Item1 == driver)) drivers.Insert(0, new Tuple<WebDriver, string>(driver, "start"));

            if (url.Contains("immobilienscout24.de/"))
            {
                set.UnionWith(driver.FindElements(By.XPath("//li[@class='result-list__listing ']"))
                                .Select(el => Immobilienscout24.tryToMake(el))
                                .Where(p => p.isValid()));

                if (driver.FindElements(By.XPath("//ul[@class='reactPagination']")).Count() != 0)
                {

                    set.UnionWith(await produceImmobilienscout24SetAsync(driver));
                }
                        
            } 

            if (url.Contains("saga.hamburg/"))
            {
                set.UnionWith(driver.FindElements(By.XPath("//div[@class='teaser3 teaser3--listing teaser-simple--boxed']"))
                            .Select(el => SagaApart.tryToMake(el))
                            .Where(p => p.isValid()));
            }

            if (url.Contains("immowelt.de/"))
            {
                set.UnionWith((await produceImmoweltApart(driver)).Where(x => x.isValid()));
            }

            return set;
        }

        static async Task<SortedSet<AbstactApart>> produceImmobilienscout24SetAsync(WebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));

            SortedSet<AbstactApart> set = new SortedSet<AbstactApart>();

            while (driver.FindElements(By.XPath("//li[@class='p-items p-next vertical-center-container']")).Count() != 0)
            {
                driver.FindElement(By.XPath("//li[@class='p-items p-next vertical-center-container']")).Click();

                //Wait until next page button will be available
                wait.Until(d =>
                d.FindElements(By.XPath("//li[@class='p-items p-next vertical-center-container']")).Count() > 0
                || d.FindElements(By.XPath("//li[@class='p-items p-next vertical-center-container disabled']")).Count() > 0);

                List<Task<AbstactApart>> tasks = new List<Task<AbstactApart>>();

                Actions actions = new Actions(driver);

                foreach (var element in driver.FindElements(By.XPath("//li[@class='result-list__listing ']")))
                {
                    actions.MoveToElement(element);
                    actions.Perform();

                    tasks.Add(Task.Run(() => Immobilienscout24.tryToMake(element)));
                }

                set.UnionWith((await Task.WhenAll(tasks))
                    .Where(p => p.isValid()));
            }

            return set;
        }

        static async Task<IEnumerable<AbstactApart>> produceImmoweltApart(WebDriver driver)
        {

            Actions actions = new Actions(driver);

            int checkedApartment = 0;

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            for(int i = 0; i < 10; i++)
            {
                try
                {
                    wait.Until(
                    d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                    wait.Until(d => d.FindElements(By.XPath("//div[@class='Pagination-190de']")).Count() > 0);
                    var p = driver.FindElement(By.XPath("//div[@class='Pagination-190de']"));
                    break;
                }catch (Exception)
                {
                    continue;
                }
            }

            var pages = driver.FindElement(By.XPath("//div[@class='Pagination-190de']"));


            if (pages.FindElements(By.TagName("button")).Count() <= 1)
            {
                List<Task<AbstactApart>> currList = new List<Task<AbstactApart>>();

                foreach (var element in pages.FindElements(By.XPath("//div[@class='EstateItem-1c115']")))
                {
                    currList.Add(Task.Run(() => ImmoweltApart.waitTryToMake(driver, element)));
                }



                return (await Task.WhenAll(currList))
                    .Where(x => x.isValid())
                    .Select(ap => setWarmPrices(driver, ap, true));

                //-----------------OPEN ALL LINKS AND CLOSE THEM (SELENIUM DRIVER ERROR) (BUG FIXED) (IS SLOWER)-------------------

                //var openLinks = (await Task.WhenAll(list))
                //.Where(ap => ap.isValid())
                //.Select(ap => openLinkTuple(driver, ap))
                //.ToList();

                //var result = openLinks.Select(tuple => OpenLinkAndChangeParameter(driver, tuple));

                //driver.SwitchTo().Window(originalWindow);

                //return result;

            }

            //if there are more than 1 page

            string urlMod = driver.Url.Substring(0, driver.Url.LastIndexOf("=")-1) + "p=";
            //driver.Navigate().GoToUrl(urlMod + 1);

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                // Javascript executor
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

            var orderedPages = getPagesNumbersImmowelt(driver);
            var maxPage = orderedPages.Last().Item2;

            SortedSet<AbstactApart> set = new SortedSet<AbstactApart>();//set must be in both async/not async



            //            produceNewDrivers(1, urlMod + 1);

            //            Console.WriteLine("Accept all Cookies, by all drivers and Press enter");
            //            Console.ReadLine();

            //            Console.WriteLine($"Drivers Count {drivers.Count()}, Pages count {maxPage}");

            //            List<Task<IEnumerable<AbstactApart>>> list = new List<Task<IEnumerable<AbstactApart>>>();

            //            if (drivers.Count() < maxPage)
            //            {
            //                int pagesProDriver = maxPage / drivers.Count();

            //                for (int i = 1; i <= drivers.Count(); i++)
            //                {
            //                    int sPage = (i - 1) * pagesProDriver + 1;
            //                    int ePage = i == drivers.Count() ? maxPage : sPage + pagesProDriver - 1;
            //                    list.Add(Task.Run(() => produceImmoweltApartSeparateDriverAsync(urlMod, sPage, ePage, i - 1)));

            //                    await Task.Delay(200);
            //                }
            //            }
            //            else
            //            {
            //                for (int i = 1; i <= maxPage; i++)
            //                {
            //                    list.Add(Task.Run(() => produceImmoweltApartSeparateDriverAsync(urlMod, i, i, i - 1)));
            //                }
            //            }

            //;            var results = await Task.WhenAll(list);


            //            foreach (var res in results)
            //            {
            //                set.UnionWith(res);
            //            }

            //            return set;

            //------------------NOT ASYNC---------------------- -
            var page = 0;

            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(
                        d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                        wait.Until(d => d.FindElements(By.XPath("//div[@class='Pagination-190de']")).Count() > 0);
                        //pages = driver.FindElement(By.XPath("//div[@class='Pagination-190de']"));

                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                        // Javascript executor
                        ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }


                List<Task<AbstactApart>> list = new List<Task<AbstactApart>>();

                foreach (var element in driver.FindElements(By.XPath("//div[@class='EstateItem-1c115']")))
                {
                    list.Add(Task.Run(() => ImmoweltApart.waitTryToMake(driver, element)));
                }


                set.UnionWith((await Task.WhenAll(list)).Where(notNull));
                Console.Write($"\rFounded apartemets: {set.Count()}");


                if (page == maxPage) break;

                page++;

                for (int i = 0; i < 20; i++)
                {
                    try { driver.Navigate().GoToUrl(urlMod + page); break; }
                    catch (Exception) { continue; }
                }

            }
            Console.WriteLine();


            return set.Select(ap => setWarmPrices(driver, ap, true));
        }


        static async Task<IEnumerable<AbstactApart>> produceImmoweltApartSeparateDriverAsync(string urlMod, int startPage, int maxPage, int driverPos)
        {
            Console.WriteLine(driverPos);
            var driver = drivers.ElementAt(driverPos).Item1;

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
            var set = new HashSet<AbstactApart>();

            int currentPage = startPage;
            
            while (true)
            {
                try
                {
                    driver.Navigate().GoToUrl(urlMod + currentPage);
                    wait.Until(
                    d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                }
                catch (Exception)
                {
                    driver.Navigate().GoToUrl(urlMod + currentPage);
                    continue;
                }
                

                List<Task<AbstactApart>> list = new List<Task<AbstactApart>>();

                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                // Javascript executor
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

                foreach (var element in driver.FindElements(By.XPath("//div[@class='EstateItem-1c115']")))
                {
                    list.Add(Task.Run(() => ImmoweltApart.waitTryToMake(driver, element)));
                }

                var res = (await Task.WhenAll(list)).Where(notNull);
                ObjectsFound += res.Count();
                set.UnionWith(res);
                if(startPage == 0) Console.Write($"\rFounded apartemets: {ObjectsFound}");


                if (currentPage == maxPage) break;

                currentPage++;
            }

            if (startPage == 0) Console.WriteLine();

            int allApart = set.Count();
            int pagesMade = 0;

            var result = set.Select(ap => {
                Console.WriteLine("Driver Nr. " + driverPos + " made " + (++pagesMade) + "/" + allApart + " apartaments!");
                return setWarmPrices(driver, ap, startPage == 0);
            }).ToHashSet();

            drivers.RemoveAt(driverPos);

            driver.Quit();
            return result;
        }

        static bool notNull(AbstactApart ap)
        {
            try
            {
                return ap.isValid();
            }
            catch (Exception)
            {
                return false;
            }
        }

        static IOrderedEnumerable<(IWebElement, int)> getPagesNumbersImmowelt(WebDriver driver)
        {
            static int pageParse(string text)
            {
                try
                {
                    return int.Parse(text);
                }
                catch
                {
                    return 0;
                }
            }

            var elements = driver.FindElement(By.XPath("//div[@class='Pagination-190de']"))
                .FindElements(By.TagName("button"))
                .Select(x => (x, pageParse(x.Text)))
                .OrderBy(x => x.Item2);

            return elements;// elements.Last().x.GetAttribute("class").Contains("primary");
        }

        static AbstactApart OpenLinkAndChangeParameter(WebDriver driver, Tuple<AbstactApart, string> tuple)
        {
            var immoweltApart = (ImmoweltApart)tuple.Item1;

            for(int i = 0; i < 10; i++)
            {
                try
                {
                    driver.SwitchTo().Window(tuple.Item2);
                    immoweltApart.setWarmPriceFromPage(driver);
                    driver.Close();

                    break;
                }
                catch (Exception)
                {
                    try
                    {
                        Console.WriteLine("Error");
                        new WebDriverWait(driver, TimeSpan.FromSeconds(4)).Until(
                        d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error in cath");
                    }

                }

            }


            return immoweltApart;
        }

        static Tuple<AbstactApart, string> openLinkTuple(WebDriver driver, AbstactApart apart)
        {
            driver.SwitchTo().NewWindow(WindowType.Tab);
            driver.Navigate().GoToUrl(apart.getUrl());

            return new Tuple<AbstactApart, string>(apart, driver.CurrentWindowHandle);
        }

        static AbstactApart setWarmPrices(WebDriver driver, AbstactApart apart, bool toPrint)
        {
            for(int i = 0; i < 10; i++)
            {
                try
                {
                    driver.Navigate().GoToUrl(apart.getUrl());
                    var immoWelt = (ImmoweltApart)apart;

                    immoWelt.setWarmPriceFromPage(driver);
                    objectsChecked++;

                    if(toPrint) Console.Write($"\rChecked apartment nr. {objectsChecked}");
                    return immoWelt;
                }
                catch (Exception)
                {
                    try
                    {
                        new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(
                        d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }

            }

            apart.checkForPrice();

            return apart;

        }

        static void produceNewDrivers(int count, string startUrl)
        {
            for(int i = 0; i < count; i++)
            {
                drivers.Add(new Tuple<WebDriver, string>(new ChromeDriver(),"start"));
            }

            if (startUrl.Equals("none")) return;

            foreach(var tuple in drivers)
            {
                for(int i = 0; i < 10; i++)
                {
                    try
                    {
                        tuple.Item1.Navigate().GoToUrl(startUrl);
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }
    }
}