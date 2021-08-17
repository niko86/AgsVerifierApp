using AgsVerifierLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private static readonly Regex _oneSF = new(@"^-?(?=\d{1}(?:[.,]0+)?0*$|(?:(?=.{1,2}0*$)(?:\d+[.,]\d+))).+$", RegexOptions.Compiled);
        private static readonly Regex _twoSF = new(@"^-?(?=\d{2}(?:[.,]0+)?0*$|(?:(?=.{2,3}0*$)(?:\d+[.,]\d+))).+$", RegexOptions.Compiled);
        private static readonly Regex _threeSF = new(@"^-?(?=\d{3}(?:[.,]0+)?0*$|(?:(?=.{3,4}0*$)(?:\d+[.,]\d+))).+$", RegexOptions.Compiled);
        private static readonly Regex _fourSF = new(@"^-?(?=\d{4}(?:[.,]0+)?0*$|(?:(?=.{4,5}0*$)(?:\d+[.,]\d+))).+$", RegexOptions.Compiled);
        private static readonly Regex _fiveSF = new(@"^-?(?=\d{5}(?:[.,]0+)?0*$|(?:(?=.{5,6}0*$)(?:\d+[.,]\d+))).+$", RegexOptions.Compiled);
        private static readonly Regex _sixSF = new(@"^-?(?=\d{6}(?:[.,]0+)?0*$|(?:(?=.{6,7}0*$)(?:\d+[.,]\d+))).+$", RegexOptions.Compiled);

        
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
                3 => _threeDP.IsMatch(value),
                4 => _threeSF.IsMatch(value),
                5 => _fiveSF.IsMatch(value),
                6 => _sixSF.IsMatch(value),
                _ => throw new NotImplementedException("Unsupported number of significant figures.")
            };
        }
    }
}
