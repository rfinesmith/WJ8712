namespace TestWJcomms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitter1 = new Splitter();
            textBox1 = new TextBox();
            rtfTerminal = new RichTextBox();
            SuspendLayout();
            // 
            // splitter1
            // 
            splitter1.Location = new Point(0, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(3, 450);
            splitter1.TabIndex = 0;
            splitter1.TabStop = false;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(23, 218);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(253, 23);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // rtfTerminal
            // 
            rtfTerminal.BackColor = SystemColors.InactiveCaption;
            rtfTerminal.Location = new Point(23, 299);
            rtfTerminal.Name = "rtfTerminal";
            rtfTerminal.Size = new Size(286, 96);
            rtfTerminal.TabIndex = 2;
            rtfTerminal.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(rtfTerminal);
            Controls.Add(textBox1);
            Controls.Add(splitter1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Splitter splitter1;
        private TextBox textBox1;
        private RichTextBox rtfTerminal;
    }
}
