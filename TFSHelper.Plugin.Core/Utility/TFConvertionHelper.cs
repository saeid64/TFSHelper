﻿using System.Collections.Generic;
using TFSHelper.Helper.Model;

namespace TFSHelper.Plugin.Core.Utility
{
    class TFWorkItemHelper
    {
        internal static IEnumerable<TFSField> GetFieldList(CoreFieldsType fields)
        {
            var tfsItems = new TFSFieldList();
            tfsItems.AddRange(fields.IntegerFields.Select(integerField => new TFSField { Id = integerField.ReferenceName, Type = TFSType.Integer, NewValue = integerField.NewValue, OldValue = integerField.OldValue }));
            tfsItems.AddRange(fields.StringFields.Select(stringField => new TFSField { Id = stringField.ReferenceName, Type = TFSType.String, NewValue = stringField.NewValue, OldValue = stringField.OldValue }));

            return tfsItems;
        }
    }
}
