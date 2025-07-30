using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BlazorMonaco;
using BlazorMonaco.Editor;
using BlazorMonaco.Languages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MockScript;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 

namespace SampleWebApp.Pages;

public partial class Index
{
    private void SetSchemaTab() => activeTab = "schema";
    private void SetRawTab() => activeTab = "raw";
    private string activeTab = "schema";
    [Inject]
    public ILogger<Index> Logger { get; set; }
    private string _valueToSet = "";
    [Inject] private MockSchemaService MockSchemaService { get; set; } = default!;
    private string? ShortUrl { get; set; }
    private string? ShortUrlForRawJson { get; set; }
    [AllowNull]
    private StandaloneCodeEditor _editor, _editorRaw;
    [AllowNull]
    private StandaloneCodeEditor _editorResults;
    private bool _isDataGenerated = false; // New state variable
    private bool _codeEditorInitialized, _resultEditorInitialized, _isSharableResultProcessing, _isResultProcessing, _codeEditorRawJsonInitialized;
    private bool _showToast = false, _pendingScroll=false;
    private string? _lastSavedSchema, _lastSavedSchemaRawJson;
    private async Task GetSharableURLForRawJson(MouseEventArgs args)
    {
        if (_isSharableResultProcessing || !_resultEditorInitialized || !_codeEditorRawJsonInitialized || _editorRaw is null)
            return;
        try
        {
            _isSharableResultProcessing = true;

            var currentSchema = await _editorRaw.GetValue();
            // Don't regenerate if the schema hasn't changed
            if (Normalize(currentSchema) == Normalize(_lastSavedSchemaRawJson))
            {
                _showToast = true;   // Still show the toast if needed
                _pendingScroll = true;
                await InvokeAsync(StateHasChanged);
                return;
            }
            _lastSavedSchemaRawJson = currentSchema;
            ShortUrlForRawJson = await MockSchemaService.SaveSchemaAsync(currentSchema);
            // Update flag to show the alert box (binded section)
            _showToast = true;
            _pendingScroll = true; // mark scroll needed
            Logger.JsonLog(LogLevel.Information, "Sharable URL generated for raw json", LoggerExtensions.LogState.GenerateJson, new { ShortUrl });
            await InvokeAsync(StateHasChanged);

        }
        catch (Exception e)
        {
            Logger.JsonLogException(e, LoggerExtensions.LogState.GenerateJson);
        }
        finally
        {
            _isSharableResultProcessing = false;
        }
    }
    private async Task GetSharableURL(MouseEventArgs args)
    {
        if (_isSharableResultProcessing || !_resultEditorInitialized || !_codeEditorInitialized || _editor is null || _editorResults is null)
            return;
        try {
            _isSharableResultProcessing = true;

            var currentSchema = await _editor.GetValue(); 
            if (Normalize(currentSchema) == Normalize(_lastSavedSchema))
            {
                _showToast = true;   // Still show the toast if needed
                _pendingScroll = true;
                await InvokeAsync(StateHasChanged);
                return;
            }
            _lastSavedSchema = currentSchema;
            ShortUrl = await MockSchemaService.SaveSchemaAsync(currentSchema);
            // Update flag to show the alert box (binded section)
            _showToast = true;
            _pendingScroll = true; // mark scroll needed
            Logger.JsonLog(LogLevel.Information, "Sharable URL generated for json template", LoggerExtensions.LogState.GenerateJson, new { ShortUrl });
            await InvokeAsync(StateHasChanged);

        }
        catch (Exception e)
        {
            Logger.JsonLogException(e, LoggerExtensions.LogState.GenerateJson);
        }
        finally
        {
            _isSharableResultProcessing = false;
        }
    }
    string Normalize(string s) => s?.Trim().Replace("\r\n", "\n");

