using Jint;
using Jint.Native;
using Jint.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MockScript
{
    public class MockScriptParser
    {
        private readonly Engine _jsEngine;
        string GetProjectRootPath()
        {
            var basePath = AppContext.BaseDirectory;

            // Try to find the node_modules folder by walking up the directory tree
            var currentDirectory = new DirectoryInfo(basePath);

            while (currentDirectory != null && !Directory.Exists(Path.Combine(currentDirectory.FullName, "node_modules")))
            {
                currentDirectory = currentDirectory.Parent;
            }

            if (currentDirectory == null)
            {
                throw new DirectoryNotFoundException("Could not locate node_modules folder.");
            }

            return currentDirectory.FullName;
        }
        public static bool ValidateJSON(string s)
        {
            try
            {
                JToken.Parse(s);
                return true;
            }
            catch (JsonReaderException ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }
        public MockScriptParser()
        {
            _jsEngine = new Engine(options =>
            {
                options.EnableModules(Path.Combine(GetProjectRootPath(), "node_modules", "@faker-js", "faker", "dist"));  
                options.LimitRecursion(100);
                options.TimeoutInterval(TimeSpan.FromMilliseconds(10000));  // Prevent long-running scripts
                options.Strict();                                        // Disallow sloppy mode
                options.LimitMemory(300_000_000);                           // 4 MB memory cap
            });

            // Load faker
            var fakerModule = _jsEngine.Modules.Import("./index.js");
            if (fakerModule != null)
            {
                JsValue fakerValue = fakerModule.Get("faker");

                if (fakerValue != JsValue.Undefined && fakerValue.IsObject())
                {
                    _jsEngine.SetValue("faker", fakerValue);
                }
                else
                {
                    Console.WriteLine("Error: 'faker' export not found or is not an object.");
                }
            }
            else
            {
                Console.WriteLine("Error: Failed to import the faker module.");
            }

            
            // Chance.js not working properly

           /* string chancePath = Path.Combine(GetProjectRootPath(), "node_modules", "chance", "dist", "chance.min.js");
            var chanceScript = File.ReadAllText(chancePath);
            // Load chance
            chanceScript = chanceScript.Replace(
                    "typeof module === 'object' && module.exports",
                    "false"
                );

            _jsEngine.Execute(chanceScript);
            // Verify Chance is now defined
            var chanceConstructor = _jsEngine.GetValue("Chance");
            if (chanceConstructor.IsUndefined())
            {
                Console.WriteLine("Chance constructor not found.");
                return;
            }

            // Create chance instance
            _jsEngine.Execute("var chance = new Chance();");

            // Use chance
            var randomPhone = _jsEngine.Evaluate("chance.phone()").ToString();
            Console.WriteLine($"Random Phone: {randomPhone}"); */
        }

        
        private bool TryParseRepeat(string input, out int min, out int max)
        {
            var match = Regex.Match(input, @"repeat\((\d+)\s*,\s*(\d+)\)");
            if (match.Success)
            {
                min = int.Parse(match.Groups[1].Value);
                max = int.Parse(match.Groups[2].Value);
                return true;
            }

            match = Regex.Match(input, @"repeat\((\d+)\)");
            if (match.Success)
            {
                min = max = int.Parse(match.Groups[1].Value);
                return true;
            }

            min = max = 1;
            return false;
        }

        public string Parse(JToken json)
        {
            var template = new MockScriptTemplate();

            if (json.Type == JTokenType.Array)
            {
                if (json.Count() > 0 && json[0].Type == JTokenType.String && TryParseRepeat(json[0].Value<string>(), out int min, out int max))
                {
                    var repeatedItems = new List<object>();
                    var templateItem = json.Count() > 1 ? json[1] : null;
                    var count = new Random().Next(min, max + 1);

                    for (int i = 0; i < count; i++)
                    {
                        template.ArrayItems.Add(ParseNode(templateItem));
                    }
                }
            }
            else if (json.Type == JTokenType.Object)
            {
                template.RootObject = ParseObject((JObject)json);
            }
            else
            {
                throw new NotSupportedException($"Root JSON type {json.Type} is not supported. Expected an array with a single object containing '_count' or a root object.");
            }

            string outputjson;
            if (template.ArrayItems.Any())
            {
                outputjson = System.Text.Json.JsonSerializer.Serialize(template.ArrayItems, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            else if (template.RootObject != null && template.RootObject.Any())
            {
                outputjson = System.Text.Json.JsonSerializer.Serialize(template.RootObject, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            else
            {
                outputjson = "{}"; // or "null", or empty string depending on your design
            }
            return outputjson;
        }
        private bool IsPureJavaScriptExpression(string input)
        {
            try
            {
                // Wrap in a function to allow object/array literals and return statement
                _jsEngine.Evaluate($"(() => {{ return {input}; }})()");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private object EvaluateJavaScript(string script, object context = null)
        {
            var regex = new Regex(@"(?:faker|chance)\.[\w.]+\([^()]*\)");

            bool isPureJs = IsPureJavaScriptExpression(script);

            try
            {
                if (context != null)
                {
                    var jsContext = JsValue.FromObject(_jsEngine, context);
                    _jsEngine.SetValue("context", jsContext);
                }

                string result = script;
                bool hasMatch;

                do
                {
                    hasMatch = false;

                    result = regex.Replace(result, match =>
                    {
                        hasMatch = true;

                        try
                        {
                            var expression = match.Value;
                            var evalResult = _jsEngine.Evaluate(expression);

                            if (evalResult.IsString())
                            {
                                var str = evalResult.AsString().Replace("\"", "\\\"");

                                // Quote only if the result is part of JS expression
                                return isPureJs ? $"\"{str}\"" : str;
                            }

                            return evalResult.ToObject()?.ToString() ?? "";
                        }
                        catch (Jint.Runtime.JavaScriptException ex)
                        {
                            return $"JavaScript Error: {ex.Message}";
                        }
                    });

                } while (hasMatch && regex.IsMatch(result));

                // Evaluate final script if it's JS context or contains 'this'
                if (isPureJs || result.Contains("this."))
                {
                    try
                    {
                        string wrapped = context != null
                            ? $"(function(){{ return {result}; }}).call(context)"
                            : result;

                        var finalEvalResult = _jsEngine.Evaluate(wrapped);
                        return finalEvalResult.IsString()
                            ? finalEvalResult.AsString()
                            : finalEvalResult.ToObject();
                    }
                    catch (Jint.Runtime.JavaScriptException ex)
                    {
                        return $"JavaScript Error: {ex.Message}";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Evaluation Error: {ex.Message}";
            }
        }



        private object ParseNode(JToken token, JsValue context = null) // Add context parameter
        {
            return token.Type switch
            {
                JTokenType.Object => ParseObject((JObject)token),
                JTokenType.Array => ParseArray((JArray)token, context), // Pass context to array parsing as well
                JTokenType.String => EvaluateJavaScript(token.ToString(), context), // Pass context here
                JTokenType.Integer => token.ToString(),
                JTokenType.Float => token.ToString(),
                JTokenType.Boolean => token.ToString().ToLower(),
                JTokenType.Null => null,
                _ => throw new NotSupportedException($"Unsupported token type: {token.Type}")
            };
        }

        private Dictionary<string, object> ParseObject(JObject obj)
        {
            var result = new Dictionary<string, object>();
            var deferred = new List<JProperty>();

            // First pass: parse all non-this.* expressions
            foreach (var prop in obj.Properties())
            {
                var value = prop.Value;
                if (value.Type == JTokenType.String && value.ToString().Contains("this."))
                {
                    deferred.Add(prop); // Delay evaluation
                    continue;
                }

                result[prop.Name] = ParseNode(value);
            }

            // Second pass: now evaluate this.* expressions using built context
            foreach (var prop in deferred)
            {
                var script = prop.Value.ToString();
                result[prop.Name] = EvaluateJavaScript(script, result);
            }

            return result;
        }


        private List<object> ParseArray(JArray array, JsValue context = null) // Add context parameter
        {
            if (array.Count() > 0 && array[0].Type == JTokenType.String && TryParseRepeat(array[0].Value<string>(), out int min, out int max))
            {
                var templateItem = array.Count() > 1 ? array[1] : null;
                var count = new Random().Next(min, max + 1);
                var items = new List<object>();
                for (int i = 0; i < count; i++)
                {
                    items.Add(ParseNode(templateItem, context)); // Pass context to array items
                }
                return items;
            }
            else
            {
                var items = new List<object>();
                foreach (var item in array)
                {
                    items.Add(ParseNode(item, context)); // Pass context to array items
                }
                return items;
            }
        }
    }
}
