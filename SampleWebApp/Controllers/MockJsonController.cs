using Microsoft.AspNetCore.Mvc;
using MockScript;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 
[ApiController]
[Route("u")]
public class MockJsonController : ControllerBase
{
    private readonly MockSchemaService _service; 
    public MockJsonController(MockSchemaService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJson(string id)
    {
        var schema = await _service.GetSchemaAsync(id);
        if (schema == null)
            return NotFound(new { error = "Not found" });
        var parser = MockScriptParserPool.Rent();
        if (!MockScript.MockScriptParser.ValidateJSON(schema))
        {
            return BadRequest(new { error = "Invalid JSON schema." });
        }

        var token = JsonConvert.DeserializeObject<JToken>(schema);
    
        var outputJson = parser.Parse(token);

        return Content(outputJson, "application/json");
    }

}
