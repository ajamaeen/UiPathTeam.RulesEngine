using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Workflow.Activities.Rules;
using System.Workflow.Activities.Rules.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;

namespace UiPathTeam.RulesEngine.RuleEditors
{
    public partial class RuleSetToolkitEditor : Form
    {

        #region Variables and constructor

       

        private RuleSet selectedRuleSet;
        private List<RuleSet> ruleSetCollection = new List<RuleSet>();
        private Type targetObjectType;
        private string rulesFilePath;

        ToolStripMenuItem copyLabel = new ToolStripMenuItem();
        ToolStripMenuItem deleteLabel = new ToolStripMenuItem();
        ToolStripMenuItem editLabel = new ToolStripMenuItem();

        public RuleSetToolkitEditor()
        {
            InitializeComponent();
        }

        #endregion

        public void LoadFile(string filePath, Type targetObjectType)
        {
            this.targetObjectType = targetObjectType;
            this.rulesFilePath = filePath;
            LoadFile();
        }
        private void LoadFile()
        {
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
                    }
                    if (ruleDefs != null)
                        ruleSetCollection = ruleDefs.RuleSets.ToList();
                    else
                    {
                        ruleSetCollection = new List<RuleSet>();
                    }
                    this.BuildTree();

                    this.EnableApplicationFields(true);
                    this.EnableRuleSetFields(false);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }
       

        #region Form level
        
        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(RuleSetEditor_FormClosing);

            treeView1.TreeViewNodeSorter = new TreeSortClass() as IComparer;
            treeView1.HideSelection = false;

            // Create the ContextMenuStrip.
            var docMenu = new ContextMenuStrip();

            //Create some menu items.
            copyLabel.Text = "Copy";
            copyLabel.Click += CopyLabel_Click;
            
            deleteLabel.Text = "Delete";
            deleteLabel.Click += DeleteLabel_Click;

            editLabel.Text = "Edit";
            editLabel.Click += EditLabel_Click;

            //Add the menu items to the menu.
            docMenu.Items.AddRange(new ToolStripMenuItem[]{copyLabel,editLabel,deleteLabel});

            // Set the ContextMenuStrip property to the ContextMenuStrip.
            treeView1.ContextMenuStrip = docMenu;


