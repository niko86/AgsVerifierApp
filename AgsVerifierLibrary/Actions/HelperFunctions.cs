using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Actions
{
    public static class HelperFunctions
    {
        public static IEnumerable<string> MergedDictColumnByStatus(List<AgsGroupModel> stdDictionary, List<AgsGroupModel> groups, Status status, string groupName, string columnName)
        {
            var stdDictKeyHeadings = stdDictionary.GetGroup("DICT").GetRowsByFilter("DICT_GRP", groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            var fileDictKeyHeadings = groups.GetGroup("DICT").GetRowsByFilter("DICT_GRP", groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            return stdDictKeyHeadings.Concat(fileDictKeyHeadings).Distinct();
        }
    }
}
