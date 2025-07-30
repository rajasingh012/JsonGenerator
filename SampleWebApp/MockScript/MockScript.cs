using System.Text.Json.Serialization;

namespace MockScript
{
    public class MockScriptTemplate
    { 
        public List<object> ArrayItems { get; set; } = new();
        public Dictionary<string, object> RootObject { get; set; }
    } 
}
