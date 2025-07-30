 
using System.Collections.Concurrent;
using System.Diagnostics;
using MockScript;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MockScript;
public static class MockScriptParserPool
{
    private static ConcurrentBag<MockScriptParser> _pool = new();

    public static MockScriptParser Rent()
    {
        if (_pool.TryTake(out var parser))
            return parser;

        return new MockScriptParser(); // create new if none available
    }

    public static void Return(MockScriptParser parser)
    {
        _pool.Add(parser);
    }
   
}
