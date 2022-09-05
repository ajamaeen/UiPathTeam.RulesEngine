using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using UiPathTeam.RulesEngine.Activities.Design.Designers;
using UiPathTeam.RulesEngine.Activities.Design.Properties;

namespace UiPathTeam.RulesEngine.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(RulesPolicy<>), categoryAttribute);
            builder.AddCustomAttributes(typeof(RulesPolicy<>), new DesignerAttribute(typeof(RulesPolicyDesigner)));
            builder.AddCustomAttributes(typeof(RulesPolicy<>), new HelpKeywordAttribute(""));

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
