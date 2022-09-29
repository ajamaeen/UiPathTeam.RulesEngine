using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;
using System.Xml.Linq;
using UiPath.Shared.Activities.Design.Services;

namespace UiPathTeam.RulesEngine.Activities.Design.Dialogs
{
    // Interaction logic for RuleSetEditorDialog.xaml
    public partial class RuleSetEditorDialog : WorkflowElementDialog
    {
        private Dictionary<TreeViewItem, RuleSetData> ruleSetDataDictionary = new Dictionary<TreeViewItem, RuleSetData>();
        private bool dirty; //indicates if any RuleSetData has been modified
        private readonly string rulesFilePath;
        private RuleSetData selectedRuleSetData;
        private List<RuleSetData> deletedRuleSetDataCollection = new List<RuleSetData>();
        private readonly int maxMinorVersions = 100;
        private readonly int maxMajorVersions = 1000;
        private readonly WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();

        public RuleSetEditorDialog(ModelItem modelItem)
        {
            InitializeComponent();
            ModelItem = modelItem;
            Context = modelItem.GetEditingContext();
            rulesFilePath = ModelItem.GetInArgumentValue<string>("RulesFilePath");
            InitializeData();
        }

        public RuleSetEditorDialog(string rulesFilePath)
        {
            InitializeComponent();
            this.rulesFilePath = rulesFilePath;
            InitializeData();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            RuleSetData newData = this.CreateRuleSetData(null);
            this.AddRuleSetData(newData);
        }

        private void AddRuleSetData(RuleSetData ruleSetData)
        {
            if (ruleSetData != null)
            {
                TreeViewItem parentNode = this.FindParentNode(ruleSetData);

                if (parentNode == null)
                {
                    parentNode = new TreeViewItem();
                    parentNode.Header = ruleSetData.Name;

                }

                TreeViewItem newVersionNode = new TreeViewItem();
                newVersionNode.Header = VersionTreeNodeText(ruleSetData.MajorVersion, ruleSetData.MinorVersion);

                parentNode.Items.Add(newVersionNode);                
                //treeView1.Sort();                
                TreeRuleSets.Items.Add(parentNode);
                ruleSetDataDictionary.Add(newVersionNode, ruleSetData);
                //TreeRuleSets.Items.Clear();
                //TreeRuleSets.ItemsSource = ruleSetDataDictionary;

                this.SetSelectedNode(newVersionNode);
            }

            //if (ruleSetData != null)
            //{
            //    TreeViewItem parentTreeViewNode = this.FindParentNode(ruleSetData);

            //    if (parentTreeViewNode == null)
            //    {
            //        parentTreeViewNode = new TreeViewItem();
            //        parentTreeViewNode.Header = new TreeNode(ruleSetData.Name);
            //        TreeRuleSets.Items.Add(parentTreeViewNode);
            //    }

            //    TreeNode newVersionNode = new TreeNode(VersionTreeNodeText(ruleSetData.MajorVersion, ruleSetData.MinorVersion));
            //    var treeNode = (TreeNode)parentTreeViewNode.Header;
            //    treeNode.Nodes.Add(newVersionNode);

            //    //treeView1.Sort();
            //    ruleSetDataDictionary.Add(newVersionNode, ruleSetData);
            //    this.SetSelectedNode(newVersionNode);
            //    parentTreeViewNode.IsSelected = true;
            //    parentTreeViewNode.IsExpanded = true;
            //}
        }

        private void SetSelectedNode(TreeViewItem node)
        {
            if (node != null && node.Parent != null)
            {
                var treeViewItem = TreeRuleSets.ItemContainerGenerator.ContainerFromItem(node.Parent) as TreeViewItem;
                if (treeViewItem != null)
                {
                    treeViewItem.IsExpanded = true;
                    treeViewItem.IsSelected = true;
                }
                //this.treeView1_AfterSelect(this, new TreeViewEventArgs(node));
            }
            else
            {
                //treeView1.SelectedNode = null;
                //this.treeView1_AfterSelect(this, new TreeViewEventArgs(null));
            }
        }

