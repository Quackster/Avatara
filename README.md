# Avatara
Habbo Hotel figure avatar imager for 2009 era figure codes.

Thanks to Webbanditten for solving some rendering problems.

## How to use?

Compile the AvataraWebApp to the operating system of your choice (it runs on .NET 5 and works on Linux).

Run the app.

(On Linux for example)

``./AvataraWebApp --urls=http://*:8090/``

(On Windows for example)

``AvataraWebApp.exe --urls=http://*:8090/``

Then proxy it through PHP.

```php
<?php
header ('Content-Type: image/png');
echo file_get_contents("http://127.0.0.1:8090/?" . $_SERVER['QUERY_STRING']);
?>
```
