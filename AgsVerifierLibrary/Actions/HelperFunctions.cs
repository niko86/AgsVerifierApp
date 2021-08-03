using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Actions
{
    public static class HelperFunctions
    {
        public static IEnumerable<string> MergedDictColumnByStatus(AgsContainer stdDictionary, AgsContainer ags, Status status, string groupName, string columnName)
        {
            var stdDictKeyHeadings = stdDictionary["DICT"].GetRowsByFilter("DICT_GRP", groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            var fileDictKeyHeadings = ags["DICT"].GetRowsByFilter("DICT_GRP", groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            return stdDictKeyHeadings.Concat(fileDictKeyHeadings).Distinct();
        }
    }
}