        private static string VersionTreeNodeText(int majorVersion, int minorVersion)
        {
            return String.Format(CultureInfo.InvariantCulture, "Version {0}.{1}", majorVersion, minorVersion);
        }

        private void EnableRuleSetFields(bool enable)
        {
            //editButton.Enabled = enable;
            //deleteButton.Enabled = enable;
            //copyButton.Enabled = enable;
            txtRuleSetName.IsEnabled = lblRuleSetName.IsEnabled = enable;
            txtMajorVersion.IsEnabled = lblMajorVersion.IsEnabled = enable;
            txtMinorVersion.IsEnabled = lblMinroVersion.IsEnabled = enable;
            //getActivityButton.Enabled = enable;
            //selectedActivityLabel.Enabled = enable;
            //membersLabel.Enabled = enable;
            //validateToolStripMenuItem.Enabled = enable;

            if (!enable)
                this.ClearRuleSetFields();
        }

        private void ClearRuleSetFields()
        {
            txtRuleSetName.Text = "";
            txtMajorVersion.Text = "0";
            txtMinorVersion.Text = "0";
            //activityBox.Text = "";
            //membersBox.Items.Clear();
        }

        private TreeViewItem FindParentNode(RuleSetData data)
        {
            if (data != null)
            {
                foreach (TreeViewItem node in TreeRuleSets.Items)
                {
                    if (String.CompareOrdinal(node.Header.ToString(), data.Name) == 0)
                        return node;
                }
            }
            return null;
        }

        //private TreeViewItem FindParentNode(RuleSetData data)
        //{
        //    if (data != null)
        //    {
        //        foreach (TreeViewItem node in TreeRuleSets.Items)
        //        {
        //            var treeHead = (TreeNode)node.Header;
        //            if (treeHead != null)
        //            {
        //                if (String.CompareOrdinal(treeHead.Text, data.Name) == 0)
        //                    return node;
        //            }
        //        }
        //    }
        //    return null;
        //}

        private RuleSetData CreateRuleSetData(RuleSet ruleSet)
        {
            RuleSetData data = new RuleSetData();
            if (ruleSet != null)
            {
                data.Name = ruleSet.Name;
                data.RuleSet = ruleSet;
            }
            else
            {
                data.Name = this.GenerateRuleSetName();
                data.RuleSet = new RuleSet(data.Name);
            }
            data.MajorVersion = 1;
            this.MarkDirty(data);
            return data;
        }

        private string GenerateRuleSetName()
        {
            string namePrefix = "RuleSet";
            string newName = "";
            bool uniqueNameNotFound = true;
            int counter = 0;

            while (uniqueNameNotFound)
            {
                counter++;
                uniqueNameNotFound = false;
                newName = namePrefix + counter.ToString(CultureInfo.InvariantCulture);
                uniqueNameNotFound = this.IsDuplicateRuleSetName(newName);
            }

            return newName;
        }

