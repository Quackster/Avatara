# Avatara
Habbo Hotel figure avatar imager for 2009 era figure codes.

## How to Use

"Avatara" on its own, is a figure library, it is not a web server!

Extract *figuredata-shockwave.zip* if you plan to use 2012-2007 era clothing. 

Extract *figuredata-2013.zip* if you plan to use 2013 era clothing.

You can add your own SWFs by simply replacing the SWFS in /figuredata/compiled/ and also replace the figuredata.xml.

If the **xml** and **images** folder doesn't exist, Avatara will automatically create the folders and extract the SWFs on first run, so that each subsequent run is quicker.

```c

// ... only need to be called once

FiguredataReader.Instance.Load();
FlashExtractor.Instance.Load();

// ... the imager arguments...

string? figure = "hd-180-1.hr-100-61.ch-210-66.lg-270-82.sh-290-80";
string size = "b";
int bodyDirection = 2;
int headDirection = 2;
string action = "std";
string gesture = "sml";
bool headOnly = false;
int frame = 1;
int carryDrink = -1;
bool cropImage = false;

// .. generating the PNG output

var avatar = new Avatar(FiguredataReader.Instance, figure, size, bodyDirection, headDirection, action: action, gesture: gesture, headOnly: headOnly, frame: frame, carryDrink: carryDrink, cropImage: cropImage);
File.WriteAllBytes("figure.png", avatar.Run());

```

## As a web server?

See: https://github.com/Quackster/Minerva

## Credits

Thanks to ArachisH for the Flazzy project. 
Forked: https://github.com/Quackster/Flazzy

Thanks to Webbanditten for solving some rendering problems.
