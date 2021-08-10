using System.ComponentModel;

namespace AgsVerifierLibrary.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    // Approach taken from https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/
    public enum AgsVersion
    {
        [Description("AGS v4.0.3")]
        V403 = 0,
        [Description("AGS v4.0.4 - Default Standard Dictionary")]
        V404 = 1,
        [Description("AGS v4.1.0")]
        V410 = 2,
    }
}
