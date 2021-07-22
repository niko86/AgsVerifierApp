using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Actions
{
    public class ProcessAgsFile
    {
        private readonly string _agsFilePath;
        private readonly DataFrame _stdDictGroup;
        private readonly List<RuleErrorModel> _ruleErrors;
        private readonly List<AgsGroupModel> _agsGroups;

        private AgsGroupModel _currentGroup;
        
        public ProcessAgsFile(string agsFilePath, DataFrame stdDictGroup = null)
        {
            _agsFilePath = agsFilePath;
            _stdDictGroup = stdDictGroup;

            _ruleErrors = new List<RuleErrorModel>();
            _agsGroups = new List<AgsGroupModel>();
        }

        public List<AgsGroupModel> ReturnGroupModels(bool rowChecks)
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

                if (csv.Parser.RawRecord == Environment.NewLine)
                    ProcessNewlineDivision();

                if (rowChecks) // Position is important, check book to see if this is correct code model.
                    RowBasedRules.CheckRow(csv, _ruleErrors, _currentGroup, _stdDictGroup);
            }
        }
        private void ProcessGroupRow(CsvReader csv)
        {
            AgsGroupModel agsGroup = new() { Name = csv.GetField(1) };
            _agsGroups.Add(agsGroup);

            _currentGroup = agsGroup;
        }

        private void ProcessHeadingRow(CsvReader csv)
        {
            for (int i = 0; i < csv.Parser.Record.Length; i++)
            {
                AGSColumn agsColumn = new() { Index = i, Heading = csv.Parser.Record[i] };
                _currentGroup.Columns.Add(agsColumn);
            }
        }

        private void ProcessUnitRow(CsvReader csv)
        {
            for (int i = 0; i < _currentGroup.Columns.Count; i++)
            {
                var heading = _currentGroup.Columns.FirstOrDefault(c => c.Index == i);
                heading.Unit = csv.Parser.Record[i];
            }
        }

        private void ProcessTypeRow(CsvReader csv)
        {
            for (int i = 0; i < _currentGroup.Columns.Count; i++)
            {
                var heading = _currentGroup.Columns.FirstOrDefault(c => c.Index == i);
                heading.Type = csv.Parser.Record[i];
            }
        }

        private void ProcessDataRow(CsvReader csv)
        {
            for (int i = 0; i < _currentGroup.Columns.Count; i++)
            {
                var heading = _currentGroup.Columns.FirstOrDefault(c => c.Index == i);
                heading.Data.Add(csv.Parser.Record[i]);
            }
        }

        private void ProcessNewlineDivision()
        {
            _currentGroup.DataFrame = AgsGroupModelToDataFrame.ReturnDataFrame(_currentGroup);
        }
    }
}
