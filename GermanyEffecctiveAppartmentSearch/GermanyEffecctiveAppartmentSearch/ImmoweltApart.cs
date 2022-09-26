using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace KateLivingPlace
{
    public class ImmoweltApart : AbstactApart
    {
        public ImmoweltApart(IWebElement el)
        {
            string[] lines = el.FindElement(By.ClassName("KeyFacts-efbce")).Text.Split("\n");

            price = float.Parse(nonNum.Replace(lines[0].Replace(".", "").Replace(",", "."), ""));
            space = float.Parse(nonNum.Replace(lines[1], ""));

            name = el.FindElement(By.ClassName("FactsMain-bb891")).FindElement(By.TagName("h2")).Text;

            url = el.FindElement(By.TagName("a")).GetAttribute("href");

            pricePerSqMeter = price / space;

        }

        public ImmoweltApart() : base() { }


        public static AbstactApart tryToMake(IWebElement el)
        {
            return new ImmoweltApart(el);
        }

        public static AbstactApart waitTryToMake(WebDriver driver, IWebElement el)
        {
            for(int i = 0; i < 10; i++)
            {
                try
                {
                    return tryToMake(el);
                }
                catch (Exception)
                {
                    try
                    {
                        new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => el.Displayed && el.Enabled);
                        continue;
                    } catch (Exception)
                    {
                        return new ImmoweltApart();
                    }
                }
            }
            return new ImmoweltApart();
        }

        public void setWarmPriceFromPage(WebDriver driver)
        {
            float normalPrice = price;

            price = 0;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            wait.Until(d => d.FindElements(By.XPath("//sd-card[@class='price card']")).Count() > 0);

            var priceCard = driver.FindElement(By.XPath("//sd-card[@class='price card']")).Text;

            var lines = priceCard.Split("\n");

            if (priceCard.Contains("\nWarmmiete\n"))
            {
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (lines[i].Equals("Warmmiete"))
                    {
                        price = float.Parse(nonNum.Replace(lines[i + 1].Replace(".", "").Replace(",", ""), ""));

                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (lines[i].Equals("Kaltmiete") || lines[i].Equals("Nebenkosten") || lines[i].Equals("Heizkosten"))
                    {
                        try
                        {
                            price += float.Parse(nonNum.Replace(lines[i + 1].Replace(".", "").Replace(",", ""), ""));
                        }
                        catch (Exception)
                        {
                            try
                            {
                                price += float.Parse(nonNum.Replace(lines[i + 2].Replace(".", "").Replace(",", ""), ""));
                                i++;
                            }catch(Exception)
                            {
                               checkForPrice();
                            }
                            
                        }
                        i++;
                    }
                }
            }
            pricePerSqMeter = price / space;
        }
    }
}

