using System;

namespace AgsVerifierLibrary.Enums
{
    public static class EnumTools
    {
        // Approach taken from https://youtu.be/BoE5Y6Xkm6w and performance on large test file went
        // from ~7.5 seconds to about 5.5 seconds. Even quicker on second file run.
        public static string FastStr(AgsDataType dataType)
        {
            switch (dataType)
            {
                case AgsDataType.ID:
                    return nameof(AgsDataType.ID);
                case AgsDataType.PA:
                    return nameof(AgsDataType.PA);
                case AgsDataType.PT:
                    return nameof(AgsDataType.PT);
                case AgsDataType.PU:
                    return nameof(AgsDataType.PU);
                case AgsDataType.X:
                    return nameof(AgsDataType.X);
                case AgsDataType.XN:
                    return nameof(AgsDataType.XN);
                case AgsDataType.T:
                    return nameof(AgsDataType.T);
                case AgsDataType.DT:
                    return nameof(AgsDataType.DT);
                case AgsDataType.MC:
                    return nameof(AgsDataType.MC);
                case AgsDataType.DP:
                    return nameof(AgsDataType.DP);
                case AgsDataType.SF:
                    return nameof(AgsDataType.SF);
                case AgsDataType.SCI:
                    return nameof(AgsDataType.SCI);
                case AgsDataType.U:
                    return nameof(AgsDataType.U);
                case AgsDataType.DMS:
                    return nameof(AgsDataType.DMS);
                case AgsDataType.YN:
                    return nameof(AgsDataType.YN);
                case AgsDataType.RL:
                    return nameof(AgsDataType.RL);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }

        public static string FastStr(AgsDescriptor descriptor)
        {
            switch (descriptor)
            {
                case AgsDescriptor.GROUP:
                    return nameof(AgsDescriptor.GROUP);
                case AgsDescriptor.HEADING:
                    return nameof(AgsDescriptor.HEADING);
                case AgsDescriptor.UNIT:
                    return nameof(AgsDescriptor.UNIT);
                case AgsDescriptor.TYPE:
                    return nameof(AgsDescriptor.TYPE);
                case AgsDescriptor.DATA:
                    return nameof(AgsDescriptor.DATA);
                default:
                    throw new ArgumentOutOfRangeException(nameof(descriptor), descriptor, null);
            }
        }

        public static string FastStr(AgsStatus status)
        {
            switch (status)
            {
                case AgsStatus.KEY:
                    return nameof(AgsStatus.KEY);
                case AgsStatus.REQUIRED:
                    return nameof(AgsStatus.REQUIRED);
                case AgsStatus.KEYPLUSREQUIRED:
                    return "KEY+REQUIRED";
                case AgsStatus.OTHER:
                    return nameof(AgsStatus.OTHER);
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static string FastStr(AgsVersion version)
        {
            switch (version)
            {
                case AgsVersion.V403:
                    return nameof(AgsVersion.V403);
                case AgsVersion.V404:
                    return nameof(AgsVersion.V404);
                case AgsVersion.V410:
                    return nameof(AgsVersion.V410);
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }
    }
}
