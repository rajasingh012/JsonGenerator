 
namespace SampleWebApp.Logging;


using System.Collections;
using System.Collections.Generic;
using Google.Cloud.Logging.Log4Net;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using log4net.Core;


/// <summary>
/// Json Layout For Cloud Logging
/// </summary>
public class GcpJsonLayout : IJsonLayout, IOptionHandler
{

    /// <summary>
    /// template
    /// </summary>
    private Struct _template;


    /// <summary>
    /// _template Struct
    /// </summary>
    public void ActivateOptions()
    {
        _template = new Struct
        {
        };
    }


    /// <summary>
    /// Format
    /// </summary>
    /// <param name="loggingEvent"></param>
    /// <returns></returns>
    public Struct Format(LoggingEvent loggingEvent)
    {
        Struct jsonStruct = _template.Clone();
        MapField<string, Value> fields = jsonStruct.Fields;
        fields["Message"] = Value.ForString(loggingEvent.RenderedMessage);
        if (loggingEvent.ExceptionObject != null)
        {
            fields["Exception"] = Value.ForString(loggingEvent.ExceptionObject.Message);
        }
        //fields["timestamp"] = Value.ForString(loggingEvent.TimeStamp.ToString("o"));
        fields["level"] = Value.ForString(loggingEvent?.Level?.Name);
        fields["logger"] = Value.ForString(loggingEvent?.LoggerName);
        //fields["methodname"] = Value.ForString(loggingEvent.LocationInformation.MethodName);
        //fields["filename"] = Value.ForString(loggingEvent.LocationInformation.FileName ?? "");
        //fields["LineNumber"] = Value.ForString(loggingEvent.LocationInformation.LineNumber);
        try
        {
            foreach (DictionaryEntry property in loggingEvent.GetProperties())
            {
                var key = property.Key.ToString();
                var value = property.Value;

                if (value is string stringValue)
                {
                    fields[key] = Value.ForString(stringValue);
                }
                else if (value is IDictionary<string, object> dictValue)
                {
                    fields[key] = Value.ForStruct(ToProtobufStruct(dictValue));
                }
                else if (value is IList<object> listValue)
                {
                    fields[key] = Value.ForList(ToProtobufValueList(listValue));
                }
                else
                {
                    fields[key] = Value.ForString(value?.ToString() ?? string.Empty);
                }
            }

        }
        catch { }
        return jsonStruct;
    }


    /// <summary>
    /// To Protobuf Struct
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    private Struct ToProtobufStruct(IDictionary<string, object> dictionary)
    {
        Struct structObject = new Struct();
        foreach (var kvp in dictionary)
        {
            if (kvp.Value is IDictionary<string, object> nestedDict)
            {
                structObject.Fields[kvp.Key] = Value.ForStruct(ToProtobufStruct(nestedDict));
            }
            else if (kvp.Value is string stringValue)
            {
                structObject.Fields[kvp.Key] = Value.ForString(stringValue);
            }
            else if (kvp.Value is int intValue)
            {
                structObject.Fields[kvp.Key] = Value.ForNumber(intValue);
            }
            else if (kvp.Value is long longValue)
            {
                structObject.Fields[kvp.Key] = Value.ForNumber(longValue);
            }
            else if (kvp.Value is double doubleValue)
            {
                structObject.Fields[kvp.Key] = Value.ForNumber(doubleValue);
            }
            else if (kvp.Value is bool boolValue)
            {
                structObject.Fields[kvp.Key] = Value.ForBool(boolValue);
            }
            else if (kvp.Value is IList<object> listValue)
            {
                structObject.Fields[kvp.Key] = Value.ForList(ToProtobufValueList(listValue));
            }
            else
            {
                structObject.Fields[kvp.Key] = Value.ForString(kvp.Value?.ToString() ?? string.Empty);
            }
        }
        return structObject;
    }


    /// <summary>
    /// To Protobuf ValueList
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private Value[] ToProtobufValueList(IList<object> list)
    {
        List<Value> values = new List<Value>();
        foreach (var item in list)
        {
            if (item is IDictionary<string, object> nestedDict)
            {
                values.Add(Value.ForStruct(ToProtobufStruct(nestedDict)));
            }
            else if (item is string stringValue)
            {
                values.Add(Value.ForString(stringValue));
            }
            else if (item is int intValue)
            {
                values.Add(Value.ForNumber(intValue));
            }
            else if (item is long longValue)
            {
                values.Add(Value.ForNumber(longValue));
            }
            else if (item is double doubleValue)
            {
                values.Add(Value.ForNumber(doubleValue));
            }
            else if (item is bool boolValue)
            {
                values.Add(Value.ForBool(boolValue));
            }
            else if (item is IList<object> nestedList)
            {
                values.Add(Value.ForList(ToProtobufValueList(nestedList)));
            }
            else
            {
                values.Add(Value.ForString(item?.ToString() ?? string.Empty));
            }
        }
        return values.ToArray();
    }
}
