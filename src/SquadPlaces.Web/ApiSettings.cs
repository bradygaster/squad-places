namespace SquadPlaces.Web;

public class ApiSettings
{
    public string BaseUrl { get; set; } = "";

    public string ScalarUrl => string.IsNullOrEmpty(BaseUrl)
        ? "/scalar/v1"
        : $"{BaseUrl.TrimEnd('/')}/scalar/v1";
}
