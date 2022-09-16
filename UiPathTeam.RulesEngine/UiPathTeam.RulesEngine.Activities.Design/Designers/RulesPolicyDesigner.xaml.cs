namespace UiPathTeam.RulesEngine.Activities.Design.Designers
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;
    using System.Workflow.Activities.Rules;
    using System.Workflow.Activities.Rules.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;
    using UiPath.Shared.Activities.Design.Controls;
    using UiPath.Shared.Activities.Design.Services;
    using UiPathTeam.RulesEngine.RuleEditors;
    using MessageBox = System.Windows.MessageBox;

    /// <summary>
    /// Interaction logic for RulesPolicyDesigner.xaml
    /// </summary>
    public partial class RulesPolicyDesigner
    {
        private readonly string ruleSetNameProperty= "RuleSetName";
        private readonly string ruleFilePathProperty= "RulesFilePath";
        private readonly string targetObjectProperty= "TargetObject";

        public RulesPolicyDesigner()
        {
            InitializeComponent();
        }

        public void EditRuleSets_Click(object sender, RoutedEventArgs e)
        {

            string rulesFilePath = ModelItem.GetInArgumentValue<string>(ruleFilePathProperty);

            if (string.IsNullOrWhiteSpace(rulesFilePath))
            {
                System.Windows.MessageBox.Show("Rules file Path needs to be configured before viewing or editing the rules");
                return;
            }

            //object targetObject = ModelItem.GetInArgumentValue<object>(targetObjectProperty);
            //if (targetObject == null)
            //{
            //    System.Windows.MessageBox.Show("TargetObject needs to be configured before viewing or editing the rules");
            //    return;
            //}

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

            Type targetObjectType = targetObjArg.ArgumentType;
            string ruleSetName = ModelItem.GetInArgumentValue<string>(ruleSetNameProperty);

            CreateEmptyFileIfNotExists(rulesFilePath,ruleSetName);

            // popup the dialog for viewing the rules
            var ruleSetDialog = new RuleSetToolkitEditor();
            ruleSetDialog.LoadFile(rulesFilePath, targetObjectType);
            var result = ruleSetDialog.ShowDialog();
            if (result == DialogResult.OK) //If OK was pressed
            {
                //TODO:Refresh Activity
            }
        }

        private void CreateEmptyFileIfNotExists(string filePath,string ruleSetName)
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

        public void EditRules_Click(object sender, RoutedEventArgs e)
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

            //var targetObject = ModelItem.GetInArgumentValue<object>(targetObjectProperty);
            //if (targetObject == null)
            //{
            //    System.Windows.MessageBox.Show("TargetObject needs to be configured before viewing or editing the rules");
            //    return;
            //}

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

        private void filePathControl_FileSelected(object sender, RoutedEventArgs e)
        {
            var args = (FileSelectedRoutedEventArgs)e;
            if (args != null)
            {
                ModelItem.Properties[ruleFilePathProperty].SetValue(new InArgument<string>(args.Path));
                try
                {
                    using (Stream stream = new FileStream(args.Path, FileMode.Open))
                    {
                        using (XmlTextReader reader = new XmlTextReader(stream))
                        {
                            WorkflowMarkupSerializer ser = new WorkflowMarkupSerializer();
                            var ruleDefs = ser.Deserialize(reader) as RuleDefinitions;
                            RuleSetNameList.ItemsSource = ruleDefs.RuleSets.Select(x => x.Name).ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO:to handel a new file
                    RuleSetNameList.ItemsSource = new List<string>();
                }
            }
        }

        private void RuleSetNameList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RuleSetNameList.SelectedValue != null)
            {
                ModelItem.Properties[ruleSetNameProperty].SetValue(new InArgument<string>(RuleSetNameList.SelectedValue.ToString()));
            }
            else
            {
                ModelItem.Properties[ruleSetNameProperty].SetValue(new InArgument<string>(""));
            }
        }

        private void TweakRuleSetDialogToResizable(RuleSetDialog ruleSetDialog)
        {
            ruleSetDialog.FormBorderStyle = FormBorderStyle.Sizable;
            ruleSetDialog.HelpButton = false;
            ruleSetDialog.MaximizeBox = true;
            ruleSetDialog.Controls["okCancelTableLayoutPanel"].Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            ruleSetDialog.Controls["rulesGroupBox"].Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            ruleSetDialog.Controls["ruleGroupBox"].Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            ruleSetDialog.Controls["ruleGroupBox"].Controls["thenTextBox"].Anchor = AnchorStyles.Left |
                                                                                    AnchorStyles.Top |
                                                                                    AnchorStyles.Right | AnchorStyles.Bottom;
            ruleSetDialog.Controls["ruleGroupBox"].Controls["elseLabel"].Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            ruleSetDialog.Controls["ruleGroupBox"].Controls["elseTextBox"].Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            ruleSetDialog.Controls["ruleGroupBox"].Controls["conditionTextBox"].Anchor = AnchorStyles.Left |
                                                                                         AnchorStyles.Top |
                                                                                         AnchorStyles.Right;

            ruleSetDialog.Controls["rulesGroupBox"].Controls["panel1"].Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            ruleSetDialog.Controls["rulesGroupBox"].Controls["panel1"].Controls["chainingBehaviourComboBox"].Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ruleSetDialog.Controls["rulesGroupBox"].Controls["panel1"].Controls["chainingLabel"].Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
    }
}
