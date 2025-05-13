    using System.ComponentModel;

    namespace KyrsovRPVS
    {
        partial class MainForm
        {
            private System.ComponentModel.IContainer components = null;

            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null)) components.Dispose();
                base.Dispose(disposing);
            }

        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
            amoutUnknowns = new TextBox();
            groupBox1 = new GroupBox();
            btnformTable = new Button();
            groupBox2 = new GroupBox();
            dgvSystem = new DataGridView();
            label1 = new Label();
            groupBox3 = new GroupBox();
            dgvSolution = new DataGridView();
            btnFindSolution = new Button();
            lstAnimationSteps = new ListBox();
            helpProvider = new HelpProvider();
            pictureBox = new PictureBox();
            label2 = new Label();
            btnexit = new Button();
            toolStrip1 = new ToolStrip();
            toolStripLabelFile = new ToolStripDropDownButton();
            экспортWordToolStripMenuItem = new ToolStripMenuItem();
            экспортExcelToolStripMenuItem = new ToolStripMenuItem();
            экспортPowerPointToolStripMenuItem = new ToolStripMenuItem();
            toolStripLabelInformation = new ToolStripLabel();
            toolStripLabelHelp = new ToolStripLabel();
            ctxmenu = new ContextMenuStrip(components);
            MenuItem1 = new ToolStripMenuItem();
            MenuItem2 = new ToolStripMenuItem();
            MenuItem3 = new ToolStripMenuItem();
            MenuItem4 = new ToolStripMenuItem();
            MenuItem5 = new ToolStripMenuItem();
            toolStrip2 = new ToolStrip();
            toolStripGraph = new ToolStripButton();
            toolStripInfo = new ToolStripButton();
            toolStripPowerPoint = new ToolStripButton();
            toolStripExcel = new ToolStripButton();
            toolStripWord = new ToolStripButton();
            toolStripColor = new ToolStripButton();
            btnMatlabVerify = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((ISupportInitialize)dgvSystem).BeginInit();
            groupBox3.SuspendLayout();
            ((ISupportInitialize)dgvSolution).BeginInit();
            ((ISupportInitialize)pictureBox).BeginInit();
            toolStrip1.SuspendLayout();
            ctxmenu.SuspendLayout();
            toolStrip2.SuspendLayout();
            SuspendLayout();
            // 
            // amoutUnknowns
            // 
            amoutUnknowns.Location = new Point(6, 41);
            amoutUnknowns.Name = "amoutUnknowns";
            amoutUnknowns.Size = new Size(77, 30);
            amoutUnknowns.TabIndex = 1;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnformTable);
            groupBox1.Controls.Add(amoutUnknowns);
            groupBox1.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            groupBox1.Location = new Point(9, 54);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(301, 84);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Установка числа переменных";
            // 
            // btnformTable
            // 
            btnformTable.Font = new Font("Arial", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnformTable.Location = new Point(103, 41);
            btnformTable.Name = "btnformTable";
            btnformTable.Size = new Size(192, 32);
            btnformTable.TabIndex = 2;
            btnformTable.Text = "Построить таблицу";
            btnformTable.UseVisualStyleBackColor = true;
            btnformTable.Click += FormTable;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(dgvSystem);
            groupBox2.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            groupBox2.Location = new Point(12, 166);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(729, 240);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Ввод уравнения";
            // 
            // dgvSystem
            // 
            dgvSystem.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSystem.Location = new Point(8, 29);
            dgvSystem.Name = "dgvSystem";
            dgvSystem.RowHeadersWidth = 51;
            dgvSystem.Size = new Size(713, 194);
            dgvSystem.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 13.2000008F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label1.Location = new Point(402, 54);
            label1.Name = "label1";
            label1.Size = new Size(845, 27);
            label1.TabIndex = 6;
            label1.Text = "Решение системы линейных алгебраических уравнений методом Гаусса";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(dgvSolution);
            groupBox3.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            groupBox3.Location = new Point(12, 412);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(367, 353);
            groupBox3.TabIndex = 7;
            groupBox3.TabStop = false;
            groupBox3.Text = "Решение";
            // 
            // dgvSolution
            // 
            dgvSolution.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSolution.GridColor = SystemColors.HighlightText;
            dgvSolution.Location = new Point(10, 32);
            dgvSolution.Name = "dgvSolution";
            dgvSolution.ReadOnly = true;
            dgvSolution.RowHeadersWidth = 51;
            dgvSolution.Size = new Size(351, 300);
            dgvSolution.TabIndex = 0;
            // 
            // btnFindSolution
            // 
            btnFindSolution.Font = new Font("Arial", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnFindSolution.Location = new Point(326, 95);
            btnFindSolution.Name = "btnFindSolution";
            btnFindSolution.Size = new Size(192, 32);
            btnFindSolution.TabIndex = 3;
            btnFindSolution.Text = "Найти решение";
            btnFindSolution.UseVisualStyleBackColor = true;
            btnFindSolution.Click += findSolution;
            // 
            // lstAnimationSteps
            // 
            lstAnimationSteps.FormattingEnabled = true;
            lstAnimationSteps.Location = new Point(396, 420);
            lstAnimationSteps.Name = "lstAnimationSteps";
            lstAnimationSteps.Size = new Size(345, 324);
            lstAnimationSteps.TabIndex = 8;
            // 
            // helpProvider
            // 
            helpProvider.Tag = "dgvSystem";
            // 
            // pictureBox
            // 
            pictureBox.Location = new Point(747, 166);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(768, 541);
            pictureBox.TabIndex = 9;
            pictureBox.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label2.Location = new Point(997, 102);
            label2.Name = "label2";
            label2.Size = new Size(230, 23);
            label2.TabIndex = 10;
            label2.Text = "График поиска решения";
            // 
            // btnexit
            // 
            btnexit.BackColor = Color.Firebrick;
            btnexit.Cursor = Cursors.Hand;
            btnexit.Font = new Font("Arial", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnexit.ForeColor = SystemColors.Window;
            btnexit.Location = new Point(1318, 713);
            btnexit.Name = "btnexit";
            btnexit.Size = new Size(197, 52);
            btnexit.TabIndex = 11;
            btnexit.Text = "ВЫХОД";
            btnexit.UseVisualStyleBackColor = false;
            btnexit.Click += exitform;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabelFile, toolStripLabelInformation, toolStripLabelHelp });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1542, 27);
            toolStrip1.TabIndex = 12;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabelFile
            // 
            toolStripLabelFile.DropDownItems.AddRange(new ToolStripItem[] { экспортWordToolStripMenuItem, экспортExcelToolStripMenuItem, экспортPowerPointToolStripMenuItem });
            toolStripLabelFile.Name = "toolStripLabelFile";
            toolStripLabelFile.Size = new Size(46, 24);
            toolStripLabelFile.Text = "File";
            // 
            // экспортWordToolStripMenuItem
            // 
            экспортWordToolStripMenuItem.Name = "экспортWordToolStripMenuItem";
            экспортWordToolStripMenuItem.Size = new Size(224, 26);
            экспортWordToolStripMenuItem.Text = "экспорт Word";
            экспортWordToolStripMenuItem.Click += экспортWord;
            // 
            // экспортExcelToolStripMenuItem
            // 
            экспортExcelToolStripMenuItem.Name = "экспортExcelToolStripMenuItem";
            экспортExcelToolStripMenuItem.Size = new Size(224, 26);
            экспортExcelToolStripMenuItem.Text = " экспорт Excel";
            экспортExcelToolStripMenuItem.Click += экспортExcel;
            // 
            // экспортPowerPointToolStripMenuItem
            // 
            экспортPowerPointToolStripMenuItem.Name = "экспортPowerPointToolStripMenuItem";
            экспортPowerPointToolStripMenuItem.Size = new Size(224, 26);
            экспортPowerPointToolStripMenuItem.Text = "экспорт PowerPoint";
            экспортPowerPointToolStripMenuItem.Click += экспортPowerPoint;
            // 
            // toolStripLabelInformation
            // 
            toolStripLabelInformation.Name = "toolStripLabelInformation";
            toolStripLabelInformation.Size = new Size(87, 24);
            toolStripLabelInformation.Text = "Information";
            // 
            // toolStripLabelHelp
            // 
            toolStripLabelHelp.Name = "toolStripLabelHelp";
            toolStripLabelHelp.Size = new Size(41, 24);
            toolStripLabelHelp.Text = "Help";
            // 
            // ctxmenu
            // 
            ctxmenu.ImageScalingSize = new Size(20, 20);
            ctxmenu.Items.AddRange(new ToolStripItem[] { MenuItem1, MenuItem2, MenuItem3, MenuItem4, MenuItem5 });
            ctxmenu.Name = "ctxmenu";
            ctxmenu.Size = new Size(228, 124);
            // 
            // MenuItem1
            // 
            MenuItem1.Name = "MenuItem1";
            MenuItem1.Size = new Size(227, 24);
            MenuItem1.Text = "Помощь";
            // 
            // MenuItem2
            // 
            MenuItem2.Name = "MenuItem2";
            MenuItem2.Size = new Size(227, 24);
            MenuItem2.Text = "О программе";
            // 
            // MenuItem3
            // 
            MenuItem3.Name = "MenuItem3";
            MenuItem3.Size = new Size(227, 24);
            MenuItem3.Text = "Экспорт в Word";
            // 
            // MenuItem4
            // 
            MenuItem4.Name = "MenuItem4";
            MenuItem4.Size = new Size(227, 24);
            MenuItem4.Text = "Экспорт в Excel";
            // 
            // MenuItem5
            // 
            MenuItem5.Name = "MenuItem5";
            MenuItem5.Size = new Size(227, 24);
            MenuItem5.Text = "Экспорт в Power Point";
            // 
            // toolStrip2
            // 
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripGraph, toolStripInfo, toolStripPowerPoint, toolStripExcel, toolStripWord, toolStripColor });
            toolStrip2.Location = new Point(0, 27);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(1542, 27);
            toolStrip2.TabIndex = 13;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripGraph
            // 
            toolStripGraph.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripGraph.Image = (Image)resources.GetObject("toolStripGraph.Image");
            toolStripGraph.ImageTransparentColor = Color.Magenta;
            toolStripGraph.Name = "toolStripGraph";
            toolStripGraph.Size = new Size(29, 24);
            toolStripGraph.Text = "График";
            // 
            // toolStripInfo
            // 
            toolStripInfo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripInfo.Image = (Image)resources.GetObject("toolStripInfo.Image");
            toolStripInfo.ImageTransparentColor = Color.Magenta;
            toolStripInfo.Name = "toolStripInfo";
            toolStripInfo.Size = new Size(29, 24);
            toolStripInfo.Text = "Информация";
            // 
            // toolStripPowerPoint
            // 
            toolStripPowerPoint.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripPowerPoint.Image = (Image)resources.GetObject("toolStripPowerPoint.Image");
            toolStripPowerPoint.ImageTransparentColor = Color.Magenta;
            toolStripPowerPoint.Name = "toolStripPowerPoint";
            toolStripPowerPoint.Size = new Size(29, 24);
            toolStripPowerPoint.Text = "Экспорт Powerpoint";
            // 
            // toolStripExcel
            // 
            toolStripExcel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripExcel.Image = (Image)resources.GetObject("toolStripExcel.Image");
            toolStripExcel.ImageTransparentColor = Color.Magenta;
            toolStripExcel.Name = "toolStripExcel";
            toolStripExcel.Size = new Size(29, 24);
            toolStripExcel.Text = "Экспорт Excel";
            // 
            // toolStripWord
            // 
            toolStripWord.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripWord.Image = (Image)resources.GetObject("toolStripWord.Image");
            toolStripWord.ImageTransparentColor = Color.Magenta;
            toolStripWord.Name = "toolStripWord";
            toolStripWord.Size = new Size(29, 24);
            toolStripWord.Text = "Экспорт Word";
            // 
            // toolStripColor
            // 
            toolStripColor.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripColor.Image = (Image)resources.GetObject("toolStripColor.Image");
            toolStripColor.ImageTransparentColor = Color.Magenta;
            toolStripColor.Name = "toolStripColor";
            toolStripColor.Size = new Size(29, 24);
            toolStripColor.Text = "Цвет графика";
            toolStripColor.Click += toolStripChangeColor;
            // 
            // btnMatlabVerify
            // 
            btnMatlabVerify.Location = new Point(541, 95);
            btnMatlabVerify.Name = "btnMatlabVerify";
            btnMatlabVerify.Size = new Size(192, 32);
            btnMatlabVerify.TabIndex = 14;
            btnMatlabVerify.Text = "Решить в Matlab";
            btnMatlabVerify.UseVisualStyleBackColor = true;
            btnMatlabVerify.Click += solveMatlab;
            // 
            // MainForm
            // 
            ClientSize = new Size(1542, 1055);
            Controls.Add(btnMatlabVerify);
            Controls.Add(toolStrip2);
            Controls.Add(toolStrip1);
            Controls.Add(btnexit);
            Controls.Add(label2);
            Controls.Add(pictureBox);
            Controls.Add(lstAnimationSteps);
            Controls.Add(btnFindSolution);
            Controls.Add(groupBox3);
            Controls.Add(label1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "MainForm";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((ISupportInitialize)dgvSystem).EndInit();
            groupBox3.ResumeLayout(false);
            ((ISupportInitialize)dgvSolution).EndInit();
            ((ISupportInitialize)pictureBox).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ctxmenu.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private GroupBox groupBox1;
            private Button btnformTable;
            private GroupBox groupBox2;
            private Label label1;
            private GroupBox groupBox3;
            private Button btnFindSolution;
            private ListBox lstAnimationSteps;
            private HelpProvider helpProvider;
            private DataGridView dgvSolution;
            private PictureBox pictureBox;
            private Label label2;
            private TextBox amoutUnknowns;
        private DataGridView dgvSystem;
        private Button btnexit;
        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabelInformation;
        private ToolStripLabel toolStripLabelHelp;
        private ToolStripDropDownButton toolStripLabelFile;
        private ToolStripMenuItem экспортWordToolStripMenuItem;
        private ToolStripMenuItem экспортExcelToolStripMenuItem;
        private ToolStripMenuItem экспортPowerPointToolStripMenuItem;
        private ContextMenuStrip ctxmenu;
        private ToolStripMenuItem MenuItem1;
        private ToolStripMenuItem MenuItem2;
        private ToolStripMenuItem MenuItem3;
        private ToolStripMenuItem MenuItem4;
        private ToolStripMenuItem MenuItem5;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripGraph;
        private ToolStripButton toolStripInfo;
        private ToolStripButton toolStripPowerPoint;
        private ToolStripButton toolStripExcel;
        private ToolStripButton toolStripWord;
        private ToolStripButton toolStripColor;
        private Button btnMatlabVerify;
    }
    }
