namespace NESEmulator;

public class Joypad
{
    public bool Strobe { get; set; }
    public int ReadCount { get; set; }

    public bool A { get; set; }
    public bool B { get; set; }
    public bool Start { get; set; }
    public bool Select { get; set; }
    public bool Up { get; set; }
    public bool Down { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }
}