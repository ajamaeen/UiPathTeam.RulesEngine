using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPathTeam.RulesEngine.Activities.Design;

namespace UiPathTeam.RulesEngine.Test
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var policy = new RulesPolicyDesigner();            
            var ruleSetEditorDialog = new Activities.Design.Dialogs.RuleSetEditorDialog("customer-discounts.rules");
            ruleSetEditorDialog.Show();
        }
    }
}
