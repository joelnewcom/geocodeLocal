internal class NominatinResponse
{
    public int place_id { get; set; }
    public String licence { get; set; }
    public String osm_type { get; set; }
    public int osm_id { get; set; }

    // boundingbox

    public String lat { get; set; }
    public String lon { get; set; }
    public String display_name { get; set; }
    public int place_rank { get; set; }
    public String category { get; set; }
    public String type { get; set; }
    public Decimal importance { get; set; }
}