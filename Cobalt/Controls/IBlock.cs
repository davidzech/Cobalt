namespace Cobalt.Controls
{
    public interface IBlock
    {
        MessageLine Source { get; }
        string TimeString { get; }
        string NickString { get; }
        int CharStart { get; }
        int CharEnd { get; }
        double Y { get;}
        double NickX { get; }
        double TextX { get; }
        double Height { get; }        
        double TimeWidth { get; }
        double NickWidth { get; }
        int NumLines { get; }
    }
}