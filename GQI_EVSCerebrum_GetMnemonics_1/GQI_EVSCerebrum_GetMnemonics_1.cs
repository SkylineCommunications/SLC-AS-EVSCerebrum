using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using GQI_EVSCerebrum_GetMnemonics_1;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.Advanced;

[GQIMetaData(Name = "EVS Cerebrum Get Mnemonics")]
public class GQI_EVSCerebrum_GetMnemonics : IGQIDataSource, IGQIOnInit, IGQIInputArguments
{
    private readonly GQIStringArgument mnemonicTypeArgument = new GQIStringArgument("Mnemonic Type") { IsRequired = true };
    private MnemonicType mnemonic;

    private GQIDMS dms;

    private int dataminerId;
    private int elementId;

    public GQIColumn[] GetColumns()
    {
        return new GQIColumn[]
        {
            new GQIStringColumn("Instance"),
            new GQIStringColumn("Mnemonic"),
            new GQIStringColumn("Categories"),
        };
    }

    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { mnemonicTypeArgument };
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        var rawMnemonic = Convert.ToString(args.GetArgumentValue(mnemonicTypeArgument));
        mnemonic = (MnemonicType) Enum.Parse(typeof(MnemonicType), rawMnemonic);

        return new OnArgumentsProcessedOutputArgs();
    }

    public GQIPage GetNextPage(GetNextPageInputArgs args)
    {
        GQIPage page = null;

        switch (mnemonic)
        {
            case MnemonicType.Sources:
                page = HandleSources();
                break;
            case MnemonicType.Destinations:
                page = HandleDestinations();
                break;
            default:
                page = new GQIPage(new GQIRow[0])
                {
                    HasNextPage = false,
                };
                break;
        }

        return page;
    }

    //public GQIPage GetNextPage(GetNextPageInputArgs args)
    //{
    //    var levels = GetLevels();
    //    var destinationAssociations = GetDestinationAssociations();

    //    var rows = new List<GQIRow>();
    //    foreach (var destinationAssociation in destinationAssociations.OrderBy(d => d.Instance))
    //    {
    //        var route = GetRouteForDestinationAssociation(destinationAssociation.Instance);
    //        if (!route.IsValid())
    //        {
    //            //levels.TryGetValue(destinationAssociation.LevelInstance, out var level);
    //            var level = levels.Values.FirstOrDefault(lvl => destinationAssociation.DisplayKey != null && destinationAssociation.DisplayKey.Contains(lvl.Mnemonic));

    //            // get levels for destination
    //            route = new Route
    //            {
    //                Destination = destinationAssociation.DestinationName,
    //                DestinationLevel = level?.Mnemonic,
    //                Source = String.Empty,
    //                SourceLevel = String.Empty,
    //            };
    //        }

    //        var row = new GQIRow(
    //            new[]
    //            {
    //                new GQICell { Value = route.Destination },
    //                new GQICell { Value = route.DestinationLevel },
    //                new GQICell { Value = route.Source },
    //                new GQICell { Value = route.SourceLevel },
    //            });
    //        rows.Add(row);
    //    }

    //    return new GQIPage(rows.ToArray())
    //    {
    //        HasNextPage = false,
    //    };
    //}

    public OnInitOutputArgs OnInit(OnInitInputArgs args)
    {
        dms = args.DMS;
        GetEvsCerebrumArgument();

        return new OnInitOutputArgs();
    }

    private void GetEvsCerebrumArgument()
    {
        dataminerId = -1;
        elementId = -1;

        var infoMessage = new GetInfoMessage { Type = InfoType.ElementInfo };
        var infoMessageResponses = dms.SendMessages(infoMessage);
        foreach (var response in infoMessageResponses)
        {
            var elementInfoEventMessage = (ElementInfoEventMessage)response;
            if (elementInfoEventMessage == null)
            {
                continue;
            }

            if (elementInfoEventMessage?.Protocol == "EVS Cerebrum" && elementInfoEventMessage?.ProtocolVersion == "Production")
            {
                dataminerId = elementInfoEventMessage.DataMinerID;
                elementId = elementInfoEventMessage.ElementID;
                break;
            }
        }
    }

    private List<Mnemonic> GetSources()
    {
        var columns = GetSourcesTableColumns();
        if (!columns.Any())
        {
            return new List<Mnemonic>();
        }

        var sources = CreateMnemonics(columns);

        return sources;
    }

    private ParameterValue[] GetSourcesTableColumns()
    {
        var tableId = 14100;
        var getPartialTableMessage = new GetPartialTableMessage(dataminerId, elementId, tableId, new[] { "forceFullTable=true" });
        var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
        if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
        {
            return new ParameterValue[0];
        }

        var columns = parameterChangeEventMessage.NewValue.ArrayValue;
        if (columns.Length < 7)
        {
            return new ParameterValue[0];
        }

        return columns;
    }

    private List<Mnemonic> GetDestinations()
    {
        var columns = GetDestinationTableColumns();
        if (!columns.Any())
        {
            return new List<Mnemonic>();
        }

        var destinations = CreateMnemonics(columns);

        return destinations;
    }

    private ParameterValue[] GetDestinationTableColumns()
    {
        var tableId = 15100;
        var getPartialTableMessage = new GetPartialTableMessage(dataminerId, elementId, tableId, new[] { "forceFullTable=true" });
        var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
        if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
        {
            return new ParameterValue[0];
        }

        var columns = parameterChangeEventMessage.NewValue.ArrayValue;
        if (columns.Length < 7)
        {
            return new ParameterValue[0];
        }

        return columns;
    }

    private List<Category> GetSourceCategories()
    {
        var columns = GetSourceCategoryTableColumns();
        if (!columns.Any())
        {
            return new List<Category>();
        }

        var categories = CreateCategories(columns);

        return categories;
    }

    private ParameterValue[] GetSourceCategoryTableColumns()
    {
        var tableId = 17300;
        var getPartialTableMessage = new GetPartialTableMessage(dataminerId, elementId, tableId, new[] { "forceFullTable=true" });
        var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
        if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
        {
            return new ParameterValue[0];
        }

        var columns = parameterChangeEventMessage.NewValue.ArrayValue;
        if (columns.Length < 4)
        {
            return new ParameterValue[0];
        }

        return columns;
    }

    private List<Category> GetDestinationCategories()
    {
        var columns = GetDestinationCategoryTableColumns();
        if (!columns.Any())
        {
            return new List<Category>();
        }

        var categories = CreateCategories(columns);

        return categories;
    }

    private ParameterValue[] GetDestinationCategoryTableColumns()
    {
        var tableId = 17400;
        var getPartialTableMessage = new GetPartialTableMessage(dataminerId, elementId, tableId, new[] { "forceFullTable=true" });
        var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
        if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
        {
            return new ParameterValue[0];
        }

        var columns = parameterChangeEventMessage.NewValue.ArrayValue;
        if (columns.Length < 4)
        {
            return new ParameterValue[0];
        }

        return columns;
    }

    private List<Mnemonic> CreateMnemonics(ParameterValue[] columns)
    {
        var mnemonics = new List<Mnemonic>();

        for (int i = 0; i < columns[0].ArrayValue.Length; i++)
        {
            var nameValue = columns[5].ArrayValue[i]?.CellValue?.GetAsStringValue();
            if (string.IsNullOrWhiteSpace(nameValue) || nameValue == "Not initialized") continue;

            var mnemonic = new Mnemonic
            {
                Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                Name = nameValue,
            };

            mnemonics.Add(mnemonic);
        }

        return mnemonics;
    }

    private List<Category> CreateCategories(ParameterValue[] columns)
    {
        var categories = new List<Category>();

        for (int i = 0; i < columns[0].ArrayValue.Length; i++)
        {
            var category = new Category
            {
                Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                Name = columns[1].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                Mnemonic = columns[3].ArrayValue[i]?.CellValue?.GetAsStringValue(),
            };

            categories.Add(category);
        }

        return categories;
    }

    private void UpdateMnemonicCategories(List<Mnemonic> mnemonics, List<Category> categories)
    {
        foreach (var mnemonic in mnemonics)
        {
            var matchingCategories = categories.Where(c => c.Mnemonic == mnemonic.Name).Select(c => c.Name).ToList();
            mnemonic.Categories = matchingCategories;
        }
    }

    private GQIPage CreatePage(List<Mnemonic> mnemonics)
    {
        var rows = new List<GQIRow>();

        foreach (var mnemonic in mnemonics.OrderBy(d => d.Name))
        {
            var row = new GQIRow(
                new[]
                {
                    new GQICell { Value = mnemonic.Instance },
                    new GQICell { Value = mnemonic.Name },
                    new GQICell { Value = string.Join(";", mnemonic.Categories) },
                });

            rows.Add(row);
        }

        return new GQIPage(rows.ToArray())
        {
            HasNextPage = false,
        };
    }

    private GQIPage HandleSources()
    {
        var sources = GetSources();
        var sourceCategories = GetSourceCategories();
        UpdateMnemonicCategories(sources, sourceCategories);
        var page = CreatePage(sources);

        return page;
    }

    private GQIPage HandleDestinations()
    {
        var destinations = GetDestinations();
        var destinationCategories = GetDestinationCategories();
        UpdateMnemonicCategories(destinations, destinationCategories);
        var page = CreatePage(destinations);

        return page;
    }
}