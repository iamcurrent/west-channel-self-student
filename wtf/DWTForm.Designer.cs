namespace wtf
{
    partial class DWTForm
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Stop_btn = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.ResolveText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.formsPlot1 = new ScottPlot.FormsPlot();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.Stop_btn);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.ResolveText);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(183, 646);
            this.panel1.TabIndex = 0;
            // 
            // Stop_btn
            // 
            this.Stop_btn.Location = new System.Drawing.Point(5, 150);
            this.Stop_btn.Name = "Stop_btn";
            this.Stop_btn.Size = new System.Drawing.Size(75, 23);
            this.Stop_btn.TabIndex = 1;
            this.Stop_btn.Text = "暂停";
            this.Stop_btn.UseVisualStyleBackColor = true;
            this.Stop_btn.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(90, 110);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "概貌";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(5, 110);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "细节";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // ResolveText
            // 
            this.ResolveText.Location = new System.Drawing.Point(5, 37);
            this.ResolveText.Name = "ResolveText";
            this.ResolveText.Size = new System.Drawing.Size(160, 21);
            this.ResolveText.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "分解层数";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.formsPlot1);
            this.panel2.Location = new System.Drawing.Point(201, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1163, 648);
            this.panel2.TabIndex = 1;
            // 
            // formsPlot1
            // 
            this.formsPlot1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.formsPlot1.AutoScroll = true;
            this.formsPlot1.AutoSize = true;
            this.formsPlot1.Location = new System.Drawing.Point(3, 3);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.formsPlot1.Size = new System.Drawing.Size(1157, 642);
            this.formsPlot1.TabIndex = 0;
            // 
            // DWTForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1377, 670);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "DWTForm";
            this.Text = "DWTForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DWTForm_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox ResolveText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private ScottPlot.FormsPlot formsPlot1;
        private System.Windows.Forms.Button Stop_btn;
    }
}