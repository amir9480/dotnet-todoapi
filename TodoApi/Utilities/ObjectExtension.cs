namespace TodoApi.Utilities;

public static class ObjectExtensions
{
    /// <summary>
    /// Convert any object into FormUrlEncodedContent for HTTP testing.
    /// </summary>
    public static FormUrlEncodedContent ToFormUrlEncodedContent(this object obj)
    {
        var properties = obj.GetType().GetProperties();

        var formData = (from property in properties
                        let key = property.Name
                        let value = property.GetValue(obj)?.ToString()
                        where value != null
                        select new KeyValuePair<string, string>(key, value)).ToList();

        return new FormUrlEncodedContent(formData);
    }
}