using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Actions
{
    public class ProcessAgsFile
    {
        private readonly string _agsFilePath;
        private readonly List<AgsGroupModel> _agsGroups;
        private readonly List<RuleErrorModel> _ruleErrors;
        private readonly AgsGroupModel _stdDictGroup;

        private AgsGroupModel _currentGroup;
        private int _groupCounter = 1;

        public ProcessAgsFile(string agsFilePath, List<RuleErrorModel> ruleErrors = null, List<AgsGroupModel> stdDictionary = null)
        {
            _agsFilePath = agsFilePath;

            _ruleErrors = ruleErrors;
            _agsGroups = new List<AgsGroupModel>();
            _stdDictGroup = stdDictionary?.GetGroup("DICT");
        }

        public List<AgsGroupModel> ReturnGroupModels(bool rowChecks) // Instaniate at DataAccess and pass the list in???
        {
            Process(rowChecks);
            return _agsGroups;
        }

        private void Process(bool rowChecks)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = false,
                Delimiter = ",",
                Quote = '"',
            };

            using var reader = new StreamReader(_agsFilePath);
            using var csv = new CsvReader(reader, csvConfig);
            while (csv.Read())
            {
                if (csv.Parser.RawRecord == Environment.NewLine)
                    continue;

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
                }

                if (rowChecks) // Position is important, check book to see if this is correct code model.
                    RowBasedRules.CheckRow(csv, _ruleErrors, _currentGroup);
            }

            // Catches last group
            ProcessCurrentGroup();
        }

        private void ProcessGroupRow(CsvReader csv)
        {
            if (_currentGroup is not null)
                ProcessCurrentGroup();         

            AgsGroupModel agsGroup = new() { Index = _groupCounter, Name = csv.GetField(1), GroupRow = csv.Parser.RawRow };

            if (_stdDictGroup is not null)
            {
                // why not do at the end and combine the two dictionaries
                agsGroup.ParentGroup = _stdDictGroup.GetRowsByFilter("DICT_GRP", csv.GetField(1)).AndBy("DICT_TYPE", "GROUP").ReturnFirstValue("DICT_PGRP");
            }

            _agsGroups.Add(agsGroup);

            _currentGroup = agsGroup;
            _groupCounter++;
        }

        private void ProcessHeadingRow(CsvReader csv)
        {
            AssignProperties(csv, Descriptor.HEADING);
            
            if (_stdDictGroup is not null)
                AssignStatuses(csv); // Decided to do only stdDict status assignments within ProcessAgsFile. Ags file dict group status need assigning afterwards.
        }

        private void ProcessUnitRow(CsvReader csv)
        {
            AssignProperties(csv, Descriptor.UNIT);
        }

        private void ProcessTypeRow(CsvReader csv)
        {
            AssignProperties(csv, Descriptor.TYPE);
        }

        private void ProcessDataRow(CsvReader csv)
        {
            if (_currentGroup.FirstDataRow == 0)
                _currentGroup.FirstDataRow = csv.Parser.RawRow;

            for (int i = 0; i < csv.Parser.Record.Length; i++)
            {
                _currentGroup.GetColumn(i).Data.Add(csv.Parser.Record[i]);
            }
        }

        private void AssignProperties(CsvReader csv, Descriptor descriptor)
        {
            if (_currentGroup.Columns == null)
                GenerateColumns(csv.Parser.Record.Length);

            _currentGroup.SetGroupDescriptorRowNumber(descriptor, csv.Parser.RawRow);

            for (int i = 0; i < csv.Parser.Record.Length; i++)
            {
                _currentGroup.GetColumn(i).SetColumnDescriptor(descriptor, csv.Parser.Record[i]);
            }
        }

        private void AssignStatuses(CsvReader csv)
        {
            var row = csv.Parser.Record[1..];

            for (int i = 0; i < row.Length; i++)
            {
                _currentGroup.GetColumn(row[i]).Status = ReturnStatus(row[i]);
            }
        }

        private string ReturnStatus(string field)
        {
            int stdDictHeadingIndex = _stdDictGroup.GetColumn("DICT_HDNG").Data.IndexOf(field);

            if (stdDictHeadingIndex > -1)
                return _stdDictGroup.GetColumn("DICT_STAT").Data.ElementAt(stdDictHeadingIndex);

            //  SEE call comment
            //  int fileDictHeadingIndex = _currentGroup.Columns.FirstOrDefault(c => c.Heading == "DICT_HDNG").Data.IndexOf(field);
            //  if (fileDictHeadingIndex > -1)
            //      return _currentGroup.Columns.FirstOrDefault(c => c.Heading == "DICT_STAT").Data.ElementAt(fileDictHeadingIndex);

            return string.Empty;
        }

        private void GenerateColumns(int length)
        {
            _currentGroup.Columns = new List<AgsColumnModel>();

            for (int i = 0; i < length; i++)
            {
                AgsColumnModel agsColumn = new();
                agsColumn.Index = i;
                _currentGroup.Columns.Add(agsColumn);
            }
        }

        private void ProcessCurrentGroup()
        {
            _currentGroup.DataFrame = AgsGroupModelToDataFrame.ReturnDataFrame(_currentGroup);
        }
    }
}
