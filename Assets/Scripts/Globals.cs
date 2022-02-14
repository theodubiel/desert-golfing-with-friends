using System.Collections.Generic;

public class Globals
{
    public static readonly ushort PORT_MIN = 80;
    public static readonly ushort PORT_MAX = 65535;

    public static readonly ushort PASSCODE_MIN_LENGTH = 0;
    public static readonly ushort PASSCODE_MAX_LENGTH = 16;

    public static readonly ushort NAME_MIN_LENGTH = 1;
    public static readonly ushort NAME_MAX_LENGTH = 32;

    public static readonly List<char> ALLOWED_SPECIAL_CHARACTERS = new List<char>() {
        '_', ' '
    };
}
