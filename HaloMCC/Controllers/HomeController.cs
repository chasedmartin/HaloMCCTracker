using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestSharp;
using Newtonsoft.Json;
using System.Xml;
using System.Threading;

namespace HaloMCC.Controllers
{
    public class HomeController : Controller
    {
        public class XboxAchievements
        {
            public int id { get; set; }
            public string name { get; set; }
            public string progressState { get; set; }
            public string description { get; set; }
            public string cateogry { get; set; }
            public string game { get; set; }
            public string levelmap { get; set; }
            public string reward { get; set; }
            public int gamerscore { get; set; }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Achievements(string gamertag)
        {
            try
            {
                List<XboxAchievements> cheevos = new List<XboxAchievements>();
                var client = new RestClient("https://xboxapi.com");
                double achievementCount = 0;
                int totalGamerscore = 0;

                var idRequest = new RestRequest("/v2/xuid/" + gamertag, Method.GET);
                idRequest.AddHeader("X-AUTH", "????????");
                string idContent = "";

                for (int i = 0; i < 5; i++)
                {
                    var idResponse = client.Execute(idRequest);
                    idContent = idResponse.Content;
                    if (idContent != null || idContent != "")
                    {
                        break;
                    }
                    if (idResponse.StatusDescription != "OK")
                    {
                        ViewBag.HtmlStr = idResponse.StatusDescription;
                        return View("Index");
                    }
                    if (!idContent.Contains("xuid") && i == 4)
                    {
                        ViewBag.HtmlStr = "Received Bad Response";
                        return View("Index");
                    }
                    Thread.Sleep(2000);
                }

                XmlDocument idDoc = JsonConvert.DeserializeXmlNode("{\"root\":" + idContent + "}", "root");
                var xuid = idDoc.DocumentElement.InnerText;

                var request = new RestRequest("/v2/" + xuid + "/achievements/?????", Method.GET);
                request.AddHeader("X-AUTH", "??????");
                string content = "";

                for (int i = 0; i < 5; i++)
                {
                    var response = client.Execute(request);
                    content = response.Content;
                    if (content.Contains("id"))
                    {
                        break;
                    }
                    if (response.StatusDescription != "OK")
                    {
                        ViewBag.HtmlStr = response.StatusDescription;
                        return View("Index");
                    }
                    if (!content.Contains("id") && i == 4)
                    {
                        ViewBag.HtmlStr = "Received Bad Response";
                        return View("Index");
                    }
                    Thread.Sleep(2000);
                }

                XmlDocument doc = JsonConvert.DeserializeXmlNode("{\"root\":" + content + "}", "root");

                var model = new HaloMCC4676_dbEntities().achievements;

                foreach (XmlNode node in doc.DocumentElement)
                {
                    XboxAchievements cheevo = new XboxAchievements();

                    cheevo.id = Convert.ToInt32(node.SelectSingleNode("id").InnerText);
                    cheevo.name = node.SelectSingleNode("name").InnerText;
                    cheevo.description = node.SelectSingleNode("description").InnerText;
                    cheevo.gamerscore = Int32.Parse(node.SelectSingleNode("rewards").SelectSingleNode("value").InnerText);

                    var result = from a in model
                                 where a.Id.ToString().Equals(cheevo.id.ToString())
                                 select new { Cateogry = a.category1.name.ToString(), Game = a.game.ToString(), Levelmap = a.levelmap.ToString(), Reward = a.reward.ToString() };

                    cheevo.cateogry = result.First().Cateogry;
                    cheevo.game = result.First().Game;
                    cheevo.levelmap = result.First().Levelmap;
                    cheevo.reward = result.First().Reward;

                    //Find progress
                    if (node.SelectSingleNode("progressState").InnerText == "Achieved")
                    {
                        cheevo.progressState = "<strong><span style=\"color:green\">" + cheevo.gamerscore + "G</span></strong>";
                        totalGamerscore += cheevo.gamerscore;
                        achievementCount++;
                    }
                    else if (node.SelectSingleNode("progressState").InnerText == "InProgress" & cheevo.game.Equals("5"))
                    {
                        try
                        {
                            int progress = (int)Math.Round(Double.Parse(node.SelectSingleNode("progression").SelectSingleNode("requirements").SelectSingleNode("current").InnerText) / Double.Parse(node.SelectSingleNode("progression").SelectSingleNode("requirements").SelectSingleNode("target").InnerText) * 100);
                            if (progress < 100)
                            {
                                cheevo.progressState = "<span style=\"color:red\">" + progress + "%</span>";
                            }
                            else
                            {
                                cheevo.progressState = "<span class=\"glyphicon glyphicon-remove\" style=\"color:red\"></span>";
                            }
                        }
                        catch (Exception e)
                        {
                            cheevo.progressState = "<span class=\"glyphicon glyphicon-remove\" style=\"color:red\"></span>";
                        }
                    }
                    else
                    {
                        cheevo.progressState = "<span class=\"glyphicon glyphicon-remove\" style=\"color:red\"></span>";
                    }

                    cheevos.Add(cheevo);
                }

                ViewBag.Title = "Halo Tracker - Spartan " + gamertag.ToUpper();
                ViewBag.HtmlStr = gamertag.ToUpper();

                ViewBag.achievementScore = totalGamerscore + "G";
                ViewBag.achievementPercent = Math.Round(achievementCount / 450 * 100) + "%";
                ViewBag.gamerscorePercent = Math.Round((double)totalGamerscore / 4500 * 100) + "%";
                
                return View("game", cheevos);
            }
            catch (Exception e)
            {
                ViewBag.HtmlStr = "Server Error - " + e.Message;
                return View("Index");
            }
        }
    }
}