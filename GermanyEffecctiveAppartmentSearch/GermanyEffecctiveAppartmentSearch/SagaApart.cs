using System;
using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace KateLivingPlace
{
	public class SagaApart : AbstactApart
	{
		private static Regex digitCheck = new Regex("\\d");
		private static Regex nonDigitCheck = new Regex("\\D");

		public SagaApart() : base() { }

		public SagaApart(IWebElement el)
        {
			name = el.FindElements(By.TagName("h3"))
				.Where(e => e.GetAttribute("class").Equals("h3 teaser-h"))
				.First()
				.Text;

			url = el.FindElement(By.TagName("a")).GetAttribute("href");

			var lines = el.FindElement(By.TagName("p")).Text.Split("\n");

			foreach(var line in lines)
            {
				if (line.Contains("€")) {
					price = float.Parse(nonNum.Replace(line.Replace(".", "").Replace(",", "."), ""));
					continue;
				}

				if(line.Contains("m²"))
				{
					string spaceSubstring = line.Substring(line.IndexOf("ca."));
					space = float.Parse(nonDigitCheck.Replace(spaceSubstring, ""));
					continue;
                }
            }

			pricePerSqMeter = price / space;
		}

		public static AbstactApart tryToMake(IWebElement el)
		{
			try
			{
				return new SagaApart(el);
			}
			catch (Exception)
			{
				return new SagaApart();
			}
		}
	}
}

