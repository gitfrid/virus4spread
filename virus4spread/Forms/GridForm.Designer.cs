namespace virus4spread.Forms
{
    partial class GridForm
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
            components = new System.ComponentModel.Container();
            Timer1 = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // Timer1
            // 
            Timer1.Tick += Timer1_Tick;
            // 
            // GridForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.Black;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1357, 863);
            DoubleBuffered = true;
            Margin = new Padding(5);
            Name = "GridForm";
            Text = "GridForm";
            Activated += GridForm_Activated;
            FormClosing += GridForm_FormClosing;
            Load += GridForm_Load;
            SizeChanged += GridForm_SizeChanged;
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Timer Timer1;
    }
}