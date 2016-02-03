using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Localization;

namespace Belitsoft.Orchard.Faq.DataMigrations
{
    public class Migrations : DataMigrationImpl
    {

        public Localizer T { get; set; }

        public Migrations()
        {
            T = NullLocalizer.Instance;
        }

        public int Create()
        {
            SchemaBuilder.CreateTable("FaqTypePartRecord", table => table.ContentPartRecord()
                                                                         .Column<string>("Title", cfg => cfg.WithLength(1024)));

            SchemaBuilder.CreateTable("FaqPartRecord", table => table
                                                                    .ContentPartRecord()
                                                                    .Column<string>("Question", column => column.Unlimited())
                                                                    .Column<int>("FaqTypeId"));

            ContentDefinitionManager.AlterTypeDefinition("Faq",
                                                         cfg => cfg.WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False"))
                                                                    .WithPart("FaqPart")
                                                                    .WithPart("BodyPart", cfg1 => cfg1.WithSetting("Title", "Answer").WithSetting("Name", "Answer").WithSetting("Required", "true"))
                                                                    .Creatable().Draftable());

            ContentDefinitionManager.AlterTypeDefinition("FaqType",cfg => cfg.WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False")).WithPart("FaqTypePart").Creatable());


            SchemaBuilder.CreateTable("FaqWidgetPartRecord", table => table.ContentPartRecord());

            ContentDefinitionManager.AlterTypeDefinition("FaqWidget",
                                                        cfg =>
                                                        cfg.WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False"))
                                                        .WithPart("WidgetPart")
                                                        .WithPart("FaqWidgetPart")
                                                        .WithSetting("Stereotype", "Widget"));


            SchemaBuilder.CreateTable("FaqWidgetAjaxPartRecord", table => table.ContentPartRecord());

            ContentDefinitionManager.AlterTypeDefinition("FaqWidgetAjax",
                                                        cfg =>
                                                        cfg.WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False"))
                                                        .WithPart("WidgetPart")
                                                        .WithPart("FaqWidgetAjaxPart")
                                                        .WithSetting("Stereotype", "Widget"));

            return 1;
        }

    }
}