    // This is called after the render is done
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingScroll)
        {
            _pendingScroll = false;
            await jsRuntime.InvokeVoidAsync("scrollToSharableUrl");
        }
    }
    public void Dispose()
    {
        _showToast = false;
    }
    private string GetFullUrl()
    {
        return NavigationManager.BaseUri.TrimEnd('/') + "/u/" + ShortUrl;
    }
    private string GetFullUrlForRawJson()
    {
        return NavigationManager.BaseUri.TrimEnd('/') + "/u/" + ShortUrlForRawJson;
    }
    private async Task CopyToClipboard()
    {
        await jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText",  GetFullUrl());
    }
    private StandaloneEditorConstructionOptions EditorConstructionOptions_results(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            AutomaticLayout = true,
            Language = "json",
            Minimap = new EditorMinimapOptions
            {
                Enabled = false
            },
            ExtraEditorClassName = "no-workers",
        };
    }
    private static StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            AutomaticLayout = true,
            Language = "json",
            Minimap = new EditorMinimapOptions
            {
                Enabled = false
            },
            Value = """ 
                       [
                        "repeat(2,3)",
                        {
                          "message": "Hello, faker.person.firstName()! Your order number is: #faker.number.int({ min: 1, max: 100 })",
                          "phoneNumber": "faker.phone.number()",
                          "phoneVariation": "+90 faker.number.int({ min: 300, max: 399 }) faker.number.int({ min: 100, max: 999 }) faker.number.int({ min: 10, max: 99 }) faker.number.int({ min: 10, max: 99 })",
                          "status": "faker.helpers.arrayElement(['active', 'disabled'])",
                          "name": {
                            "first": "faker.person.firstName()",
                            "middle": "faker.person.middleName()",
                            "last": "faker.person.lastName()"
                          },
                          "username": "this.name.first + '-' + this.name.last",
                          "password": "faker.internet.password()",
                          "emails": [
                            "repeat(5,6)",
                            "faker.internet.email(undefined, undefined, faker.helpers.arrayElement(['gmail.com', 'example.com']))"
                          ],
                          "location": {
                            "street": "faker.location.streetAddress()",
                            "city": "faker.location.city()",
                            "state": "faker.location.state()",
                            "country": "faker.location.country()",
                            "zip": "faker.location.zipCode()",
                            "coordinates": {
                              "latitude": "faker.location.latitude()",
                              "longitude": "faker.location.longitude()"
                            }
                          },
                          "website": "faker.internet.url()",
                          "domain": "faker.internet.domainName()",
                          "job": {
                            "title": "faker.person.jobTitle()",
                            "descriptor": "faker.person.jobDescriptor()",
                            "area": "faker.person.jobArea()",
                            "type": "faker.person.jobType()",
                            "company": "faker.company.name()"
                          },
                          "creditCard": {
                            "number": "faker.finance.creditCardNumber()",
                            "cvv": "faker.finance.creditCardCVV()",
                            "issuer": "faker.finance.creditCardIssuer()"
                          },
                          "uuid": "faker.string.uuid()",
                          "objectId": "faker.database.mongodbObjectId()"
                        }
                      ] 
                    """
        };
    }

    private async Task EditorOnDidInit()
    {
        _codeEditorInitialized = true;
        _resultEditorInitialized = true;
        _codeEditorRawJsonInitialized = true;
        await _editor.AddCommand((int)KeyMod.CtrlCmd | (int)KeyCode.KeyH, (args) =>
        {
            Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
        });

        var newDecorations = new ModelDeltaDecoration[]
        {
            new() {
                Range = new BlazorMonaco.Range(3,1,3,1),
                Options = new ModelDecorationOptions
                {
                    IsWholeLine = true,
                    ClassName = "decorationContentClass",
                    GlyphMarginClassName = "decorationGlyphMarginClass"
                }
            }
        };

        var decorationIds = await _editor.DeltaDecorations(null, newDecorations);
        // You can now use '_decorationIds' to change or remove the decorations
    }

    private void OnContextMenu(EditorMouseEvent eventArg)
    {
        Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
    }

    private async Task ChangeTheme(ChangeEventArgs e)
    {
        Console.WriteLine($"setting theme to: {e.Value?.ToString()}");
        await BlazorMonaco.Editor.Global.SetTheme(jsRuntime, e.Value?.ToString());
    }

    private async Task SetValue()
    {
        Console.WriteLine($"setting value to: {_valueToSet}");
        await _editor.SetValue(_valueToSet);
    }
  
    private async Task GenerateData()
    {
        try
        {
            Stopwatch s = Stopwatch.StartNew();
            string EditorText = await _editor.GetValue(); 
            if (!MockScript.MockScriptParser.ValidateJSON(EditorText))
            {
                Console.WriteLine("Invalid JSON input.");
                return;
            }
            JToken token = JsonConvert.DeserializeObject<JToken>(EditorText);

            var parser = MockScriptParserPool.Rent();
            string outputjson = parser.Parse(token);
            _isDataGenerated = true;
            await _editorResults.SetValue(outputjson);
            s.Stop();
            Logger.JsonLog(LogLevel.Information, "Json Generated", LoggerExtensions.LogState.GenerateJson, new { ElapsedMilliSeconds= s.ElapsedMilliseconds });
            await GetSharableURL(new MouseEventArgs()); // Call to generate sharable URL
        }
        catch (Exception e)
        {
            Logger.JsonLogException(e, LoggerExtensions.LogState.GenerateJson);
        } 
    }

    private async Task AddCommand()
    {
        await _editor.AddCommand((int)KeyMod.CtrlCmd | (int)KeyCode.Enter, (args) =>
        {
            Console.WriteLine("Ctrl+Enter : Editor command is triggered.");
        });
    }

    private async Task AddAction()
    {
        var actionDescriptor = new ActionDescriptor
        {
            Id = "testAction",
            Label = "Test Action",
            Keybindings = [(int)KeyMod.CtrlCmd | (int)KeyCode.KeyB],
            ContextMenuGroupId = "navigation",
            ContextMenuOrder = 1.5f,
            Run = (editor) =>
            {
                Console.WriteLine("Ctrl+B : Editor action is triggered.");
            }
        };
        await _editor.AddAction(actionDescriptor);
    }

    private async Task RegisterCodeActionProvider()
    {
        // Set sample marker
        var model = await _editor.GetModel();
        var markers = new List<MarkerData>
        {
            new() {
                CodeAsObject = new MarkerCode
                {
                    TargetUri = "https://www.google.com",
                    Value = "my-value"
                },
                Message = "Marker example",
                Severity = MarkerSeverity.Warning,
                StartLineNumber = 4,
                StartColumn = 3,
                EndLineNumber = 4,
                EndColumn = 7
            }
        };
        await BlazorMonaco.Editor.Global.SetModelMarkers(jsRuntime, model, "default", markers);

        // Register quick fix for marker
        await BlazorMonaco.Languages.Global.RegisterCodeActionProvider(jsRuntime, "javascript", async (modelUri, range, context) =>
        {
            var model = await BlazorMonaco.Editor.Global.GetModel(jsRuntime, modelUri);

            var codeActionList = new CodeActionList();
            if (context.Markers.Count == 0)
                return codeActionList;

            codeActionList.Actions =
            [
                new CodeAction
                {
                    Title = "Fix example",
                    Kind = "quickfix",
                    Diagnostics = markers,
                    Edit = new WorkspaceEdit
                    {
                        Edits =
                        [
                            new WorkspaceTextEdit
                            {
                                ResourceUri = modelUri,
                                TextEdit = new TextEditWithInsertAsSnippet
                                {
                                    Range = range,
                                    Text = "THIS"
                                }
                            }
                        ]
                    },
                    IsPreferred = true
                }
            ];
            return codeActionList;
        });
    }

    private async Task RegisterDocumentFormattingEditProvider()
    {
        await BlazorMonaco.Languages.Global.RegisterDocumentFormattingEditProvider(jsRuntime, "javascript", async (modelUri, options) =>
        {
            var model = await _editor.GetModel();
            var lines = await model.GetLineCount();
            var columns = await model.GetLineMaxColumn(lines);

            var value = await _editor.GetValue();
            var result = value.Split("\n").Select(m => m.Trim()).ToArray();

            return [
                new TextEdit {
                    Range = new BlazorMonaco.Range(1, 1, lines, columns),
                    Text = string.Join("\n", result)
                }
            ];
        });
    }

    private async Task RegisterCompletionItemProvider()
    {
        // Register completion item to replace warning item
        await BlazorMonaco.Languages.Global.RegisterCompletionItemProvider(jsRuntime, "javascript", async (modelUri, position, context) =>
        {
            var model = await BlazorMonaco.Editor.Global.GetModel(jsRuntime, modelUri);

            var completionList = new CompletionList()
            {
                Suggestions =
                [
                    new CompletionItem
                    {
                        LabelAsString = "Replace by THIS",
                        Kind = CompletionItemKind.Variable,
                        Detail = "this -> THIS",
                        InsertText = "THIS",
                        Preselect = true,
                        RangeAsObject = new BlazorMonaco.Range
                        {
                            StartLineNumber = 4,
                            StartColumn = 3,
                            EndLineNumber = 4,
                            EndColumn = 7
                        }
                    }
                ]
            };
            return completionList;
        });
    } 
}
