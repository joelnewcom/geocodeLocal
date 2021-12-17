namespace GeoCodeLocal
{
    public interface ILineParser
    {
        Boolean IsValid(String line);
        Address Parse(String line);
    }
}