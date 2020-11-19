using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using PuppeteerSharp;
using System.Net;
using System.Collections.Generic;

namespace PuppeteerSharp
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var downloadPath = Path.Combine(currentDirectory, "CustomChromium");
            Console.WriteLine($"Attemping to set up puppeteer to use Chromium found under directory {downloadPath} ");

            if (!Directory.Exists(downloadPath))
            {
                Console.WriteLine("Custom directory not found. Creating directory");
                Directory.CreateDirectory(downloadPath);
            }

            Console.WriteLine("Downloading Chromium...");

            var browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
            var browserFetcher = new BrowserFetcher(browserFetcherOptions);
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);

            var executablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultRevision);

            if (string.IsNullOrEmpty(executablePath))
            {
                Console.WriteLine("Custom Chromium location is empty. Unable to start Chromium. Exiting.\n Press any key to continue");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Attemping to start Chromium using executable path: {executablePath}");

            var options = new LaunchOptions { Headless = true, ExecutablePath = executablePath };

            using (var browser = await Puppeteer.LaunchAsync(options))
            using (var page = await browser.NewPageAsync())
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
                
                var waitUntil = new NavigationOptions { Timeout = 0, WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } };
                string url = "https://github.com/puppeteer/puppeteer/issues/1345";
                await page.GoToAsync(url,waitUntil);

                #region Screenshot Dashboard:
                var optionsScreenShot = new ScreenshotOptions { FullPage = true };
                //Đường dẫn lưu file
                var savePath = Path.Combine(currentDirectory, "Capture");
                if (!Directory.Exists(savePath))
                {
                    Console.WriteLine("SavePath directory not found. Creating directory");
                    Directory.CreateDirectory(savePath);
                }
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");
                var outputfile = savePath + "/capture_"+date+".png";
                await page.ScreenshotAsync(outputfile, optionsScreenShot);
                Console.WriteLine("Capture completed! Path: " + outputfile, ConsoleColor.Green);
                #endregion
                
                await page.CloseAsync();
            }
            return;
        }
    }
}
