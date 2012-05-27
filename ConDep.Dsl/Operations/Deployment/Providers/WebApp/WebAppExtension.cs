using System;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Operations.WebDeploy.Options;

namespace ConDep.Dsl
{
	public static class WebAppExtension
	{
		public static void WebApp(this IProvideForExistingIisServer providerCollection, string sourceDir, string webAppName, string destinationWebSiteName)
		{
		    AddProvider(sourceDir, webAppName, destinationWebSiteName, providerCollection);
		}

	    public static void WebApp(this IProvideForCustomIis providerCollection, string sourceDir, string webAppName, string destinationWebSiteName)
        {
            AddProvider(sourceDir, webAppName, destinationWebSiteName, providerCollection);
        }

        public static void WebApp(this IProvideForCustomWebSite providerCollection, string sourceDir, string webAppName, string destinationWebSiteName)
        {
            AddProvider(sourceDir, webAppName, destinationWebSiteName, providerCollection);
        }

        private static void AddProvider(string sourceDir, string webAppName, string destinationWebSiteName, IProviderCollection providerCollection)
        {
            var webAppProvider = new WebAppProvider(sourceDir, webAppName, destinationWebSiteName);
            providerCollection.AddProvider(webAppProvider);
        }
    }
}