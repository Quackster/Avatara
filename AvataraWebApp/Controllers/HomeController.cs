using Avatara;
using Avatara.Figure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AvataraWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static FiguredataReader figuredataReader;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

        }

        public IActionResult Index()
        {
            if (figuredataReader == null)
            {
                FigureExtractor.Parse();

                figuredataReader = new FiguredataReader();
                figuredataReader.LoadFigurePalettes();
                figuredataReader.loadFigureSetTypes();
                figuredataReader.LoadFigureSets();
            }

            bool isSmall = false;
            int bodyDirection = 2;
            int headDirection = 2;
            string figure = null;
            string action = "std";
            string gesture = "sml";
            bool headOnly = false;
            int frame = 1;
            int carryDrink = -1;

            if (Request.Query.ContainsKey("figure"))
            {
                Request.Query.TryGetValue("figure", out var value);
                figure = value.ToString();
            }

            if (Request.Query.ContainsKey("action"))
            {
                Request.Query.TryGetValue("action", out var value);
                action = value.ToString();
            }

            if (Request.Query.ContainsKey("gesture"))
            {
                Request.Query.TryGetValue("gesture", out var value);
                gesture = value.ToString();
            }

            if (Request.Query.ContainsKey("figure"))
            {
                Request.Query.TryGetValue("figure", out var value);
                figure = value.ToString();
            }

            if (Request.Query.ContainsKey("size"))
            {
                Request.Query.TryGetValue("size", out var value);

                if (value == "s")
                {
                    isSmall = true;
                }
            }

            if (Request.Query.ContainsKey("head"))
            {
                Request.Query.TryGetValue("head", out var value);
                headOnly = value.ToString() == "1" || value.ToString() == "true";
            }

            if (Request.Query.ContainsKey("direction"))
            {
                Request.Query.TryGetValue("direction", out var value);

                if (value.ToString().IsNumeric())
                {
                    bodyDirection = int.Parse(value.ToString());
                }
            }

            if (Request.Query.ContainsKey("head_direction"))
            {
                Request.Query.TryGetValue("head_direction", out var value);

                if (value.ToString().IsNumeric())
                {
                    headDirection = int.Parse(value.ToString());
                }
            }

            if (Request.Query.ContainsKey("frame"))
            {
                Request.Query.TryGetValue("frame", out var value);

                if (value.ToString().IsNumeric())
                {
                    int v = int.Parse(value.ToString());
                    frame = v < 1 ? 1 : v;
                }
            }

            if (Request.Query.ContainsKey("drk"))
            {
                Request.Query.TryGetValue("drk", out var value);
                action = (value.ToString() == "1" || value.ToString() == "true") ? "drk" : action;
            }

            if (Request.Query.ContainsKey("crr"))
            {
                Request.Query.TryGetValue("crr", out var value);

                if (value.ToString().IsNumeric())
                {
                    carryDrink = int.Parse(value.ToString());
                }
            }

            if (figure != null && figure.Length > 0)
            {
                var furni = new Avatar(figure, isSmall, bodyDirection, headDirection, figuredataReader, action: action, gesture: gesture, headOnly: headOnly, frame: frame, carryDrink: carryDrink);

                return File(furni.Run(), "image/png");
            }

            return null;
        }
    }
}
