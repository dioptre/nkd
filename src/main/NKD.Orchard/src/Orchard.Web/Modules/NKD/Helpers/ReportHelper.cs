using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data;
using DevExpress.Web.Mvc;
using System.ComponentModel;
using System.Web.Mvc;
using System.Collections;
using DevExpress.XtraReports.UI;
using DevExpress.Web.ASPxClasses.Internal;
using DevExpress.XtraReports.Parameters;
using NKD.ViewModels;

namespace NKD.Helpers
{
    public static class ReportHelper
    {
        public class ReportRegistrationItem
        {
            public Func<IReport, XtraReport> ReportBuilder { get; set; }
            public string ParametersView { get; set; }
        }

        public static string GetValidColumnName(string columnName, DataTable table)
        {
            if (string.IsNullOrEmpty(columnName))
                return string.Empty;
            if (table.Columns.Contains(columnName))
                return columnName;
            string[] words = columnName.Split('_');
            if (words == null || words.Length == 0)
                return string.Empty;
            string name = DevExpress.XtraPrinting.StringUtils.Join(" ", words);
            if (table.Columns.Contains(name))
                return name;
            return string.Empty;
        }

        public static void CopyToDataTable<T>(this IEnumerable<T> query, DataTable table)
        {
            if (query == null)
                return;

            PropertyInfo[] properties = null;

            foreach (T item in query)
            {
                if (properties == null)
                    properties = ((Type)item.GetType()).GetProperties();

                DataRow row = table.NewRow();

                foreach (PropertyInfo property in properties)
                {
                    string columnName = GetValidColumnName(property.Name, table);
                    if (string.IsNullOrEmpty(columnName))
                        continue;
                    var propertyValue = property.GetValue(item, null);
                    if (propertyValue is System.Data.Linq.Binary)
                        propertyValue = ((System.Data.Linq.Binary)propertyValue).ToArray();
                    row[columnName] = propertyValue ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }
        }

        public static IEnumerable<SelectListItem> Generate<T>(T selectedValue)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            foreach (Enum value in Enum.GetValues(typeof(T)))
                yield return new SelectListItem()
                {
                    Text = converter.ConvertToInvariantString(value),
                    Selected = Enum.Equals(value, selectedValue)
                };
        }

        public static IEnumerable<SelectListItem> Generate(string[] values, int selectedIndex)
        {
            for (int i = 0; i < values.Length; i++)
                yield return new SelectListItem()
                {
                    Value = i.ToString(),
                    Text = values[i],
                    Selected = i == selectedIndex
                };
        }

        public static IEnumerable<SelectListItem> GetFormattingRuleConditionItems(string[] FormattingRuleConditions, int defaultValue)
        {
            return Generate(FormattingRuleConditions, defaultValue);
        }
        public static IEnumerable<SelectListItem> GetFormattingRuleStyleItems(string[] FormattingRuleStyles, int defaultValue)
        {
            return Generate(FormattingRuleStyles, defaultValue);
        }

        public class ParameterDictionaryBinder : DefaultModelBinder
        {
            static IEnumerable<string> GetValueProviderKeys(ControllerContext context)
            {
                return context.HttpContext.Request.Params.AllKeys;
            }
            static object ConvertType(string stringValue, Type type)
            {
                return TypeDescriptor.GetConverter(type).ConvertFrom(stringValue);
            }

            public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                Type modelType = bindingContext.ModelType;
                Type idictType = modelType.GetInterface("System.Collections.Generic.IDictionary`2");

                if (idictType != null)
                {
                    Type[] argumetTypes = idictType.GetGenericArguments();

                    object result = null;
                    IModelBinder valueBinder = Binders.GetBinder(argumetTypes[1]);

                    foreach (string key in GetValueProviderKeys(controllerContext))
                    {
                        if (!key.StartsWith(bindingContext.ModelName, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        object dictKey;
                        string parameterName = key.Substring(bindingContext.ModelName.Length);
                        try
                        {
                            dictKey = ConvertType(parameterName, argumetTypes[0]);
                        }
                        catch (NotSupportedException)
                        {
                            continue;
                        }

                        ModelBindingContext innerBindingContext = new ModelBindingContext()
                        {
                            ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, argumetTypes[1]),
                            ModelName = key,
                            ModelState = bindingContext.ModelState,
                            PropertyFilter = bindingContext.PropertyFilter,
                            ValueProvider = bindingContext.ValueProvider
                        };
                        object newPropertyValue = valueBinder.BindModel(controllerContext, innerBindingContext);

                        if (result == null)
                            result = CreateModel(controllerContext, bindingContext, modelType);

                        if (!(bool)idictType.GetMethod("ContainsKey").Invoke(result, new object[] { dictKey }))
                            idictType.GetProperty("Item").SetValue(result, newPropertyValue, new object[] { dictKey });
                    }
                    return result;
                }
                return new DefaultModelBinder().BindModel(controllerContext, bindingContext);
            }
        }

        public interface ITableReportDataFiller
        {
            void Fill(TableReport report);
        }

        public partial class TableReport : DevExpress.XtraReports.UI.XtraReport
        {
            public string ParameterString { get; set; }

            #region inner classes
            class TableAdapterDataFiller : ITableReportDataFiller
            {
                public void Fill(TableReport report)
                {
                    IDataAdapter ad = report.DataAdapter as IDataAdapter;
                    DataSet ds = report.DataSource as DataSet;
                    if (ad != null && ds != null)
                        ad.Fill(ds);
                }
            }
            #endregion

            ITableReportDataFiller dataFiller;

            public TableReport() : this(new TableAdapterDataFiller()) { }
            public TableReport(ITableReportDataFiller dataFiller)
            {
                this.dataFiller = dataFiller;                
            }

            protected override void BeforeReportPrint()
            {
                dataFiller.Fill(this);
                base.BeforeReportPrint();
            }

        }

    }
}