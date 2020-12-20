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
                figuredataReader = new FiguredataReader();
                figuredataReader.LoadFigurePalettes();
                figuredataReader.loadFigureSetTypes();
                figuredataReader.LoadFigureSets();

                FigureExtractor.Parse();
            }

            bool isSmall = false;
            int renderDirection = 0;
            string figure = null;

            if (Request.Query.ContainsKey("figure"))
            {
                Request.Query.TryGetValue("figure", out var value);
                figure = value.ToString();
            }

            if (Request.Query.ContainsKey("s"))
            {
                Request.Query.TryGetValue("s", out var value);

                if (value == "1" || value == "true")
                {
                    isSmall = true;
                }
            }

            if (Request.Query.ContainsKey("small"))
            {
                Request.Query.TryGetValue("small", out var value);

                if (value == "1" || value == "true")
                {
                    isSmall = true;
                }
            }


            if (Request.Query.ContainsKey("direction"))
            {
                Request.Query.TryGetValue("direction", out var value);

                if (value.ToString().IsNumeric())
                {
                    renderDirection = int.Parse(value.ToString());
                }
            }

            if (Request.Query.ContainsKey("rotation"))
            {
                Request.Query.TryGetValue("rotation", out var value);

                if (value.ToString().IsNumeric())
                {
                    renderDirection = int.Parse(value.ToString());
                }
            }

            if (figure != null && figure.Length > 0)
            {
                var furni = new Avatar(figure, isSmall, renderDirection, renderDirection, figuredataReader);

                return File(furni.Run(), "image/png");
            }

            return null;
        }
    }
}
