using AgsVerifierLibrary.Enums;
using System;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Actions
{
    public static class NumericChecks
    {
        private static readonly Regex _zeroDP = new(@"^\d+$", RegexOptions.Compiled);
        private static readonly Regex _oneDP = new(@"^\d+\.\d{1}$", RegexOptions.Compiled);
        private static readonly Regex _twoDP = new(@"^\d+\.\d{2}$", RegexOptions.Compiled);
        private static readonly Regex _threeDP = new(@"^\d+\.\d{3}$", RegexOptions.Compiled);
        private static readonly Regex _fourDP = new(@"^\d+\.\d{4}$", RegexOptions.Compiled);
        private static readonly Regex _fiveDP = new(@"^\d+\.\d{5}$", RegexOptions.Compiled);
        private static readonly Regex _sixDP = new(@"^\d+\.\d{6}$", RegexOptions.Compiled);

        // Below regex from https://stackoverflow.com/q/26231423/3653306
        private static readonly Regex _oneSF = new(@"^(?!(?:.*[1-9](\.?[0-9]){1,}))([-+]?\d+\.?\d*?)$", RegexOptions.Compiled);
        private static readonly Regex _twoSF = new(@"^(?!(?:.*[1-9](\.?[0-9]){2,}))([-+]?\d+\.?\d*?)$", RegexOptions.Compiled);
        private static readonly Regex _threeSF = new(@"^(?!(?:.*[1-9](\.?[0-9]){3,}))([-+]?\d+\.?\d*?)$", RegexOptions.Compiled);
        private static readonly Regex _fourSF = new(@"^(?!(?:.*[1-9](\.?[0-9]){4,}))([-+]?\d+\.?\d*?)$", RegexOptions.Compiled);
        private static readonly Regex _fiveSF = new(@"^(?!(?:.*[1-9](\.?[0-9]){5,}))([-+]?\d+\.?\d*?)$", RegexOptions.Compiled);
        private static readonly Regex _sixSF = new(@"^(?!(?:.*[1-9](\.?[0-9]){6,}))([-+]?\d+\.?\d*?)$", RegexOptions.Compiled);

        
        public static bool NumericTypeIsValid(string type, string value)
        {
            if (type == AgsDataType.MC.ToString())
            {
                return MoistureContent(value);
            }
            else if (type[^2..] == AgsDataType.DP.ToString())
            {
                return DecimalPoints(type, value);
            }
            else if (type[^2..] == AgsDataType.SF.ToString())
            {
                return SignificantFigures(type, value);
            }

            return false;
        }

        private static bool MoistureContent(string value)
        {
            return _zeroDP.IsMatch(value) || _twoSF.IsMatch(value);
        }

        private static bool DecimalPoints(string type, string value)
        {
            int precision = char.IsDigit(type[0]) ? int.Parse(type[0].ToString()) : -1;

            return precision switch
            {
                0 => _zeroDP.IsMatch(value),
                1 => _oneDP.IsMatch(value),
                2 => _twoDP.IsMatch(value),
                3 => _threeDP.IsMatch(value),
                4 => _fourDP.IsMatch(value),
                5 => _fiveDP.IsMatch(value),
                6 => _sixDP.IsMatch(value),
                _ => throw new NotImplementedException("Unsupported number of decimal places.")
            };
        }

        private static bool SignificantFigures(string type, string value)
        {
            int precision = char.IsDigit(type[0]) ? int.Parse(type[0].ToString()) : -1;

            return precision switch
            {
                1 => _oneSF.IsMatch(value),
                2 => _twoSF.IsMatch(value),
                3 => _threeSF.IsMatch(value),
                4 => _fourSF.IsMatch(value),
                5 => _fiveSF.IsMatch(value),
                6 => _sixSF.IsMatch(value),
                _ => throw new NotImplementedException("Unsupported number of significant figures.")
            };
        }
    }
}
