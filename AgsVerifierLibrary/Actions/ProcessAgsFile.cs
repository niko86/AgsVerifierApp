using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AgsVerifierLibrary.Actions
{
    public class ProcessAgsFile
    {
        private readonly string _agsFilePath;
        private readonly List<RuleErrorModel> _ruleErrors;
        private readonly List<AgsGroupModel> _agsGroups;
        private static readonly Type _groupType = typeof(AgsGroupModel);
        private static readonly Type _columnType = typeof(AGSColumnModel);

        private AgsGroupModel _currentGroup;
        private int _groupCounter = 1;

        public ProcessAgsFile(string agsFilePath, List<RuleErrorModel> ruleErrors = null)
        {
            _agsFilePath = agsFilePath;

            _ruleErrors = ruleErrors;
            _agsGroups = new List<AgsGroupModel>();
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
            _agsGroups.Add(agsGroup);

            _currentGroup = agsGroup;
            _groupCounter++;
        }

        private void ProcessHeadingRow(CsvReader csv)
        {
            AssignProperties(csv, "Heading");
        }

        private void ProcessUnitRow(CsvReader csv)
        {
            AssignProperties(csv, "Unit");
        }

        private void ProcessTypeRow(CsvReader csv)
        {
            AssignProperties(csv, "Type");
        }

        private void ProcessDataRow(CsvReader csv)
        {
            if (_currentGroup.FirstDataRow == 0)
                _currentGroup.FirstDataRow = csv.Parser.RawRow;

            for (int i = 0; i < csv.Parser.Record.Length; i++)
            {
                var column = _currentGroup.Columns.FirstOrDefault(c => c.Index == i);
                column.Data.Add(csv.Parser.Record[i]);
            }
        }

        private void AssignProperties(CsvReader csv, string agsField)
        {
            if (_currentGroup.Columns == null)
                GenerateColumns(csv.Parser.Record.Length);

            _groupType.GetProperty(agsField + "Row").SetValue(_currentGroup, csv.Parser.RawRow, null);

            for (int i = 0; i < csv.Parser.Record.Length; i++)
            {
                var column = _currentGroup.Columns.FirstOrDefault(c => c.Index == i);
                _columnType.GetProperty(agsField).SetValue(column, csv.Parser.Record[i], null);
            }
        }

        private void GenerateColumns(int length)
        {
            _currentGroup.Columns = new List<AGSColumnModel>();

            for (int i = 0; i < length; i++)
            {
                AGSColumnModel agsColumn = new();
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
