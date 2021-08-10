using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AgsVerifierWindowsGUI.Extensions
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        public EnumBindingSourceExtension(Type enumType)
        {
            if (enumType is null || enumType.IsEnum == false)
                throw new Exception("EnumType must not be null and must be of type Enum.");

            EnumType = enumType;
        }

        public Type EnumType { get; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(EnumType);
        }
    }
}
