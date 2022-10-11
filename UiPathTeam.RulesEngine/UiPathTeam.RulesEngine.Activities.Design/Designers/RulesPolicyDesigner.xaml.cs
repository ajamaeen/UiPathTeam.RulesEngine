namespace UiPathTeam.RulesEngine.Activities.Design
{
    using System;
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Forms;
    using System.Workflow.Activities.Rules;
    using System.Workflow.Activities.Rules.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;
    using UiPath.Shared.Activities.Design.Controls;
    using UiPath.Shared.Activities.Design.Services;
    using UiPathTeam.RulesEngine.Activities.Design;
    using UiPathTeam.RulesEngine.Activities.Design.Dialogs;

    /// <summary>
    /// Interaction logic for RulesPolicyDesigner.xaml
    /// </summary>
    public partial class RulesPolicyDesigner
    {
        private readonly string ruleSetNameProperty = "RuleSetName";
        private readonly string ruleFilePathProperty = "RulesFilePath";
        private readonly string targetObjectProperty = "TargetObject";
        
        public RulesPolicyDesigner()
        {
            InitializeComponent();
            this.Loaded += RulesPolicyDesigner_Loaded;
        }

        private void RulesPolicyDesigner_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ModelItem.GetInArgumentValue<string>(ruleFilePathProperty)))
            {
                LoadRuleSets(ModelItem.GetInArgumentValue<string>(ruleFilePathProperty));
            }
        }

        /// <summary>
        /// Edit Rule Set Click Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnEditRuleSets_Click(object sender, RoutedEventArgs e)
        {
            string rulesFilePath = ModelItem.GetInArgumentValue<string>(ruleFilePathProperty);

            if (string.IsNullOrWhiteSpace(rulesFilePath))
            {
                System.Windows.Forms.MessageBox.Show("Rules file Path needs to be configured before viewing or editing the rules.",
                                                     "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RuleSetEditorDialog ruleSetEditorDialog = new RuleSetEditorDialog(ModelItem);
            if (ruleSetEditorDialog.ShowOkCancel())
            {
                LoadRuleSets(rulesFilePath);
            }
        }

        /// <summary>
        /// Check if the file exists. If not, it will create a new file with an empty rule set with the provided name
        /// </summary>
        /// <param name="filePath">Rules File Path </param>
        /// <param name="ruleSetName">Rule Set Name to be used when creating a empty file in case the file is not exist</param>
        private void CreateEmptyFileIfNotExists(string filePath, string ruleSetName)
        {
            if (!File.Exists(filePath))
            {
                //create empty file
                File.Create(filePath).Close();

                WorkflowMarkupSerializer ser = new WorkflowMarkupSerializer();

                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    RuleDefinitions ruleDefs = new RuleDefinitions();

                    if (!string.IsNullOrWhiteSpace(ruleSetName))
                    {
                        var ruleSet = new RuleSet()
                        {
                            Name = ruleSetName,
                        };
                        ruleDefs.RuleSets.Add(ruleSet);
                    }

                    using (var xmlTW = new System.Xml.XmlTextWriter(filePath, null))
                    {
                        ser.Serialize(xmlTW, ruleDefs);
                    }
                }
            }
        }

        /// <summary>
        /// Edit rule Click Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnEditRules_Click(object sender, RoutedEventArgs e)
        {
            string rulesFilePath = ModelItem.GetInArgumentValue<string>(ruleFilePathProperty);

            if (string.IsNullOrWhiteSpace(rulesFilePath))
            {
                System.Windows.MessageBox.Show("Rules file Path needs to be configured before viewing or editing the rules");
                return;
            }

            string ruleSetName = ModelItem.GetInArgumentValue<string>(ruleSetNameProperty);
            if (string.IsNullOrWhiteSpace(ruleSetName))
            {
                System.Windows.MessageBox.Show("RuleSet Name needs to be configured before viewing or editing the rules");
                return;
            }

            ModelItem targetObjectModelItem = ModelItem.Properties["TargetObject"].Value;
            if (targetObjectModelItem == null || targetObjectModelItem.GetCurrentValue() == null)
            {
                System.Windows.MessageBox.Show("TargetObject needs to be configured before viewing or editing the rules");
                return;
            }

            //// verify that target object is correctly configured
            InArgument targetObjArg = targetObjectModelItem.GetCurrentValue() as InArgument;
            if (targetObjArg == null)
            {
                System.Windows.MessageBox.Show("Invalid target object");
                return;
            }

            CreateEmptyFileIfNotExists(rulesFilePath, ruleSetName);

            // open the ruleset editor
            Type targetObjectType = targetObjArg.ArgumentType;
            WorkflowMarkupSerializer ser = new WorkflowMarkupSerializer();

            using (Stream stream = new FileStream(rulesFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                DialogResult result;
                RuleSet ruleSet;
                RuleDefinitions ruleDefs;
                RuleSetDialog ruleSetDialog;
                try
                {
                    using (XmlTextReader reader = new XmlTextReader(stream))
                    {
                        ruleDefs = ser.Deserialize(reader) as RuleDefinitions;
                        if (!ruleDefs.RuleSets.Contains(ruleSetName))
                        {
                            throw new InvalidOperationException($"Ruleset name {ruleSetName} not found in {rulesFilePath}");
                        }
                        ruleSet = ruleDefs.RuleSets[ruleSetName];
                    }

                    // popup the dialog for viewing the rules
                    ruleSetDialog = new RuleSetDialog(targetObjectType, null, ruleSet);
                    //TweakRuleSetDialogToResizable(ruleSetDialog);
                    result = ruleSetDialog.ShowDialog();
                }
                finally
                {
                    if (stream != null)
                        stream.Dispose();
                }

                // update if they changed the Rules
                if (result == DialogResult.OK) //If OK was pressed
                {
                    for (int index = 0; index < ruleDefs.RuleSets.Count; index++)
                    {
                        if (ruleDefs.RuleSets[index].Name == (string)ruleSetName)
                        {
                            ruleDefs.RuleSets[index] = ruleSetDialog.RuleSet;
                            break;
                        }
                    }
                    try
                    {
                        using (var xmlTW = new System.Xml.XmlTextWriter(rulesFilePath, null))
                        {
                            ser.Serialize(xmlTW, ruleDefs);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // File does not have write access. Make a local copy so user changes are not lost
                        FileInfo fileInfo = new FileInfo(rulesFilePath);
                        // create local file by adding a random suffix to original filename
                        string localFileCopy = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.')) + new Random().Next() + fileInfo.Extension;

                        using (var xmlTW = new System.Xml.XmlTextWriter(localFileCopy, null))
                        {
                            ser.Serialize(xmlTW, ruleDefs);
                        }
                        System.Windows.MessageBox.Show("Rules file is not writeable. Created copy of your changes in " + localFileCopy);
                    }
                }
            }
        }

        /// <summary>
        /// Rules file path Changed Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filePathControl_FileSelected(object sender, RoutedEventArgs e)
        {
            var args = (FileSelectedRoutedEventArgs)e;
            if (args != null)
            {
                ModelItem.Properties[ruleFilePathProperty].SetValue(new InArgument<string>(args.Path));
                if (!File.Exists(args.Path))
                {
                    using (Stream stream = new FileStream(args.Path, FileMode.OpenOrCreate))
                    {
                    }
                    RuleDefinitions ruleDefinitions = new RuleDefinitions();
                    WorkflowMarkupSerializer ser = new WorkflowMarkupSerializer();
                    ser.Serialize(new XmlTextWriter(args.Path, null), ruleDefinitions);
                }
                else
                {
                    LoadRuleSets(args.Path);
                }
            }
        }

        /// <summary>
        /// Selected Rule Set changed Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbRuleSetNames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbRuleSetNames.SelectedItem != null)
            {
                var selectedItem = (RuleSetSummary)cbRuleSetNames.SelectedItem;
                ModelItem.Properties[ruleSetNameProperty].SetValue(new InArgument<string>(selectedItem.Name));
            }
            else
            {
                ModelItem.Properties[ruleSetNameProperty].SetValue(new InArgument<string>(""));
            }
        }

        /// <summary>
        /// Load the RuleSets from the provided rules file path
        /// </summary>
        /// <param name="filePath">Rules file path</param>
        private void LoadRuleSets(string filePath)
        {
            var ruelSets = new List<RuleSetSummary>();

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    WorkflowMarkupSerializer ser = new WorkflowMarkupSerializer();
                    RuleDefinitions ruleDefinitions;
                    try
                    {
                        using (XmlTextReader reader = new XmlTextReader(stream))
                        {
                            ruleDefinitions = ser.Deserialize(reader) as RuleDefinitions;
                            foreach (var ruleset in ruleDefinitions.RuleSets)
                            {
                                RuleSetSummary ruleSetSummary = new RuleSetSummary();
                                ruleSetSummary.Name = ruleset.Name;

                                string[] nameSplitted = ruleset.Name.Split('-');
                                if (nameSplitted.Length == 3)
                                {
                                    if (int.TryParse(nameSplitted[1], out int majorVersion))
                                    {
                                        ruleSetSummary.MajorVersion = majorVersion;
                                    }

                                    if (int.TryParse(nameSplitted[2], out int minorVersion))
                                    {
                                        ruleSetSummary.MinorVersion = minorVersion;
                                    }

                                    ruleSetSummary.DisplayName = $"{nameSplitted[0]} - v{ ruleSetSummary.MajorVersion}.{ruleSetSummary.MinorVersion}";
                                }
                                else
                                {
                                    ruleSetSummary.Name = ruleset.Name;
                                }

                                if (string.IsNullOrWhiteSpace(ruleSetSummary.DisplayName))
                                {
                                    ruleSetSummary.DisplayName = ruleSetSummary.Name;
                                }

                                ruelSets.Add(ruleSetSummary);
                            }
                        }
                    }
                    catch (InvalidCastException)
                    {
                        System.Windows.Forms.MessageBox.Show("Error parsing table row.", "RuleSet Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Dispose();
                    }
                }
                cbRuleSetNames.ItemsSource = ruelSets;

                //To save selected value when user close then re-open the activity
                if (!string.IsNullOrWhiteSpace(ModelItem.GetInArgumentValue<string>(ruleSetNameProperty)))
                {
                    cbRuleSetNames.SelectedValue = ModelItem.GetInArgumentValue<string>(ruleSetNameProperty);
                }
            }
        }
    }
}
