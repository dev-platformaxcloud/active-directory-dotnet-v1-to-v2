﻿
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonCacheADALV5
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
            DoIt().Wait();
        }

        static async Task DoIt()
        {
            AppCoordinates.AppCoordinates app = AppCoordinates.PreRegisteredApps.GetV1App(useInMsal: false);
            string resource = AppCoordinates.PreRegisteredApps.MsGraph;

            string cacheFolder = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"..\..\..\..");
            string adalV3cacheFileName = Path.Combine(cacheFolder, "cacheAdalV3.bin");
            string msalCacheFileName = Path.Combine(cacheFolder, "cacheMsal.bin");
            FilesBasedTokenCache tokenCache = new FilesBasedTokenCache(adalV3cacheFileName, msalCacheFileName);
            AuthenticationContext authenticationContext = new AuthenticationContext(app.Authority, tokenCache);

            AuthenticationResult result;
            try
            {
                result = await authenticationContext.AcquireTokenSilentAsync(resource, app.ClientId);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"got token for '{result.UserInfo.DisplayableId}' from the cache");
                Console.ResetColor();
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                result = await authenticationContext.AcquireTokenAsync(resource, app.ClientId, app.RedirectUri, new PlatformParameters(PromptBehavior.SelectAccount));
                Console.WriteLine($"got token for '{result.UserInfo.DisplayableId}' without the cache");
            }

        }
    }
}
