using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;
using System.Linq;
using KateLivingPlace;

WebDriver driver = new ChromeDriver();

//string url =   // "https://www.saga.hamburg/immobiliensuche?type=wohnungen";
//               //"https://www.immobilienscout24.de/Suche/de/hamburg/hamburg/hamburg-mitte/wilhelmsburg/wohnung-mieten?pricetype=calculatedtotalrent&sorting=2&enteredFrom=result_list";
//               //"https://www.immobilienscout24.de/Suche/radius/wohnung-mieten?centerofsearchaddress=Harburg%20(Kreis);;;;;&pricetype=calculatedtotalrent&geocoordinates=53.31735;10.01619;50.0&sorting=37&pagenumber=77";
//               //"https://www.immowelt.de/liste/hamburg/wohnungen/mieten?d=true&r=3&sd=DESC&sf=RELEVANCE&sp=1";//multiple pages
//               //"https://www.immowelt.de/liste/hamburg-harburg/wohnungen/mieten?d=true&sd=DESC&sf=RELEVANCE&sp=1";//one page
//               //"https://www.immowelt.de/liste/hamburg-altona-altstadt/wohnungen/mieten?d=true&lids=441360&lids=441363&lids=441365&lids=441366&sd=DESC&sf=RELEVANCE&sp=1";// two pages
//               //"https://www.immowelt.de/liste/hamburg/wohnungen/mieten?d=true&sd=DESC&sf=RELEVANCE&sp=1";//whole Hamburg
//               //"https://www.immowelt.de/liste/hamburg-lohbruegge/wohnungen/mieten?lat=53.5095&lon=10.1854&sort=relevanz%20distance&sr=1";
//               //"https://www.immowelt.de/liste/hamburg-hammerbrook/wohnungen/mieten?lat=53.5451&lon=10.0311&sort=relevanz%20distance&sr=3";//KAtes favoeite
//               //"https://www.immowelt.de/liste/hamburg-wilhelmsburg/wohnungen/miten?d=true&lids=441440&lids=8719&lids=441411&sd=DESC&sf=RELEVANCE&sp=1"; //Hammerbrook, Harburg, Wilhelmsburg etc.
//               "https://www.immobilienscout24.de/Suche/de/hamburg/hamburg/wohnung-mieten?enteredFrom=one_step_search";


string url;
while (true)
{
    Console.WriteLine("Please enter your searching url:");
    url = Console.ReadLine();

    Console.WriteLine("Your link is: " + url + "\nIf its ok enter y");

    if (Console.ReadLine().ToUpper() == "Y") break;
}

string outputFileName;

while (true)
{
    Console.WriteLine("Please enter output file name");
    outputFileName = Console.ReadLine();

    if(!File.Exists(outputFileName))
    {
        Console.WriteLine("Sorry there are no file fith path:\n" + outputFileName);
        continue;
    }

    Console.WriteLine("Your file name is:\n" + outputFileName + "\nIf its ok enter y");

    if (Console.ReadLine().ToUpper() == "Y") break;
}



Console.Clear();

SortedSet < AbstactApart> set = new SortedSet<AbstactApart>();

driver.Navigate().GoToUrl(url);

Console.WriteLine("Press Enter after dis-/agree");
Console.ReadLine();//click capcha and accept all cookies

set.UnionWith(await ApartFactory.produceApart(url, driver));

Console.WriteLine("\nReady to Print");
Console.ReadLine();

foreach (var el in set.OrderByDescending(x => x.getPricePerSqMeter()))
{
    Console.WriteLine(el.ToString() + "\n---------------------\n");
}

string toWtrite = "";

foreach (var el in set.OrderBy(x => x.getPricePerSqMeter()))
{
    toWtrite += el.ToString() + "\n---------------------\n";
}

File.WriteAllText(outputFileName, toWtrite);


Console.WriteLine(set.Count());

driver.Quit();

Console.ReadLine();