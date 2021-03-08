namespace Model63200ESeries
{
    partial class OptControl
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
            this.startP = new System.Windows.Forms.TextBox();
            this.inter = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.endP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.s_inter = new System.Windows.Forms.TextBox();
            this.btn_chanage2 = new System.Windows.Forms.Button();
            this.btn_changecs = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.LoopNum = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.Clear = new System.Windows.Forms.Button();
            this.AnyVal = new System.Windows.Forms.Button();
            this.ucNumTextBox1 = new HZH_Controls.Controls.UCNumTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // startP
            // 
            this.startP.Location = new System.Drawing.Point(87, 12);
            this.startP.Name = "startP";
            this.startP.Size = new System.Drawing.Size(146, 21);
            this.startP.TabIndex = 0;
            // 
            // inter
            // 
            this.inter.Location = new System.Drawing.Point(87, 66);
            this.inter.Name = "inter";
            this.inter.Size = new System.Drawing.Size(146, 21);
            this.inter.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(272, 44);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "正向变化";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(272, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "反向变化";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // endP
            // 
            this.endP.Location = new System.Drawing.Point(87, 39);
            this.endP.Name = "endP";
            this.endP.Size = new System.Drawing.Size(146, 21);
            this.endP.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 23);
            this.label1.TabIndex = 5;
            this.label1.Text = "起点";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 18);
            this.label2.TabIndex = 6;
            this.label2.Text = "变化间隔";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "终点";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "指令间隔(ms)";
            // 
            // s_inter
            // 
            this.s_inter.Location = new System.Drawing.Point(95, 95);
            this.s_inter.Name = "s_inter";
            this.s_inter.Size = new System.Drawing.Size(146, 21);
            this.s_inter.TabIndex = 9;
            // 
            // btn_chanage2
            // 
            this.btn_chanage2.Location = new System.Drawing.Point(272, 73);
            this.btn_chanage2.Name = "btn_chanage2";
            this.btn_chanage2.Size = new System.Drawing.Size(75, 23);
            this.btn_chanage2.TabIndex = 10;
            this.btn_chanage2.Text = "2的次幂变化";
            this.btn_chanage2.UseVisualStyleBackColor = true;
            this.btn_chanage2.Click += new System.EventHandler(this.btn_chanage2_Click);
            // 
            // btn_changecs
            // 
            this.btn_changecs.Location = new System.Drawing.Point(272, 102);
            this.btn_changecs.Name = "btn_changecs";
            this.btn_changecs.Size = new System.Drawing.Size(75, 23);
            this.btn_changecs.TabIndex = 11;
            this.btn_changecs.Text = "正弦变化";
            this.btn_changecs.UseVisualStyleBackColor = true;
            this.btn_changecs.Click += new System.EventHandler(this.btn_changecs_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(14, 135);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(215, 122);
            this.richTextBox1.TabIndex = 12;
            this.richTextBox1.Text = "";
            // 
            // LoopNum
            // 
            this.LoopNum.Location = new System.Drawing.Point(294, 186);
            this.LoopNum.Name = "LoopNum";
            this.LoopNum.Size = new System.Drawing.Size(100, 21);
            this.LoopNum.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(235, 189);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "重复次数";
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(235, 234);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(75, 23);
            this.Clear.TabIndex = 15;
            this.Clear.Text = "清空";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // AnyVal
            // 
            this.AnyVal.Location = new System.Drawing.Point(272, 135);
            this.AnyVal.Name = "AnyVal";
            this.AnyVal.Size = new System.Drawing.Size(75, 22);
            this.AnyVal.TabIndex = 16;
            this.AnyVal.Text = "随机变换";
            this.AnyVal.UseVisualStyleBackColor = true;
            this.AnyVal.Click += new System.EventHandler(this.AnyVal_Click);
            // 
            // ucNumTextBox1
            // 
            this.ucNumTextBox1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ucNumTextBox1.InputType = HZH_Controls.TextInputType.Number;
            this.ucNumTextBox1.IsNumCanInput = true;
            this.ucNumTextBox1.KeyBoardType = HZH_Controls.Controls.KeyBoardType.UCKeyBorderNum;
            this.ucNumTextBox1.Location = new System.Drawing.Point(77, 263);
            this.ucNumTextBox1.MaxValue = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.ucNumTextBox1.MinValue = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.ucNumTextBox1.Name = "ucNumTextBox1";
            this.ucNumTextBox1.Num = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.ucNumTextBox1.Padding = new System.Windows.Forms.Padding(2);
            this.ucNumTextBox1.Size = new System.Drawing.Size(152, 44);
            this.ucNumTextBox1.TabIndex = 21;
            this.ucNumTextBox1.NumChanged += new System.EventHandler(this.ucNumTextBox1_NumChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 280);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 22;
            this.label6.Text = "顺序实验";
            // 
            // OptControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 311);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ucNumTextBox1);
            this.Controls.Add(this.AnyVal);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.LoopNum);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btn_changecs);
            this.Controls.Add(this.btn_chanage2);
            this.Controls.Add(this.s_inter);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.endP);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.inter);
            this.Controls.Add(this.startP);
            this.Name = "OptControl";
            this.Text = "OptControl";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptControl_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox startP;
        private System.Windows.Forms.TextBox inter;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox endP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox s_inter;
        private System.Windows.Forms.Button btn_chanage2;
        private System.Windows.Forms.Button btn_changecs;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox LoopNum;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button Clear;
        private System.Windows.Forms.Button AnyVal;
        private HZH_Controls.Controls.UCNumTextBox ucNumTextBox1;
        private System.Windows.Forms.Label label6;
    }
}