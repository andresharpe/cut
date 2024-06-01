﻿using Cut.Exceptions;
using Cut.Services;
using System.Data;
using System.Text;
using YamlDotNet.Serialization;

namespace Cut.OutputAdapters;

internal class YamlOutputAdapter : OutputAdapterBase, IOutputAdapter
{
    private readonly StreamWriter _writer;

    private DynamicDictionaryBuilder? _dataTableHelper;

    private readonly ISerializer _yaml;

    public YamlOutputAdapter(string contentName, string? fileName) : base(fileName ?? contentName + ".yaml")
    {
        _yaml = new SerializerBuilder().Build();

        _writer = new(FileName, false, Encoding.UTF8);

        _writer.WriteLine($"# Contentful model: {contentName}");
        _writer.WriteLine($"# generated by Contentful Update Tool v{VersionChecker.GetInstalledCliVersion()}");
        _writer.WriteLine($"# See: https://github.com/andresharpe/cut");
        _writer.WriteLine();
        _writer.WriteLine($"{contentName}:");
    }

    public override void AddHeadings(DataTable table)
    {
        _dataTableHelper ??= new DynamicDictionaryBuilder(table.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName.Split('.'))
            .ToList());
    }

    public override void AddRow(DataRow row)
    {
        if (_dataTableHelper == null)
        {
            throw new CliException("'AddHeadings' should be called before 'AddRows'");
        }

        var obj = _dataTableHelper.ToDictionary(row.ItemArray);

        var sb = new StringBuilder(_yaml.Serialize(obj));
        sb.Replace("\n", "\n  ");
        sb.Length -= 2;

        _writer.Write("- ");
        _writer.WriteLine(sb.ToString());
    }

    public override void Dispose()
    {
        _writer.Dispose();
    }

    public override void Save()
    {
        _writer.Flush();
    }
}