using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Repository.Azure
{

    internal static class AzureUtility
    {
        //===============================================================
        public static String GetSanitizedContainerName(Type type)
        {
            var bannedCharacters = new[] { '<', '>', '\'', '[', ']', '`' };
            var name = type.Name.ToLower();

            // Handle generic args
            if (type.GenericTypeArguments.Any())
                name = type.GenericTypeArguments.Aggregate(name, (current, t) => current + ("-" + GetSanitizedContainerName(t)));

            // Can only be alphanumeric and '-'
            name = bannedCharacters.Aggregate(name, (current, character) => current.Replace(character, '-'));

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
        public static String GetSanitizedContainerName<TValue>()
        {
            return GetSanitizedContainerName(typeof(TValue));
        }
        //===============================================================
    }
}
