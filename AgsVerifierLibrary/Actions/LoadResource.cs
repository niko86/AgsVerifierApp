using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Properties;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace AgsVerifierLibrary.Actions
{
    public static class LoadResource
    {
        public static List<AgsGroup> StdDictionary(AgsVersion version)
        {
            using StreamReader sr = new(new MemoryStream(buffer: (byte[])Resources.ResourceManager.GetObject(version.ToString())));
            JsonSerializer serializer = new();
            List<AgsGroup> groups = (List<AgsGroup>)serializer.Deserialize(sr, typeof(List<AgsGroup>));
            foreach (AgsGroup group in groups)
            {
                foreach (AgsColumn column in group.Columns)
                {
                    column.Group = group;
                }
            }
            return groups;
        }

    }
}
