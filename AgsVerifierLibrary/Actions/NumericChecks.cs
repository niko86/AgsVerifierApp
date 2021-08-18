using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using System;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Actions
{
    public static class NumericChecks
    {
        // Using static compiled regex for performance reasons.
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

        private static readonly Regex _altOneSF = new(@"^[-+]?[0-9]{1}0*$", RegexOptions.Compiled);
        private static readonly Regex _altTwoSF = new(@"^[-+]?[0-9]{2}0*$", RegexOptions.Compiled);
        private static readonly Regex _altThreeSF = new(@"^[-+]?[0-9]{3}0*$", RegexOptions.Compiled);
        private static readonly Regex _altFourSF = new(@"^[-+]?[0-9]{4}0*$", RegexOptions.Compiled);
        private static readonly Regex _altFiveSF = new(@"^[-+]?[0-9]{5}0*$", RegexOptions.Compiled);
        private static readonly Regex _altSixSF = new(@"^[-+]?[0-9]{6}0*$", RegexOptions.Compiled);

        private static readonly Regex _oneSCI = new(@"^\d\.\d{1}E[-]?\d+$", RegexOptions.Compiled);
        private static readonly Regex _twoSCI = new(@"^\d\.\d{2}E[-]?\d+$", RegexOptions.Compiled);
        private static readonly Regex _threeSCI = new(@"^\d\.\d{3}E[-]?\d+$", RegexOptions.Compiled);
        private static readonly Regex _fourSCI = new(@"^\d\.\d{4}E[-]?\d+$", RegexOptions.Compiled);
        private static readonly Regex _fiveSCI = new(@"^\d\.\d{5}E[-]?\d+$", RegexOptions.Compiled);
        private static readonly Regex _sixSCI = new(@"^\d\.\d{6}E[-]?\d+$", RegexOptions.Compiled);

        private static readonly Regex _hhmmss = new(@"^(2[0-3]|[01]?[0-9]):([0-5]?[0-9]):([0-5]?[0-9])$", RegexOptions.Compiled);
        private static readonly Regex _hhmm = new(@"^(2[0-3]|[01]?[0-9]):([0-5]?[0-9])$", RegexOptions.Compiled);
        private static readonly Regex _mmss = new(@"^([0-5]?[0-9]):([0-5]?[0-9])$", RegexOptions.Compiled);

        private static readonly Regex _fullIsoDateTime = new(@"^([0-9]{2}|[0-9]{4})(?:-|\/)(?:(1[0-2]|0?[1-9])(?:-|\/)(3[01]|[12][0-9]|0?[1-9])|(3[01]|[12][0-9]|0?[1-9])-(1[0-2]|0?[1-9]))(?: |T)(2[0-3]|[01]?[0-9]):([0-5]?[0-9]):([0-5]?[0-9])\.(\d{3})Z\(\+(2[0-3]|[01]?[0-9]):([0-5]?[0-9])\)$", RegexOptions.Compiled);
        private static readonly Regex _dateYear = new(@"^(?:(1[0-2]|0?[1-9])(?:-|\/)(3[01]|[12][0-9]|0?[1-9])|(3[01]|[12][0-9]|0?[1-9])-(1[0-2]|0?[1-9]))(?:-|\/)([0-9]{2}|[0-9]{4})$", RegexOptions.Compiled);
        private static readonly Regex _yearDate = new(@"^([0-9]{2}|[0-9]{4})(?:-|\/)(?:(1[0-2]|0?[1-9])(?:-|\/)(3[01]|[12][0-9]|0?[1-9])|(3[01]|[12][0-9]|0?[1-9])-(1[0-2]|0?[1-9]))$", RegexOptions.Compiled);
        private static readonly Regex _dateYearTime = new(@"^(?:(1[0-2]|0?[1-9])(?:-|\/)(3[01]|[12][0-9]|0?[1-9])|(3[01]|[12][0-9]|0?[1-9])-(1[0-2]|0?[1-9]))(?:-|\/)([0-9]{2}|[0-9]{4})(?: |T)(2[0-3]|[01]?[0-9]):([0-5]?[0-9]):([0-5]?[0-9])$", RegexOptions.Compiled);
        private static readonly Regex _yearDateTime = new(@"^([0-9]{2}|[0-9]{4})(?:-|\/)(?:(1[0-2]|0?[1-9])(?:-|\/)(3[01]|[12][0-9]|0?[1-9])|(3[01]|[12][0-9]|0?[1-9])-(1[0-2]|0?[1-9]))(?: |T)(2[0-3]|[01]?[0-9]):([0-5]?[0-9]):([0-5]?[0-9])$", RegexOptions.Compiled);


        public static bool NumericTypeIsValid(AgsColumn column, string value)
        {

            if (column.Type == AgsDataType.DT.ToString())
            {
                return DateTimeCheck(column.Unit, value);
            }
            else if (column.Type == AgsDataType.MC.ToString())
            {
                return MoistureContentCheck(value);
            }
            else if (column.Type == AgsDataType.T.ToString())
            {
                return ElapsedTimeCheck(column.Unit, value);
            }
            else if (column.Type[^2..] == AgsDataType.DP.ToString())
            {
                return DecimalPointCheck(column.Type, value);
            }
            else if (column.Type[^3..] == AgsDataType.SCI.ToString())
            {
                return ScientificNotationCheck(column.Type, value);
            }
            else if (column.Type[^2..] == AgsDataType.SF.ToString())
            {
                return SignificantFigureCheck(column.Type, value);
            }

            return false;
        }

        private static bool ElapsedTimeCheck(string unit, string value)
        {
            return unit switch
            {
                "hh:mm:ss" => _hhmmss.IsMatch(value),
                "hh:mm" => _hhmm.IsMatch(value),
                "mm:ss" => _mmss.IsMatch(value),
                _ => throw new NotImplementedException("Unrecognised time duration string format.")
            };
        }

        private static bool DateTimeCheck(string unit, string value)
        {
            return unit switch
            {
                "yyyy-mm-ddThh:mm:ss.sssZ(+hh:mm)" => _fullIsoDateTime.IsMatch(value),
                "yyyy/mm/ddThh:mm:ss.sssZ(+hh:mm)" => _fullIsoDateTime.IsMatch(value),

                "yyyy-mm-ddThh:mm:ss" => _yearDateTime.IsMatch(value),
                "yyyy/mm/ddThh:mm:ss" => _yearDateTime.IsMatch(value),

                "dd-mm-yyyyThh:mm:ss" => _dateYearTime.IsMatch(value),
                "mm-dd-yyyyThh:mm:ss" => _dateYearTime.IsMatch(value),
                "dd/mm/yyyyThh:mm:ss" => _dateYearTime.IsMatch(value),
                "mm/dd/yyyyThh:mm:ss" => _dateYearTime.IsMatch(value),

                "yyyy-mm-dd hh:mm:ss" => _yearDateTime.IsMatch(value),
                "yyyy/mm/dd hh:mm:ss" => _yearDateTime.IsMatch(value),

                "dd-mm-yyyy hh:mm:ss" => _dateYearTime.IsMatch(value),
                "mm-dd-yyyy hh:mm:ss" => _dateYearTime.IsMatch(value),
                "dd/mm/yyyy hh:mm:ss" => _dateYearTime.IsMatch(value),
                "mm/dd/yyyy hh:mm:ss" => _dateYearTime.IsMatch(value),

                "yyyy-mm-dd" => _yearDate.IsMatch(value),
                "yyyy/mm/dd" => _yearDate.IsMatch(value),


                "dd-mm-yyyy" => _dateYear.IsMatch(value),
                "mm-dd-yyyy" => _dateYear.IsMatch(value),
                "dd/mm/yyyy" => _dateYear.IsMatch(value),
                "mm/dd/yyyy" => _dateYear.IsMatch(value),

                "hh:mm:ss" => _hhmmss.IsMatch(value),
                _ => throw new NotImplementedException("Unrecognised date time string format.")
            };
        }

        private static bool MoistureContentCheck(string value)
        {
            return _zeroDP.IsMatch(value) || _twoSF.IsMatch(value);
        }

        private static bool DecimalPointCheck(string type, string value)
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

        private static bool ScientificNotationCheck(string type, string value)
        {
            int precision = char.IsDigit(type[0]) ? int.Parse(type[0].ToString()) : -1;

            return precision switch
            {
                1 => _oneSCI.IsMatch(value),
                2 => _twoSCI.IsMatch(value),
                3 => _threeSCI.IsMatch(value),
                4 => _fourSCI.IsMatch(value),
                5 => _fiveSCI.IsMatch(value),
                6 => _sixSCI.IsMatch(value),
                _ => throw new NotImplementedException("Unsupported scientific notation figures.")
            };
        }

        private static bool SignificantFigureCheck(string type, string value)
        {
            int precision = char.IsDigit(type[0]) ? int.Parse(type[0].ToString()) : -1;

            return precision switch
            {
                // Need a fallback for the failures until make a regex pattern to account for trailing zeros before decimal point.
                1 => _oneSF.IsMatch(value) || _altOneSF.IsMatch(value), // double bar if left is true right isnt computed.
                2 => _twoSF.IsMatch(value) || _altTwoSF.IsMatch(value),
                3 => _threeSF.IsMatch(value) || _altThreeSF.IsMatch(value),
                4 => _fourSF.IsMatch(value) || _altFourSF.IsMatch(value),
                5 => _fiveSF.IsMatch(value) || _altFiveSF.IsMatch(value),
                6 => _sixSF.IsMatch(value) || _altSixSF.IsMatch(value),
                _ => throw new NotImplementedException("Unsupported number of significant figures.")
            };
        }
    }
}