        private bool IsDuplicateRuleSetName(string name)
        {
            foreach (RuleSetData data in ruleSetDataDictionary.Values)
            {
                if (String.CompareOrdinal(data.Name, name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void MarkDirty(RuleSetData data)
        {
            if (data != null)
                data.Dirty = true;

            dirty = true;
        }

        private void TreeRuleSets_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RuleSetData data;
            var node = (TreeViewItem)e.NewValue;
            if (node != null && ruleSetDataDictionary.TryGetValue(node, out data))
            {
                selectedRuleSetData = data;
                //assemblyPath = selectedRuleSetData.AssemblyPath;
                txtRuleSetName.Text = selectedRuleSetData.Name;
                txtMajorVersion.Text = selectedRuleSetData.MajorVersion.ToString();
                txtMinorVersion.Text = selectedRuleSetData.MinorVersion.ToString();
                //activityBox.Text = selectedRuleSetData.ActivityName;
                //if (selectedRuleSetData.Activity == null)
                //    this.LoadAssemblyAndActivity();

                //this.PopulateMembers();

                this.EnableRuleSetFields(true);
            }
            else
            {
                selectedRuleSetData = null;
                //assemblyPath = null;
                this.EnableRuleSetFields(false);
            }
        }

        private bool IsDuplicateRuleSet(string name, int majorVersion, int minorVersion, out RuleSetData duplicateRuleSetData)
        {
            foreach (RuleSetData data in ruleSetDataDictionary.Values)
            {
                if (String.CompareOrdinal(data.Name, name) == 0 && data.MajorVersion == majorVersion && data.MinorVersion == minorVersion)
                {
                    duplicateRuleSetData = data;
                    return true;
                }
            }
            duplicateRuleSetData = null;
            return false;
        }

        private void BuildTree(List<RuleSetData> ruleSetDataCollection)
        {
            ruleSetDataCollection.Sort();
            ruleSetDataDictionary.Clear();
            TreeRuleSets.Items.Clear();
            //treeView1.Nodes.Clear();
            RuleSetData lastData = null;
            TreeViewItem lastRuleSetNameNode = null;
            foreach (RuleSetData data in ruleSetDataCollection)
            {
                if (lastData == null || lastData.Name != data.Name) //new ruleset name
                {
                    TreeViewItem newNode = new TreeViewItem();
                    newNode.Header = data.Name;

                    TreeRuleSets.Items.Add(newNode);
                    lastRuleSetNameNode = newNode;
                }

                TreeViewItem newVersionNode = new TreeViewItem();
                newVersionNode.Header = VersionTreeNodeText(data.MajorVersion, data.MinorVersion);

                lastRuleSetNameNode.Items.Add(newVersionNode);
                ruleSetDataDictionary.Add(newVersionNode, data);
                lastData = data;
            }
            //treeView1.Sort();
        }

        private void txtRuleSetName_LostFocus(object sender, RoutedEventArgs e)
        {
            //e.Cancel = false;
            if (selectedRuleSetData != null)
            {
                if (String.IsNullOrEmpty(txtRuleSetName.Text))
                {
                    System.Windows.Forms.MessageBox.Show("RuleSet Name cannot be empty.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtRuleSetName.Text = selectedRuleSetData.Name;
                }
                else if (txtRuleSetName.Text != selectedRuleSetData.Name)
                {
                    RuleSetData duplicateData;
                    if (!this.IsDuplicateRuleSet(txtRuleSetName.Text, selectedRuleSetData.MajorVersion, selectedRuleSetData.MinorVersion, out duplicateData)
                        || duplicateData == selectedRuleSetData)
                    {
                        selectedRuleSetData.Name = txtRuleSetName.Text;
                      
                        this.MarkDirty(selectedRuleSetData);

                        List<RuleSetData> ruleSetDataCollection = new List<RuleSetData>();
                        foreach (RuleSetData data in ruleSetDataDictionary.Values)
                            ruleSetDataCollection.Add(data);

                        this.BuildTree(ruleSetDataCollection);
                        this.SetSelectedNode(this.GetTreeNodeForRuleSetData(selectedRuleSetData));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("A RuleSet with the same name and version numbers already exists.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //e.Cancel = true;
                    }
                }
            }
        }

        private TreeViewItem GetTreeNodeForRuleSetData(RuleSetData data)
        {
            if (data != null)
            {
                Dictionary<TreeViewItem, RuleSetData>.Enumerator enumerator = ruleSetDataDictionary.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    RuleSetData otherData = enumerator.Current.Value;
                    if (String.CompareOrdinal(otherData.Name, data.Name) == 0 && otherData.MajorVersion == data.MajorVersion && otherData.MinorVersion == data.MinorVersion)
                        return enumerator.Current.Key;
                }
            }
            return null;
        }

        private void txtMajorVersion_LostFocus(object sender, RoutedEventArgs e)
        {
            ///e.Cancel = false;
            int majorInt = Convert.ToInt32(txtMajorVersion.Text);
            if (selectedRuleSetData != null && TreeRuleSets.SelectedItem != null && majorInt != selectedRuleSetData.MajorVersion)
            {
                if (majorInt > 0)
                {
                    RuleSetData duplicateData;
                    if (!this.IsDuplicateRuleSet(selectedRuleSetData.Name, Convert.ToInt32(txtMajorVersion.Text), selectedRuleSetData.MinorVersion, out duplicateData)
                        || duplicateData == selectedRuleSetData)
                    {
                        selectedRuleSetData.MajorVersion = majorInt;
                        this.MarkDirty(selectedRuleSetData);

                        TreeViewItem selectedNode = (TreeViewItem)TreeRuleSets.SelectedItem;
                        selectedNode.Header = VersionTreeNodeText(selectedRuleSetData.MajorVersion, selectedRuleSetData.MinorVersion);
                        //treeView1.Sort();
                        this.SetSelectedNode(selectedNode);
                        TreeRuleSets.Items.Refresh();
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("A RuleSet with the same name and version numbers already exists.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //e.Cancel = true;
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Major version number must be greater than 0", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //e.Cancel = true;
                }
            }
        }

        private void txtMinorVersion_LostFocus(object sender, RoutedEventArgs e)
        {
            //e.Cancel = false;
            int minorInt = Convert.ToInt32(txtMinorVersion.Text);
            if (selectedRuleSetData != null && TreeRuleSets.SelectedItem != null && minorInt != selectedRuleSetData.MinorVersion)
            {
                RuleSetData duplicateData;
                if (!this.IsDuplicateRuleSet(selectedRuleSetData.Name, selectedRuleSetData.MajorVersion, Convert.ToInt32(txtMinorVersion.Text), out duplicateData)
                    || duplicateData == selectedRuleSetData)
                {
                    selectedRuleSetData.MinorVersion = minorInt;
                    this.MarkDirty(selectedRuleSetData);

                    TreeViewItem selectedNode = (TreeViewItem)TreeRuleSets.SelectedItem;
                    selectedNode.Header = VersionTreeNodeText(selectedRuleSetData.MajorVersion, selectedRuleSetData.MinorVersion);
                    //this.treeView1.Sort();
                    this.SetSelectedNode(selectedNode);
                    TreeRuleSets.Items.Refresh();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("A RuleSet with the same name and version numbers already exists.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //e.Cancel = true;
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedNode = (TreeViewItem)TreeRuleSets.SelectedItem;
            TreeViewItem parentNode = selectedNode.Parent as TreeViewItem;

            if (IsVersionNode(selectedNode) && selectedRuleSetData != null)
            {
                deletedRuleSetDataCollection.Add(selectedRuleSetData);
                this.MarkDirty(selectedRuleSetData);

                ruleSetDataDictionary.Remove(selectedNode);
                parentNode.Items.Remove(selectedNode);

                //if this was the only version node, remove the ruleset name node
                if (parentNode.Items.Count == 0)
                {
                    TreeRuleSets.Items.Remove(parentNode);
                }
                //selectedRuleSetData = null;
                //assemblyPath = null;
                this.SetSelectedNode(null);
            }
        }

        private static bool IsVersionNode(TreeViewItem node)
        {
            if (node != null)
                return node.Header.ToString().StartsWith("Version", StringComparison.Ordinal);
            else
                return false;
        }

        private void WorkflowElementDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtMajorVersion.MaxLength = maxMajorVersions;
            txtMinorVersion.MaxLength = maxMinorVersions;
            //TreeRuleSets.TreeViewNodeSorter = new TreeSortClass() as IComparer;
            //treeView1.HideSelection = false;
        }

        private bool ContinueRuleDefinitionsChange()
        {
            bool continueResult = true;

            if (dirty)
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Do you want to save the changes?"),
                    "RuleSet Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                {
                    SaveToFile();
                }
                else if (result == DialogResult.No)
                {
                }
                else //Cancel
                {
                    continueResult = false;
                }
            }
            return continueResult;
        }

        protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
        {
            //ContinueRuleDefinitionsChange()
            //Update parent controls
            base.OnWorkflowElementDialogClosed(dialogResult);
        }

        private void SaveToFile()
        {
            if (ruleSetDataDictionary != null)
            {
                try
                {
                    WorkflowMarkupSerializer ser = new WorkflowMarkupSerializer();
                    RuleDefinitions ruleDefs = new RuleDefinitions();
                    foreach (var rule in ruleSetDataDictionary.Values)
                    {
                        rule.RuleSet.Name = $"{rule.Name}-{rule.MajorVersion}-{rule.MinorVersion}";
                        ruleDefs.RuleSets.Add(rule.RuleSet);
                    }
                    using (var xmlTW = new System.Xml.XmlTextWriter(rulesFilePath, null))
                    {
                        ser.Serialize(xmlTW, ruleDefs);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    //// File does not have write access. Make a local copy so user changes are not lost
                    //FileInfo fileInfo = new FileInfo(rulesFilePath);
                    //// create local file by adding a random suffix to original filename
                    //string localFileCopy = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.')) + new Random().Next() + fileInfo.Extension;
                    //ser.Serialize(new System.Xml.XmlTextWriter((string)localFileCopy, null), ruleDefs);
                    System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.InvariantCulture, $"Error saving RuleSets to {rulesFilePath}. \r\n\n", ex.Message), "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //System.Windows.MessageBox.Show("Rules file is not writeable. Created copy of your changes in " + localFileCopy);
                }
                InitializeData();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("RuleSet collection is empty.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private string SerializeRuleSet(RuleSet ruleSet)
        {
            StringBuilder ruleDefinition = new StringBuilder();

            if (ruleSet != null)
            {
                try
                {
                    StringWriter stringWriter = new StringWriter(ruleDefinition, CultureInfo.InvariantCulture);
                    XmlTextWriter writer = new XmlTextWriter(stringWriter);
                    serializer.Serialize(writer, ruleSet);
                    writer.Flush();
                    writer.Close();
                    stringWriter.Flush();
                    stringWriter.Close();
                }
                catch (Exception ex)
                {
                    if (selectedRuleSetData != null)
                        System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Error serializing RuleSet: '{0}'. \r\n\n{1}", selectedRuleSetData.Name, ex.Message), "Serialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Error serializing RuleSet. \r\n\n{0}", ex.Message), "Serialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (selectedRuleSetData != null)
                    System.Windows.Forms.MessageBox.Show(String.Format(CultureInfo.InvariantCulture, "Error serializing RuleSet: '{0}'.", selectedRuleSetData.Name), "Serialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    System.Windows.Forms.MessageBox.Show("Error serializing RuleSet.", "Serialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ruleDefinition.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ContinueRuleDefinitionsChange();
        }

        private void InitializeData()
        {
            selectedRuleSetData = null;
            List<RuleSetData> ruleSetDataCollection = this.GetRuleSets();



            this.BuildTree(ruleSetDataCollection);

            this.EnableApplicationFields(true);
            this.EnableRuleSetFields(false);
        }

        private void EnableApplicationFields(bool enable)
        {
            btnNew.IsEnabled = enable;
            //ruleSetNameCollectionLabel.Enabled = enable;
            if (!enable)
            {
                this.EnableRuleSetFields(enable);
            }
        }

        private List<RuleSetData> GetRuleSets()
        {
            List<RuleSetData> ruleSetDataCollection = new List<RuleSetData>();
            dirty = false;

            using (Stream stream = new FileStream(rulesFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                            RuleSetData data = new RuleSetData();

                            string[] nameSplitted = ruleset.Name.Split('-');
                            if (nameSplitted.Length == 3)
                            {
                                data.Name = nameSplitted[0];
                                if(int.TryParse(nameSplitted[1], out int majorVersion))
                                {
                                    data.MajorVersion = majorVersion;
                                }

                                if (int.TryParse(nameSplitted[2], out int minorVersion))
                                {
                                    data.MinorVersion = minorVersion;
                                }
                            }
                            else
                            {
                                data.Name = ruleset.Name;
                            }
                            data.OriginalName = data.Name; // will be used later to see if one of these key values changed                                                  
                            data.OriginalMajorVersion = data.MajorVersion;                    
                            data.OriginalMinorVersion = data.MinorVersion;
                            data.RuleSetDefinition = SerializeRuleSet(ruleset);
                            //data.Status = reader.GetInt16(4);
                            //data.AssemblyPath = reader.GetString(5);
                            //data.ActivityName = reader.GetString(6);
                            //data.ModifiedDate = reader.GetDateTime(7);
                            data.RuleSet = ruleset;
                            data.Dirty = false;
                            ruleSetDataCollection.Add(data);
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
            return ruleSetDataCollection;
        }
    }
}
