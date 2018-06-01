# Directory Crawler

Directory Crawler is a simple command-line program that is built in .NET C# to crawl all accessible directories within a target directory (entry point) and output the results to a text file. I created this program because I need to monitor some shared directories within a local network.

```shell
DirectoryCrawler.exe /targetdir=<PATH_TO_TARGET_FOLDER> 
```

![DirectoryCrawler](https://i.imgur.com/Re1267D.gif)

After the program finished running, an output text file will be created within the program base folder and if you open the text file, this is how it looked like; a list of accessible directories within the target directory.

Example of output:

![Output file](http://i.imgur.com/qaUZ9n3.png)

### License

[MIT](LICENSE.md)
