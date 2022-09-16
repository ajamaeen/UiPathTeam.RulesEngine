

namespace UiPathTeam.RulesEngine.RuleEditors
{
    partial class RuleSetToolkitEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ruleSetNameCollectionLabel = new System.Windows.Forms.Label();
            this.ruleSetsGroupBox = new System.Windows.Forms.GroupBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.copyButton = new System.Windows.Forms.Button();
            this.ruleSetNameBox = new System.Windows.Forms.TextBox();
            this.ruleSetNameLabel = new System.Windows.Forms.Label();
            this.deleteButton = new System.Windows.Forms.Button();
            this.newButton = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.ruleSetsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ruleSetNameCollectionLabel
            // 
            this.ruleSetNameCollectionLabel.AutoSize = true;
            this.ruleSetNameCollectionLabel.Location = new System.Drawing.Point(30, 42);
            this.ruleSetNameCollectionLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.ruleSetNameCollectionLabel.Name = "ruleSetNameCollectionLabel";
            this.ruleSetNameCollectionLabel.Size = new System.Drawing.Size(242, 32);
            this.ruleSetNameCollectionLabel.TabIndex = 3;
            this.ruleSetNameCollectionLabel.Text = "RuleSet Versions:";
            // 
            // ruleSetsGroupBox
            // 
            this.ruleSetsGroupBox.Controls.Add(this.btnEdit);
            this.ruleSetsGroupBox.Controls.Add(this.treeView1);
            this.ruleSetsGroupBox.Controls.Add(this.copyButton);
            this.ruleSetsGroupBox.Controls.Add(this.ruleSetNameBox);
            this.ruleSetsGroupBox.Controls.Add(this.ruleSetNameLabel);
            this.ruleSetsGroupBox.Controls.Add(this.deleteButton);
            this.ruleSetsGroupBox.Controls.Add(this.newButton);
            this.ruleSetsGroupBox.Controls.Add(this.ruleSetNameCollectionLabel);
            this.ruleSetsGroupBox.Location = new System.Drawing.Point(30, 32);
            this.ruleSetsGroupBox.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.ruleSetsGroupBox.Name = "ruleSetsGroupBox";
            this.ruleSetsGroupBox.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.ruleSetsGroupBox.Size = new System.Drawing.Size(895, 863);
            this.ruleSetsGroupBox.TabIndex = 2;
            this.ruleSetsGroupBox.TabStop = false;
            this.ruleSetsGroupBox.Text = "RuleSets";
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(25, 85);
            this.treeView1.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(809, 506);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(442, 614);
            this.copyButton.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(188, 58);
            this.copyButton.TabIndex = 3;
            this.copyButton.Text = "&Copy";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // ruleSetNameBox
            // 
            this.ruleSetNameBox.Location = new System.Drawing.Point(270, 712);
            this.ruleSetNameBox.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.ruleSetNameBox.Name = "ruleSetNameBox";
            this.ruleSetNameBox.Size = new System.Drawing.Size(360, 39);
            this.ruleSetNameBox.TabIndex = 5;
            // 
            // ruleSetNameLabel
            // 
            this.ruleSetNameLabel.AutoSize = true;
            this.ruleSetNameLabel.Location = new System.Drawing.Point(30, 720);
            this.ruleSetNameLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.ruleSetNameLabel.Name = "ruleSetNameLabel";
            this.ruleSetNameLabel.Size = new System.Drawing.Size(206, 32);
            this.ruleSetNameLabel.TabIndex = 9;
            this.ruleSetNameLabel.Text = "RuleSet Name:";
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(242, 614);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(188, 58);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "&Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // newButton
            // 
            this.newButton.Location = new System.Drawing.Point(38, 614);
            this.newButton.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(188, 58);
            this.newButton.TabIndex = 1;
            this.newButton.Text = "&New";
            this.newButton.UseVisualStyleBackColor = true;
            this.newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(645, 614);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(8);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(188, 58);
            this.btnEdit.TabIndex = 10;
            this.btnEdit.Text = "&Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // RuleSetToolkitEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(240F, 240F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(977, 931);
            this.Controls.Add(this.ruleSetsGroupBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RuleSetToolkitEditor";
            this.Text = "RuleSet Browser";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ruleSetsGroupBox.ResumeLayout(false);
            this.ruleSetsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ruleSetNameCollectionLabel;
        private System.Windows.Forms.GroupBox ruleSetsGroupBox;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button newButton;
        private System.Windows.Forms.Label ruleSetNameLabel;
        private System.Windows.Forms.TextBox ruleSetNameBox;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btnEdit;
    }
}


