
namespace TodoApi.Utilities;

public static class ObjectExtensions
{
    /// <summary>
    /// Convert any object into FormUrlEncodedContent for HTTP testing.
    /// </summary>
    public static FormUrlEncodedContent ToFormUrlEncodedContent(this object obj)
    {
        var formData = new List<KeyValuePair<string, string>>();
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            var key = property.Name;
            var value = property.GetValue(obj)?.ToString();

            if (value != null)
            {
                formData.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        return new FormUrlEncodedContent(formData);
    }
}
