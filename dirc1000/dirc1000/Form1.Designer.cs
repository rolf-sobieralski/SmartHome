namespace dirc1000
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cb_bereiche = new System.Windows.Forms.ComboBox();
            this.but_neue_ebene = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.placeableImageBox1 = new TilingImageBox.PlaceableImageBox();
            this.ImageBox_ContentMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.bildLadenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Smart_Client_Commands = new System.Windows.Forms.DataGridView();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.Smart_Client_Infos = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.CommandList = new System.Windows.Forms.BindingSource(this.components);
            this.akt_smart_client = new System.Windows.Forms.BindingSource(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.OpenHGImage = new System.Windows.Forms.OpenFileDialog();
            this.Ebenen_Eingabe = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.ImageBox_ContentMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Smart_Client_Commands)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Smart_Client_Infos)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CommandList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.akt_smart_client)).BeginInit();
            this.Ebenen_Eingabe.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "start server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(87, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "stop server";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(168, 6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "senden";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(249, 8);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(422, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.Visible = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(6, 6);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(417, 503);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(6, 35);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(156, 446);
            this.listBox1.TabIndex = 5;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDoubleClick);
            this.listBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1232, 541);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cb_bereiche);
            this.tabPage1.Controls.Add(this.but_neue_ebene);
            this.tabPage1.Controls.Add(this.checkBox1);
            this.tabPage1.Controls.Add(this.placeableImageBox1);
            this.tabPage1.Controls.Add(this.Smart_Client_Commands);
            this.tabPage1.Controls.Add(this.button5);
            this.tabPage1.Controls.Add(this.button4);
            this.tabPage1.Controls.Add(this.Smart_Client_Infos);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.listBox1);
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.button3);
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1224, 515);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Übersicht";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cb_bereiche
            // 
            this.cb_bereiche.FormattingEnabled = true;
            this.cb_bereiche.Location = new System.Drawing.Point(817, 6);
            this.cb_bereiche.Name = "cb_bereiche";
            this.cb_bereiche.Size = new System.Drawing.Size(220, 21);
            this.cb_bereiche.TabIndex = 14;
            this.cb_bereiche.SelectedIndexChanged += new System.EventHandler(this.cb_bereiche_SelectedIndexChanged);
            // 
            // but_neue_ebene
            // 
            this.but_neue_ebene.Location = new System.Drawing.Point(699, 6);
            this.but_neue_ebene.Name = "but_neue_ebene";
            this.but_neue_ebene.Size = new System.Drawing.Size(111, 23);
            this.but_neue_ebene.TabIndex = 13;
            this.but_neue_ebene.Text = "Bereich hinzufügen";
            this.but_neue_ebene.UseVisualStyleBackColor = true;
            this.but_neue_ebene.MouseClick += new System.Windows.Forms.MouseEventHandler(this.but_neue_ebene_MouseClick);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 487);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(102, 17);
            this.checkBox1.TabIndex = 12;
            this.checkBox1.Text = "nur neue Clients";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // placeableImageBox1
            // 
            this.placeableImageBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.placeableImageBox1.ContextMenuStrip = this.ImageBox_ContentMenu;
            this.placeableImageBox1.Location = new System.Drawing.Point(169, 37);
            this.placeableImageBox1.Name = "placeableImageBox1";
            this.placeableImageBox1.Size = new System.Drawing.Size(768, 470);
            this.placeableImageBox1.TabIndex = 11;
            this.placeableImageBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.placeableImageBox1_MouseDown);
            this.placeableImageBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // ImageBox_ContentMenu
            // 
            this.ImageBox_ContentMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bildLadenToolStripMenuItem});
            this.ImageBox_ContentMenu.Name = "ImageBox_ContentMenu";
            this.ImageBox_ContentMenu.Size = new System.Drawing.Size(127, 26);
            // 
            // bildLadenToolStripMenuItem
            // 
            this.bildLadenToolStripMenuItem.Name = "bildLadenToolStripMenuItem";
            this.bildLadenToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.bildLadenToolStripMenuItem.Text = "Bild laden";
            this.bildLadenToolStripMenuItem.Click += new System.EventHandler(this.bildLadenToolStripMenuItem_Click);
            // 
            // Smart_Client_Commands
            // 
            this.Smart_Client_Commands.AllowUserToAddRows = false;
            this.Smart_Client_Commands.AllowUserToDeleteRows = false;
            this.Smart_Client_Commands.AllowUserToResizeColumns = false;
            this.Smart_Client_Commands.AllowUserToResizeRows = false;
            this.Smart_Client_Commands.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Smart_Client_Commands.Location = new System.Drawing.Point(944, 268);
            this.Smart_Client_Commands.Name = "Smart_Client_Commands";
            this.Smart_Client_Commands.Size = new System.Drawing.Size(274, 239);
            this.Smart_Client_Commands.TabIndex = 10;
            this.Smart_Client_Commands.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.Smart_Client_Commands_CellBeginEdit);
            this.Smart_Client_Commands.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Smart_Client_Commands_CellClick);
            this.Smart_Client_Commands.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.Smart_Client_Commands_CellEndEdit);
            this.Smart_Client_Commands.SelectionChanged += new System.EventHandler(this.Smart_Client_Commands_SelectionChanged);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(1117, 239);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(101, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "entfernen";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(944, 239);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(107, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "hinzufügen";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Smart_Client_Infos
            // 
            this.Smart_Client_Infos.AllowUserToAddRows = false;
            this.Smart_Client_Infos.AllowUserToDeleteRows = false;
            this.Smart_Client_Infos.AllowUserToResizeColumns = false;
            this.Smart_Client_Infos.AllowUserToResizeRows = false;
            this.Smart_Client_Infos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Smart_Client_Infos.Location = new System.Drawing.Point(943, 37);
            this.Smart_Client_Infos.Name = "Smart_Client_Infos";
            this.Smart_Client_Infos.Size = new System.Drawing.Size(275, 196);
            this.Smart_Client_Infos.TabIndex = 6;
            this.Smart_Client_Infos.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.Smart_Client_Infos_CellBeginEdit);
            this.Smart_Client_Infos.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.Smart_Client_Infos_CellEndEdit);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listBox2);
            this.tabPage2.Controls.Add(this.comboBox1);
            this.tabPage2.Controls.Add(this.richTextBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1224, 515);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Debug";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(430, 34);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(788, 472);
            this.listBox2.TabIndex = 6;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(429, 6);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(789, 21);
            this.comboBox1.TabIndex = 5;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1247, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // OpenHGImage
            // 
            this.OpenHGImage.FileName = "openFileDialog1";
            // 
            // Ebenen_Eingabe
            // 
            this.Ebenen_Eingabe.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox1});
            this.Ebenen_Eingabe.Name = "contextMenuStrip1";
            this.Ebenen_Eingabe.Size = new System.Drawing.Size(161, 29);
            this.Ebenen_Eingabe.VisibleChanged += new System.EventHandler(this.contextMenuStrip1_VisibleChanged);
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 23);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1247, 580);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ImageBox_ContentMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Smart_Client_Commands)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Smart_Client_Infos)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CommandList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.akt_smart_client)).EndInit();
            this.Ebenen_Eingabe.ResumeLayout(false);
            this.Ebenen_Eingabe.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.DataGridView Smart_Client_Infos;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.DataGridView Smart_Client_Commands;
        private TilingImageBox.PlaceableImageBox placeableImageBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.BindingSource akt_smart_client;
        private System.Windows.Forms.BindingSource CommandList;
        private System.Windows.Forms.ContextMenuStrip ImageBox_ContentMenu;
        private System.Windows.Forms.ToolStripMenuItem bildLadenToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog OpenHGImage;
        private System.Windows.Forms.ComboBox cb_bereiche;
        private System.Windows.Forms.Button but_neue_ebene;
        private System.Windows.Forms.ContextMenuStrip Ebenen_Eingabe;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

