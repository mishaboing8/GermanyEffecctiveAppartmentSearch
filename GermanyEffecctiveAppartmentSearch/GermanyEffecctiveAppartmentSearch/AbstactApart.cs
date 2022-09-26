using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace KateLivingPlace
{
	public abstract class AbstactApart : IComparable<AbstactApart>
    {
        static protected int apartementChecked = 0;

        protected float price { get; set; }
        protected float space { get; set; }
        protected string name { get; set; }
        protected string url { get; set; }
        protected float pricePerSqMeter { get; set; }
        private bool chekIfPriceOk = false;

        protected static Regex nonNum = new Regex("[^[\\d*\\.\\d*]|^[\\d*\\.?]]");//replaces no float character


        public bool isValid()
        {
            return !name.Equals("Null");
        }

        public void checkForPrice()
        {
            chekIfPriceOk = true;
        }

        public string getUrl()
        {
            return url;
        }

        public float getPricePerSqMeter()
        {
            return pricePerSqMeter;
        }

        public AbstactApart()
        {
            name = "Null";
        }

        int IComparable<AbstactApart>.CompareTo(AbstactApart? other)
        {
            return url.CompareTo(other.url);
        }

        public override string ToString()
        {
            return $"Name: {name} \nPrice per sq. meter: {pricePerSqMeter}\n{(chekIfPriceOk ? "CHECK PRICE!!\n" : "")}Price: {price} | Space: {space}\nURL: {url}";
        }
    }
}
