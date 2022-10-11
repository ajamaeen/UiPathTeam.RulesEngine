using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiPathTeam.RulesEngine.Activities.Design
{
    /// <summary>
    /// Class that represent the RuleSet basic infromation:
    /// Name, Major Version and Minor Version
    /// </summary>
    public class RuleSetSummary
    {
        public string Name { get; set; }

        public int MajorVersion { get; set; }

        public int MinorVersion { get; set; }

        public string DisplayName
        {
            get;
            set;
        }
    }
}
