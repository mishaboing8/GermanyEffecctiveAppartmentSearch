using System;
using OpenQA.Selenium;

namespace KateLivingPlace
{
	public class Immobilienscout24 : AbstactApart
	{
        public Immobilienscout24() : base() { }

		public Immobilienscout24(IWebElement e)
        {
            var list = e.FindElements(By.TagName("dd"))
                .Where(el => el.GetAttribute("class").Equals("font-highlight font-tabular"))
                .ToList();

            price = float.Parse(nonNum.Replace(list[0].Text.Replace(".", "").Replace(",", "."), ""));
            space = float.Parse(nonNum.Replace(list[1].Text.Replace(",", "."), ""));

            pricePerSqMeter = price / space;

            name = e.FindElements(By.TagName("h5"))
                .Where(el => el.GetAttribute("class").Equals("result-list-entry__brand-title font-h6 onlyLarge font-ellipsis font-regular nine-tenths"))
                .First().Text.Replace("NEU", "").Trim();

            url = e.FindElements(By.TagName("a")).Where(el => el.GetAttribute("class").Equals("result-list-entry__brand-title-container "))
                .First()
                .GetAttribute("href");
        }

        public static AbstactApart tryToMake(IWebElement el)
        {
            try
            {
                apartementChecked++;
                Console.Write("\rApartement Chaecked: " + apartementChecked);

                return new Immobilienscout24(el);
            }
            catch (Exception)
            {
                return new Immobilienscout24();
            }
        }
    }
}

