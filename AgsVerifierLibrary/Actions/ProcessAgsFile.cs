using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Properties;
using AgsVerifierLibrary.Rules;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Actions
{
    public class ProcessAgsFile
    {
        private static readonly string[] _parentGroupExceptions = new string[] { "PROJ", "TRAN", "ABBR", "DICT", "UNIT", "TYPE", "LOCA", "FILE", "LBSG", "PREM", "STND" };
        private readonly AgsContainer _ags;
        private readonly List<RuleError> _ruleErrors;
        private readonly AgsContainer _stdDictionary;
        private AgsGroup _currentGroup;

        public ProcessAgsFile(AgsContainer ags, List<RuleError> ruleErrors, AgsContainer stdDictionary)
        {
            _ags = ags;
            _ruleErrors = ruleErrors;
            _stdDictionary = stdDictionary;
        }

        public void Process(StreamReader reader)
        {
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                IgnoreBlankLines = true,
                Delimiter = ",",
                Quote = '"',
            };

            using CsvReader csv = new(reader, csvConfig);
            while (csv.Read())
            {
                switch (csv.GetField(0))
                {
                    case "GROUP":
                        ProcessGroupRow(csv);
                        break;
                    case "HEADING":
                        ProcessHeadingRow(csv);
                        break;
                    case "UNIT":
                        ProcessUnitRow(csv);
                        break;
                    case "TYPE":
                        ProcessTypeRow(csv);
                        break;
                    case "DATA":
                        ProcessDataRow(csv);
                        break;
                    default:
                        break;
                }

                PerRowRules.CheckRow(csv, _ruleErrors, _currentGroup);
            }

            FinalAssignments();
        }

        private void ProcessGroupRow(CsvReader csv)
        {
            AgsGroup agsGroup = new() { Name = csv.GetField(1), GroupRow = csv.Parser.RawRow };

            _ags.Groups.Add(agsGroup);

            _currentGroup = agsGroup;
        }

        private void ProcessHeadingRow(CsvReader csv)
        {
            AssignProperties(csv, AgsDescriptor.HEADING);
        }

        private void ProcessUnitRow(CsvReader csv)
        {
            AssignProperties(csv, AgsDescriptor.UNIT);
        }

        private void ProcessTypeRow(CsvReader csv)
        {
            AssignProperties(csv, AgsDescriptor.TYPE);
        }

        private void ProcessDataRow(CsvReader csv)
        {
            _currentGroup["Index"].Data.Add(csv.Parser.RawRow.ToString());

            for (int i = 1; i < _currentGroup.Columns.Count; i++) // Starting at 1
            {
                try
                {
                    _currentGroup[i].Data.Add(csv.Parser.Record[i - 1]);
                }
                catch (Exception)
                {
                    // TODO add error
                    throw new IndexOutOfRangeException();
                    //_currentGroup[i].Data.Add(string.Empty);
                }
            }
        }

        private void AssignProperties(CsvReader csv, AgsDescriptor descriptor)
        {
            if (_currentGroup.Columns.Count == 0)
                GenerateColumns(csv.Parser.Record.Length);

            _currentGroup.SetGroupDescriptorRowNumber(descriptor, csv.Parser.RawRow);

            for (int i = 1; i < _currentGroup.Columns.Count; i++)
            {
                try
                {
                    _currentGroup[i].SetColumnDescriptor(descriptor, csv.Parser.Record[i - 1]);
                }
                catch (Exception)
                {
                    // TODO add error
                    throw new IndexOutOfRangeException();
                    //_currentGroup[i].SetColumnDescriptor(descriptor, string.Empty);
                }
            }
        }

        private void GenerateColumns(int length)
        {
            var indexColumn = _currentGroup.AddColumn();
            indexColumn.Heading = "Index";

            for (int i = 0; i < length; i++)
            {
                _currentGroup.AddColumn();
            }
        }

        public void FinalAssignments()
        {
            foreach (var group in _ags.Groups)
            {
                AssignParentGroup(group);
                AssignStatuses(group);
            }
        }

        private void AssignParentGroup(AgsGroup group)
        {
            if (_parentGroupExceptions.Contains(group.Name))
                return;

            string parentGroupName = _stdDictionary["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AndBy("DICT_TYPE", AgsDescriptor.GROUP).FirstOf("DICT_PGRP")
                ?? _ags["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AndBy("DICT_TYPE", AgsDescriptor.GROUP).FirstOf("DICT_PGRP")
                ?? string.Empty;

            if (parentGroupName is not "")
                group.ParentGroup = _ags[parentGroupName];
        }

        private void AssignStatuses(AgsGroup group)
        {
            foreach (var column in group.Columns)
            {
                if (column.Heading is "Index" or "HEADING")
                    continue;

                column.Status = _stdDictionary["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AndBy("DICT_HDNG", column.Heading).FirstOf("DICT_STAT")
                    ?? _ags["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AndBy("DICT_HDNG", column.Heading).FirstOf("DICT_STAT")
                    ?? string.Empty;
            }
        }
    }
}
