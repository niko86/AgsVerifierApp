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
        private readonly AgsContainer _ags;
        private readonly List<RuleError> _ruleErrors;
        private readonly AgsContainer _stdDictionary;
        private AgsGroup _currentGroup;

        public ProcessAgsFile(AgsContainer ags, List<RuleError> ruleErrors = null, AgsContainer stdDictionary = null)
        {
            _ags = ags;
            _ruleErrors = ruleErrors;
            _stdDictionary = stdDictionary;
        }

        public void Process()
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = false,
                Delimiter = ",",
                Quote = '"',
            };

            using var reader = new StreamReader(_ags.FilePath);
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

                if (_stdDictionary is not null) 
                    RowBasedRules.CheckRow(csv, _ruleErrors, _currentGroup);
            }

            // Last Actions
            // TODO
        }

        private void ProcessGroupRow(CsvReader csv)
        {
            AgsGroup agsGroup = new() { Name = csv.GetField(1), GroupRow = csv.Parser.RawRow };

            // why not do at the end and combine the two dictionaries. MOVE TO OWN METHOD AND RUN AFTER WHILE LOOP.
            if (_stdDictionary is not null)
            {
                agsGroup.ParentGroup = _stdDictionary["DICT"].GetRowsByFilter("DICT_GRP", csv.GetField(1)).AndBy("DICT_TYPE", "GROUP").ReturnFirstValueOf("DICT_PGRP");
            }

            _ags.Groups.Add(agsGroup);

            _currentGroup = agsGroup;
        }

        private void SetParentGroups()
        {

        }

        private void ProcessHeadingRow(CsvReader csv)
        {
            AssignProperties(csv, Descriptor.HEADING);
            
            if (_stdDictionary is not null)
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

            // Adding index
            //_currentGroup[0].Data.Add(csv.Parser.RawRow.ToString());

            //for (int i = 1; i <= csv.Parser.Record[1..].Length; i++)
            for (int i = 0; i < _currentGroup.Columns.Count; i++)
            {
                try
                {
                    _currentGroup[i].Data.Add(csv.Parser.Record[i]);
                }
                catch (Exception)
                {
                    // TODO add error
                    _currentGroup[i].Data.Add(string.Empty);
                }
            }
        }

        private void AssignProperties(CsvReader csv, Descriptor descriptor)
        {
            if (_currentGroup.Columns == null)
                GenerateColumns(csv.Parser.Record.Length);

            _currentGroup.SetGroupDescriptorRowNumber(descriptor, csv.Parser.RawRow);

            for (int i = 0; i < _currentGroup.Columns.Count; i++)
            {
                try
                {
                    _currentGroup[i].SetColumnDescriptor(descriptor, csv.Parser.Record[i]);
                }
                catch (IndexOutOfRangeException)
                {
                    // TODO add error
                    _currentGroup[i].SetColumnDescriptor(descriptor, string.Empty);
                }
            }
        }

        private void AssignStatuses(CsvReader csv)
        {
            var row = csv.Parser.Record[1..];

            for (int i = 0; i < row.Length; i++)
            {
                _currentGroup[row[i]].Status = ReturnStatus(row[i]);
            }
        }

        private string ReturnStatus(string field)
        {
            // TODO check current dictionary in file at end.
            int stdDictHeadingIndex = _stdDictionary["DICT"]["DICT_HDNG"].Data.IndexOf(field);

            if (stdDictHeadingIndex > -1)
                return _stdDictionary["DICT"]["DICT_STAT"].Data.ElementAt(stdDictHeadingIndex);

            return string.Empty;
        }

        private void GenerateColumns(int length)
        {
            _currentGroup.Columns = new List<AgsColumn>();

            for (int i = 0; i < length; i++)
            {
                AgsColumn agsColumn = new()
                {
                    PartOfGroup = _currentGroup.Name
                };

                _currentGroup.Columns.Add(agsColumn);
            }
        }
    }
}
