using TodoApi.Utilities;

namespace TodoApi.Tests.Utilities;

public class ObjectExtensionTest
{
    public class TestObject
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    [Fact]
    public void ToFormUrlEncodedContent_ReturnsExpectedContent()
    {
        // Arrange
        var obj = new TestObject
        {
            Name = "John",
            Age = 30
        };

        var expectedContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Name", "John"),
            new KeyValuePair<string, string>("Age", "30")
        });

        // Act
        var result = obj.ToFormUrlEncodedContent();

        // Assert
        Assert.Equal(expectedContent.ReadAsStringAsync().Result, result.ReadAsStringAsync().Result);
    }
}
