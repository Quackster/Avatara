# Avatara
Habbo Hotel figure avatar imager for 2009 era figure codes.

Thanks to Webbanditten for solving some rendering problems.

## How to use?

Compile the AvataraWebApp to the operating system of your choice (it runs on .NET 5 and works on Linux).

Extract *figuredata-shockwave.zip* if you plan to use 2012-2007 era clothing. 

Extract *figuredata-2013.zip* if you plan to use 2013 era clothing.

You can add your own SWFs by simply replacing the SWFS in /figuredata/compiled/ and also replace the figuredata.xml.

If the **xml** and **images** folder doesn't exist, Avatara will automatically create the folders and extract the SWFs on first run, so that each subsequent run is quicker.

Run the app.

(On Linux for example)

``./AvataraWebApp --urls=http://*:8090/``

(On Windows for example)

``AvataraWebApp.exe --urls=http://*:8090/``

Then it should be accessible via http://localhost:8080/habbo-imaging/avatarimage?figure=hd-180-1.hr-100-61.ch-210-66.lg-270-82.sh-290-80 - giving you this image.

![image](https://user-images.githubusercontent.com/1328523/211535708-8fb6e931-4087-4d54-aae0-90a7e629bad6.png)

## How do I use it for my CMS?

Then proxy it through PHP.

```php
<?php
header ('Content-Type: image/png');
echo file_get_contents("http://127.0.0.1:8090/?" . $_SERVER['QUERY_STRING']);
?>
```
