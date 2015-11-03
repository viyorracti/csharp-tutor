<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
static readonly Regex BrRegex = new Regex(@"<br\s*/?>", RegexOptions.Compiled);

void Main()
{
	ReadTextFromDetail(File.ReadAllText(@"C:\Users\lacti\Downloads\detail.html", Encoding.UTF8)).Dump();
}

void Main2()
{
	var searchUrl = "http://www.ntis.go.kr/ThFastSearchProjectList.do?advancedFromYear=2010&advancedToYear=2015&advancedDeptChoice=&advancedDeptNew=%B0%A8%BB%E7%BF%F8&advancedDeptOld=%B0%A8%BB%E7%BF%F8&advancedPolicyProject=&advancedBudgetProjectName=&advancedProjectTitle=&advancedResearchAgencyName=&advancedManagerName=&advancedKeyword=%C0%CE%C3%BC%C0%DA%BF%F8&advancedAbstract=&advancedProjectNumber=&advancedOrganProjectNumber=";

	var doc = new HtmlDocument();
	doc.LoadHtml(GetPage(searchUrl, "EUC-KR"));

	const string detailPageName = "pjtMainInfo.do";
	var detailUrls = new List<string>();
	foreach (var node in doc.DocumentNode.SelectNodes("//a"))
	{
		var linkUrl = node.Attributes["href"].Value;
		if (linkUrl.Contains(detailPageName))
			detailUrls.Add(linkUrl);
	}
	detailUrls.Dump();
	var countNode = doc.DocumentNode.SelectNodes("//span[@class='result_num']").FirstOrDefault();
	if (countNode != null)
	{
		countNode.InnerText.Dump();
	}

	foreach (var detailUrl in detailUrls)
	{
		var texts = ReadTextFromDetail(GetPage(detailUrl, "UTF-8"));
		texts.Dump();
	}
}

string GetPage(string url, string encoding)
{
	var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
	request.Method = "GET";
	request.ServicePoint.Expect100Continue = false;
	request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36";

	using (var response = (HttpWebResponse)request.GetResponse())
	{
		var respPostStream = response.GetResponseStream();
		var readerPost = new StreamReader(respPostStream, Encoding.GetEncoding(encoding), true);
		return readerPost.ReadToEnd();
	}
}

List<TitledText> ReadTextFromDetail(string detailHtml)
{
	var doc = new HtmlDocument();
	doc.LoadHtml(detailHtml);

	var resultOffDl = doc.DocumentNode.SelectSingleNode("//dl[@class='result_off']");
	var resultOnDl = doc.DocumentNode.SelectSingleNode("//dl[@class='result_on']");
	
	var texts = new List<TitledText>();
	texts.AddRange(VisitNode(resultOffDl));
	texts.AddRange(VisitNode(resultOnDl));

	var mainDiv = doc.DocumentNode.SelectSingleNode("//div[@id='divMain']");
	var summaryDiv = doc.DocumentNode.SelectSingleNode("//div[@id='divSummary']");

	foreach (var table in mainDiv.SelectNodes("table[@class='table5']"))
		texts.AddRange(VisitNode(table));
	foreach (var table in summaryDiv.SelectNodes("table[@class='table5']"))
		texts.AddRange(VisitNode(table));
	return texts;
}

IEnumerable<TitledText> VisitNode(HtmlNode node)
{
	var targetElements = new[] { "dt", "dd", "th", "td" };
	var notYet = new Queue<TitledText>();
	
	var children = node.Name == "table"
		? node.Descendants("tr").SelectMany(e => e.ChildNodes)
		: node.ChildNodes;

	foreach (var child in children)
	{
		if (child.NodeType != HtmlNodeType.Element || targetElements.All(e => e != child.Name))
			continue;
			
		if (child.Name == "dt" || child.Name == "th")
		{
			notYet.Enqueue(new TitledText { Title = child.InnerHtml });
		}
		else
		{
			var titledText = notYet.Dequeue();
			titledText.Text = child.InnerHtml;
			yield return titledText;
		}
	}
}

string EscapeString(string text, bool header)
{
	var result = WhitespaceRegex.Replace(text.Trim(), " ").Trim();
	if (header)
	{
		result = result.Replace("\n", " ");
	}
	if (result.Contains("▶"))
	{
		result = result.Replace("▶", "\n▶").Trim();
	}
	return result;
}

class TitledText
{
	public string Title { get; set; }
	public string Text { get; set; }
}