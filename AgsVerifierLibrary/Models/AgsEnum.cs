using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Models
{
    public class AgsEnum
    {
        public enum Descriptor
        {
            GROUP = 0,
            HEADING = 1,
            UNIT = 2,
            TYPE = 3,
            DATA = 4,
        }

        public enum Status
        {
            KEY = 0,
            REQUIRED = 1,
            [Display(Name = "KEY+REQUIRED")]
            KEYPLUSREQUIRED = 2,
            OTHER = 3,
        }

        public enum DataType
        {
            /// <summary>Unique Identifier.</summary>
            [Description]
            ID = 0,
            /// <summary>Text listed in ABBR Group.</summary>
            [Description]
            PA = 1,
            /// <summary>Text listed in TYPE Group.</summary>
            [Description]
            PT = 2,
            /// <summary>Text listed in UNIT Group.</summary>
            [Description]
            PU = 3,
            /// <summary>Text.</summary>
            [Description]
            X = 4,
            /// <summary>Text / numeric.</summary>
            [Description]
            XN = 5,
            /// <summary>Elapsed Time.</summary>
            [Description]
            T = 6,
            /// <summary>Date time in international format.</summary>
            [Description]
            DT = 7,
            /// <summary>British Standard BS1377 : Part 2 reported moisture content.</summary>
            [Description]
            MC = 8,
            /// <summary>Value with required number of decimal places.</summary>
            [Description]
            DP = 9,
            /// <summary>Value with required number of significant figures.</summary>
            [Description]
            SF = 10,
            /// <summary>Scientific Notation with required number of decimal places.</summary>
            [Description]
            SCI = 11,
            /// <summary>Value with a variable format.</summary>
            [Description]
            U = 12,
            /// <summary>Degrees:Minutes:Seconds.</summary>
            [Description]
            DMS = 13,
            /// <summary>Yes or No.</summary>
            [Description]
            YN = 14,
            /// <summary>Record Link.</summary>
            [Description]
            RL = 15,
        }
    }
}