            this.ruleSetNameBox.Validating += new System.ComponentModel.CancelEventHandler(ruleSetNameBox_Validating);

        }



        #region RuleSet actions

        private void CopyLabel_Click(object sender, EventArgs e)
        {
            copyRuleSet();
        }
        private void copyButton_Click(object sender, EventArgs e)
        {
            copyRuleSet();
        }

        private void copyRuleSet()
        {
            if (selectedRuleSet != null)
            {
                RuleSet newData = selectedRuleSet.Clone();
                var newName=GenerateRuleSetName($"{newData.Name} Copy");
                newData.Name = newName;
                this.AddRuleSet(newData);
            }
        }

        private void DeleteLabel_Click(object sender, EventArgs e)
        {
            deleteRuleSet();
        }
        private void deleteButton_Click(object sender, EventArgs e)
        {
            deleteRuleSet();
        }
        private void deleteRuleSet()
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedRuleSet != null)
            {
                DialogResult result = MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Do you want to delete the selected rule set?"), "RuleSet Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (result == DialogResult.Yes)
                {
                    ruleSetCollection.Remove(selectedRuleSet);
                    treeView1.Nodes.Remove(selectedNode);

                    selectedRuleSet = null;
                    this.SetSelectedNode(null);
                }
            }
        }

        

        private void EditLabel_Click(object sender, EventArgs e)
        {
            editRuleSet();
        }
        private void btnEdit_Click(object sender, EventArgs e)
        {
            editRuleSet();
        }
        private void editRuleSet()
        {
            // popup the dialog for viewing and editing the rules
            var ruleSetDialog = new RuleSetDialog(targetObjectType, null, selectedRuleSet);
            var result = ruleSetDialog.ShowDialog();
            if (result == DialogResult.OK) //If OK was pressed
            {
                var ruleSet = ruleSetCollection.First(x => x.Name == ruleSetDialog.RuleSet.Name);
                ruleSetCollection.Remove(ruleSet);
                ruleSetCollection.Add(ruleSetDialog.RuleSet);
            }
        }

        private TreeNode GetTreeNodeForRuleSet(RuleSet data)
        {
            if (data != null)
            {
                var nodes = treeView1.Nodes.Find(data.Name, true);
                if (nodes.Count() > 0)
                {
                    return nodes[0];
                }
                else
                {
                    //TODO:should not happen, throw error ?
                    return null;
                }

            }
            return null;
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            
            var ruleSetNameDialog=new RuleSetNameDialog();
            var result = ruleSetNameDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string newRuleSetName = ruleSetNameDialog.GetNewRuleSetName();

                if (String.IsNullOrEmpty(newRuleSetName))
                {
                    MessageBox.Show("RuleSet Name cannot be empty.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                RuleSet duplicateData;
                if (!this.IsDuplicateRuleSet(newRuleSetName, out duplicateData))
                {
                    RuleSet newData = this.CreateRuleSet(newRuleSetName);
                    this.AddRuleSet(newData);
                }
                else
                {
                    MessageBox.Show("A RuleSet with the same name already exists.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private RuleSet CreateRuleSet(string ruleSetName)
        {
            RuleSet data = new RuleSet();
            data.Name = ruleSetName;
            return data;
        }

        #endregion

      

        void RuleSetEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = ContinueRuleDefinitionsChange() == DialogResult.Cancel;
        }
        private DialogResult ContinueRuleDefinitionsChange(bool openEditRuleSet = false)
        {
            string msg = "Do you want to save the changes?";
            if (openEditRuleSet)
            {
                msg = "Do you want to save the changes before edit this rule set?";
            }

            DialogResult result = MessageBox.Show(string.Format(CultureInfo.InvariantCulture, msg), "RuleSet Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.Yes)
            {
                WorkflowMarkupSerializer ser = new WorkflowMarkupSerializer();
                RuleDefinitions ruleDefs = new RuleDefinitions();
                foreach (var rule in ruleSetCollection)
                {
                    ruleDefs.RuleSets.Add(rule);
                }
                using (var xmlTW = new System.Xml.XmlTextWriter(rulesFilePath, null))
                {
                    ser.Serialize(xmlTW, ruleDefs);
                }
            }
            else if (result == DialogResult.No)
            {
            }
            else //Cancel
            {
               
            }
            return result;
        }

        #endregion


        #region TreeView

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && ruleSetCollection.Any(x => x.Name == e.Node.Text))
            {
                selectedRuleSet = ruleSetCollection.First(x => x.Name == e.Node.Text);
                ruleSetNameBox.Text = selectedRuleSet.Name;
                this.EnableRuleSetFields(true);
            }
            else
            {
                selectedRuleSet = null;
                this.EnableRuleSetFields(false);
            }
        }

        private void SetSelectedNode(TreeNode node)
        {
            if (node != null)
            {
                treeView1.SelectedNode = node;
                this.treeView1_AfterSelect(this, new TreeViewEventArgs(node));
            }
            else
            {
                treeView1.SelectedNode = null;
                this.treeView1_AfterSelect(this, new TreeViewEventArgs(null));
            }
        }

        private TreeNode FindRuleSetNode(RuleSet data)
        {
            if (data != null)
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if (String.CompareOrdinal(node.Text, data.Name) == 0)
                        return node;
                }
            }
            return null;
        }

        private void EnableApplicationFields(bool enable)
        {
            newButton.Enabled = enable;
            ruleSetNameCollectionLabel.Enabled = enable;

            if (!enable)
                this.EnableRuleSetFields(enable);
        }

        private void EnableRuleSetFields(bool enable)
        {
            copyLabel.Enabled = enable;
            deleteLabel.Enabled= enable;
            editLabel.Enabled= enable;

            deleteButton.Enabled = enable;
            copyButton.Enabled = enable;
            btnEdit.Enabled = enable;

            ruleSetNameBox.Enabled = enable;
            ruleSetNameLabel.Enabled = enable;

            if (!enable)
                this.ClearRuleSetFields();
        }

        private void ClearRuleSetFields()
        {
            ruleSetNameBox.Text = "";
        }

        private void BuildTree()
        {
            treeView1.Nodes.Clear();
            foreach (RuleSet data in ruleSetCollection)
            {
                TreeNode newNode = new TreeNode(data.Name);
                treeView1.Nodes.Add(newNode);
            }
            treeView1.Sort();
        }
        #endregion


     

        #region Event handlers



        void ruleSetNameBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            if (selectedRuleSet != null)
            {
                if (String.IsNullOrEmpty(ruleSetNameBox.Text))
                {
                    MessageBox.Show("RuleSet Name cannot be empty.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ruleSetNameBox.Text = selectedRuleSet.Name;
                }
                else if (ruleSetNameBox.Text != selectedRuleSet.Name)
                {
                    RuleSet duplicateData;
                    if (!this.IsDuplicateRuleSet(ruleSetNameBox.Text, out duplicateData)
                        || duplicateData == selectedRuleSet)
                    {
                        selectedRuleSet.Name = ruleSetNameBox.Text;

                        this.BuildTree();
                        this.SetSelectedNode(this.GetTreeNodeForRuleSet(selectedRuleSet));
                    }
                    else
                    {
                        MessageBox.Show("A RuleSet with the same name already exists.", "RuleSet Property Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                    }
                }
            }
        }


        #endregion

        #region Other



        private void AddRuleSet(RuleSet RuleSet)
        {
            if (RuleSet != null)
            {
                TreeNode parentNode = this.FindRuleSetNode(RuleSet);

                if (parentNode == null)
                {
                    ruleSetCollection.Add(RuleSet);
                    parentNode = new TreeNode(RuleSet.Name);
                    treeView1.Nodes.Add(parentNode);
                }
                treeView1.Sort();
                this.SetSelectedNode(this.GetTreeNodeForRuleSet(RuleSet));
            }
        }

        private bool IsDuplicateRuleSet(string name, out RuleSet duplicateRuleSet)
        {
            foreach (RuleSet data in ruleSetCollection)
            {
                if (String.CompareOrdinal(data.Name, name) == 0)
                {
                    duplicateRuleSet = data;
                    return true;
                }
            }
            duplicateRuleSet = null;
            return false;
        }

        private string GenerateRuleSetName(string template)
        {
            string newName = "";
            bool uniqueNameNotFound = true;
            int counter = 0;

            while (uniqueNameNotFound)
            {
                counter++;
                uniqueNameNotFound = false;
                newName = template + counter.ToString(CultureInfo.InvariantCulture);
                uniqueNameNotFound = this.IsDuplicateRuleSetName(newName);
            }

            return newName;
        }

        private bool IsDuplicateRuleSetName(string name)
        {
            foreach (RuleSet data in ruleSetCollection)
            {
                if (String.CompareOrdinal(data.Name, name) == 0)
                {
                    return true;
                }
            }
            return false;
        }



        #endregion

     
    }


    internal class TreeSortClass : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            TreeNode xNode = x as TreeNode;
            TreeNode yNode = y as TreeNode;


            return String.CompareOrdinal(xNode.Text, yNode.Text);
        }
    }
}
