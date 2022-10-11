using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;

namespace UiPathTeam.RulesEngine.Activities.Design
{
    /// <summary>
    /// Class to wrap System.Workflow.Activities.Rules.RuleSet and its original name and version 
    /// </summary>
    public class RuleSetData : IComparable<RuleSetData>
    {
        #region Variables and constructor 

        public RuleSetData()
        {
        }

        private string name;
        private string originalName;
        private int majorVersion;
        private int originalMajorVersion;
        private int minorVersion;
        private int originalMinorVersion;
        private string ruleSetDefinition;
        private RuleSet ruleSet;
        private short status;
        private string assemblyPath;
        private string activityName;
        private DateTime modifiedDate;
        private bool dirty;
        private Type activity;

        private WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
               /* if (this.RuleSet != null)
                    this.RuleSet.Name = name;*/
            }
        }
        public string OriginalName
        {
            get { return originalName; }
            set { originalName = value; }
        }
        public int MajorVersion
        {
            get { return majorVersion; }
            set { majorVersion = value; }
        }
        public int OriginalMajorVersion
        {
            get { return originalMajorVersion; }
            set { originalMajorVersion = value; }
        }
        public int MinorVersion
        {
            get { return minorVersion; }
            set { minorVersion = value; }
        }
        public int OriginalMinorVersion
        {
            get { return originalMinorVersion; }
            set { originalMinorVersion = value; }
        }
        public string RuleSetDefinition
        {
            get { return ruleSetDefinition; }
            set { ruleSetDefinition = value; }
        }
        public RuleSet RuleSet
        {
            get
            {
                if (ruleSet == null)
                {
                    ruleSet = this.DeserializeRuleSet(ruleSetDefinition);
                }
                return ruleSet;
            }
            set
            {
                ruleSet = value;
                //name = ruleSet.Name;
            }
        }
        public short Status
        {
            get { return status; }
            set { status = value; }
        }
        public string AssemblyPath
        {
            get { return assemblyPath; }
            set { assemblyPath = value; }
        }
        public string ActivityName
        {
            get { return activityName; }
            set { activityName = value; }
        }
        public DateTime ModifiedDate
        {
            get { return modifiedDate; }
            set { modifiedDate = value; }
        }
        public bool Dirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        public Type Activity
        {
            get { return activity; }
            set
            {
                activity = value;
                if (activity != null)
                    activityName = activity.ToString();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deserialize RuleSet from XML to System.Workflow.Activities.Rules.RuleSet
        /// </summary>
        /// <param name="ruleSetXmlDefinition"> RuleSet XML serialized string</param>
        /// <returns></returns>
        private RuleSet DeserializeRuleSet(string ruleSetXmlDefinition)
        {
            if (!String.IsNullOrEmpty(ruleSetXmlDefinition))
            {
                StringReader stringReader = new StringReader(ruleSetXmlDefinition);
                XmlTextReader reader = new XmlTextReader(stringReader);
                return serializer.Deserialize(reader) as RuleSet;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// return RuleSet [Name - MajorVersion.MinorVersion]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} - {1}.{2}", name, majorVersion, minorVersion);
        }

        /// <summary>
        /// Deep Clone the current instance
        /// </summary>
        /// <returns></returns>
        public RuleSetData Clone()
        {
            RuleSetData newData = new RuleSetData();
            newData.Activity = this.Activity;
            //newData.ActivityName = activityName; //Set by setting Activity
            newData.AssemblyPath = this.AssemblyPath;
            newData.Dirty = true;
            newData.MajorVersion = this.MajorVersion;
            newData.MinorVersion = this.MinorVersion;
            newData.Name = name;
            newData.RuleSet = this.RuleSet.Clone();
            newData.Status = 0;

            return newData;
        }

        #endregion

        #region IComparable<RuleSetData> Members

        /// <summary>
        /// Compares current instance with the provided RuleSetData object using (In order) the name, Major version and Minor version. 
        /// </summary>
        /// <param name="other"> The second RuleSetData to compare.</param>
        /// <returns></returns>
        public int CompareTo(RuleSetData other)
        {
            if (other != null)
            {
                int nameComparison = String.CompareOrdinal(this.Name, other.Name);
                if (nameComparison != 0)
                    return nameComparison;

                int majorVersionComparison = this.MajorVersion - other.MajorVersion;
                if (majorVersionComparison != 0)
                    return majorVersionComparison;

                int minorVersionComparison = this.MinorVersion - other.MinorVersion;
                if (minorVersionComparison != 0)
                    return minorVersionComparison;

                return 0;
            }
            else
            {
                return 1;
            }
        }

        #endregion
    }
}
