//#define ENABLE_ENCRYPTION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM
{
    public static class FConnFileHelper
    {

        public enum FConnFileFormat : byte
        {
            UNKNOWN = 0x00,
            CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP = 0x01,
            CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP = 0x02,
            CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM = 0x03,
        }

        public enum FConnFileState : ushort
        {
            UNKNOWN = 0x0000,

            CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP_EDITABLE = 0x0100,
            CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP_READONLY = 0x0101,

            CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP_EDITABLE = 0x0200,
            CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP_READONLY = 0x0201,

            CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM_FIRST_OPEN = 0x0300,
            CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM_OPEN = 0x0301,
            CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM_MODIFIED = 0x0302,
            CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM_COMPLETE = 0x0303
        }

        public static FConnFileFormat ResolveFileFormat(byte FormatCode)
        {
            if (typeof(FConnFileFormat).IsEnumDefined(FormatCode))
                return (FConnFileFormat)FormatCode;
            else
                return FConnFileFormat.UNKNOWN;
        }

        public static FConnFileState ResolveFileState(FConnFileFormat FormatCode, byte State)
        {
            if (FormatCode == FConnFileFormat.UNKNOWN) return FConnFileState.UNKNOWN;

            ushort _FConnFileState = 0x0000;
            _FConnFileState |= (byte)FormatCode;
            _FConnFileState = (byte)(_FConnFileState << 8);
            _FConnFileState |= State;

            if (typeof(FConnFileState).IsEnumDefined(_FConnFileState))
                return (FConnFileState)_FConnFileState;
            else
                return FConnFileState.UNKNOWN;
        }

        private static decimal MIN_SUPPORTED_VERSION(FConnFileFormat Format)
        {
            switch (Format)
            {
                case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP:
                    return 3.0M;
                case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP:
                    return 3.0M;
                case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM:
                    return 3.0M;
                default:
                    return 0M;
            }
        }

        public static int MIN_SUPPORTED_MAJOR_VERSION(FConnFileFormat Format)
        {
            return (int)Math.Truncate(MIN_SUPPORTED_VERSION(Format));
        }

        public static int MIN_SUPPORTED_MINOR_VERSION(FConnFileFormat Format)
        {
            return (int)(MIN_SUPPORTED_VERSION(Format) - Math.Truncate(MIN_SUPPORTED_VERSION(Format)));
        }

        private static decimal CURRENT_VERSION(FConnFileFormat Format)
        {
            switch (Format)
            {
                case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP:
                    return 3.0M;
                case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP:
                    return 3.0M;
                case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM:
                    return 3.0M;
                default:
                    return 0M;
            }
        }

        public static int CURRENT_MAJOR_VERSION(FConnFileFormat Format)
        {
            return (int) Math.Truncate(CURRENT_VERSION(Format));
        }

        public static int CURRENT_MINOR_VERSION(FConnFileFormat Format)
        {
            return (int) (CURRENT_VERSION(Format) - Math.Truncate(CURRENT_VERSION(Format)));
        }
    }
}
