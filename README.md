# Avatara
Habbo Hotel figure avatar imager for 2009 era figure codes.

Thanks to Webbanditten for solving some rendering problems.

## How to Use

Compile the AvataraWebApp to the operating system of your choice (it runs on .NET 5+ and works on Linux).

Extract *figuredata-shockwave.zip* if you plan to use 2012-2007 era clothing. 

Extract *figuredata-2013.zip* if you plan to use 2013 era clothing.

You can add your own SWFs by simply replacing the SWFS in /figuredata/compiled/ and also replace the figuredata.xml.

If the **xml** and **images** folder doesn't exist, Avatara will automatically create the folders and extract the SWFs on first run, so that each subsequent run is quicker.

```c
// ... store it as a field

private FiguredataReader? figuredataReader;

// ... in the constructor somewhere

if (figuredataReader == null)
{
    FigureExtractor.Parse();

    figuredataReader = new FiguredataReader();
    figuredataReader.LoadFigurePalettes();
    figuredataReader.loadFigureSetTypes();
    figuredataReader.LoadFigureSets();
}

// ...when rendering...

var avatar = new Avatar(figure, size, bodyDirection, headDirection, figuredataReader, action: action, gesture: gesture, headOnly: headOnly, frame: frame, carryDrink: carryDrink, cropImage: cropImage);
var figureData = avatar.Run();
```

## As a web server?

See: https://github.com/Quackster/Helios.Imager
