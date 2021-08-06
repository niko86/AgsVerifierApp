using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Actions
{
    public static class HelperFunctions
    {
        public static IEnumerable<dynamic> MergedDictColumnByStatus(AgsContainer stdDictionary, AgsContainer ags, Status status, string groupName, string columnName)
        {
            var stdDictKeyHeadings = stdDictionary["DICT"]["DICT_GRP"].FilterRowsBy(groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            var fileDictKeyHeadings = ags["DICT"]["DICT_GRP"].FilterRowsBy(groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            return stdDictKeyHeadings.Concat(fileDictKeyHeadings).Distinct();
        }
    }
}
