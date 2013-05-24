using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Repository.Azure
{

    internal static class AzureUtility
    {
        private static readonly char[] BannedCharacters = new[] { '<', '>', '\'', '[', ']', '`' };
        public const String EMULATOR_CONNECTION_STRING = "UseDevelopmentStorage=true;";

        //===============================================================
        public static String SanitizeContainerName(String name)
        {
            // Must be lowercase
            name = name.ToLower();

            // Can only be alphanumeric and '-'
            name = BannedCharacters.Aggregate(name, (current, character) => current.Replace(character, '-'));

            // Can't have two '-' in a row
            while (name.Contains("--"))
                name = name.Replace("--", "-1-");

            // Must be at least 3 chars long
            if (name.Length == 1)
                name = name + "-";
            else if (name.Length == 2)
                name = name + "-1";

            return name;
        }
        //===============================================================
        public static String GetSanitizedContainerName(Type type)
        {
            var name = type.Name;

            // Handle generic args
            if (type.GenericTypeArguments.Any())
                name = type.GenericTypeArguments.Aggregate(name, (current, t) => current + ("-" + GetSanitizedContainerName(t)));

            return SanitizeContainerName(name);
        }
        //===============================================================
        public static String GetSanitizedContainerName<TValue>()
        {
            return GetSanitizedContainerName(typeof(TValue));
        }
        //===============================================================
        public static String GetNamedConnectionString(String name)
        {
            var connStr = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            if (String.IsNullOrWhiteSpace(connStr))
                connStr = WebConfigurationManager.ConnectionStrings[name].ConnectionString;

            return connStr;
        }
        //===============================================================
    }
}
