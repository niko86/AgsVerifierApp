using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Actions
{
    public static class HelperFunctions
    {
        public static IEnumerable<string> MergedDictColumnByStatus(List<AgsGroupModel> stdDictionary, List<AgsGroupModel> groups, string status, string groupName, string columnName)
        {
            var stdDictKeyHeadings = stdDictionary.GetGroup("DICT").GetRowsByFilter("DICT_GRP", groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            var fileDictKeyHeadings = groups.GetGroup("DICT").GetRowsByFilter("DICT_GRP", groupName).AndBy("DICT_STAT", status).ReturnAllValuesOf(columnName);
            return stdDictKeyHeadings.Concat(fileDictKeyHeadings).Distinct();
        }

        public static IEnumerable<string> GetLinkedGroupKeyRows(List<AgsGroupModel> groups, string groupName, string delimiter)
        {
            return groups.GetGroup(groupName).Columns.ByStatus(Status.KEY).OrderBy(i => i.Index).ReturnRows(delimiter);
        }

        public static void ParseRecordLinkDataField(List<AgsGroupModel> groups, AgsGroupModel currentGroup, string dataField, string delimiter, string concatenator)
        {
            List<string> recordLinks = new();

            if (dataField.Contains(delimiter) == false)
            {

            }

            if (dataField.Contains(concatenator))
                recordLinks.AddRange(dataField.Split(concatenator));
            else
                recordLinks.Add(dataField);

            foreach (var recordLink in recordLinks)
            {
                string linkedGroupName = recordLink.Split(delimiter).First();

                var linkedGroupKeyRows = GetLinkedGroupKeyRows(groups, linkedGroupName, delimiter);
            }
        }
    }
}
