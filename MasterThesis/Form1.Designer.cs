namespace Neuroevolution_Application
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.btnSaveBestNetwork = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.lblCurrStatus = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtbDataPath = new System.Windows.Forms.TextBox();
            this.groupTraining = new System.Windows.Forms.GroupBox();
            this.btnStopTrain = new System.Windows.Forms.Button();
            this.btnStartTrain = new System.Windows.Forms.Button();
            this.chartErrors = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.listEvolutionErrors = new System.Windows.Forms.ListBox();
            this.groupTesting = new System.Windows.Forms.GroupBox();
            this.btnDetect = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.listTestingErrors = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupMain = new System.Windows.Forms.GroupBox();
            this.imgOutput5 = new System.Windows.Forms.PictureBox();
            this.imgOutput4 = new System.Windows.Forms.PictureBox();
            this.imgOutput3 = new System.Windows.Forms.PictureBox();
            this.imgOutput2 = new System.Windows.Forms.PictureBox();
            this.tmpNext = new System.Windows.Forms.Button();
            this.tmpValue = new System.Windows.Forms.NumericUpDown();
            this.tmpTest = new System.Windows.Forms.Button();
            this.imgOutput = new System.Windows.Forms.PictureBox();
            this.imgInput = new System.Windows.Forms.PictureBox();
            this.btnChangeDataPath = new System.Windows.Forms.Button();
            this.groupTraining.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartErrors)).BeginInit();
            this.groupTesting.SuspendLayout();
            this.groupMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tmpValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgInput)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSaveBestNetwork
            // 
            this.btnSaveBestNetwork.Enabled = false;
            this.btnSaveBestNetwork.Location = new System.Drawing.Point(15, 83);
            this.btnSaveBestNetwork.Name = "btnSaveBestNetwork";
            this.btnSaveBestNetwork.Size = new System.Drawing.Size(143, 36);
            this.btnSaveBestNetwork.TabIndex = 2;
            this.btnSaveBestNetwork.Text = "Save Best Network";
            this.btnSaveBestNetwork.UseVisualStyleBackColor = true;
            this.btnSaveBestNetwork.Click += new System.EventHandler(this.BtnSaveNetwork_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(52, 17);
            this.label9.TabIndex = 18;
            this.label9.Text = "Status:";
            // 
            // lblCurrStatus
            // 
            this.lblCurrStatus.AutoSize = true;
            this.lblCurrStatus.ForeColor = System.Drawing.Color.Red;
            this.lblCurrStatus.Location = new System.Drawing.Point(86, 22);
            this.lblCurrStatus.Name = "lblCurrStatus";
            this.lblCurrStatus.Size = new System.Drawing.Size(42, 17);
            this.lblCurrStatus.TabIndex = 19;
            this.lblCurrStatus.Text = "None";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 45);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 17);
            this.label10.TabIndex = 20;
            this.label10.Text = "DataPath:";
            // 
            // txtbDataPath
            // 
            this.txtbDataPath.Location = new System.Drawing.Point(89, 42);
            this.txtbDataPath.Name = "txtbDataPath";
            this.txtbDataPath.ReadOnly = true;
            this.txtbDataPath.Size = new System.Drawing.Size(144, 22);
            this.txtbDataPath.TabIndex = 21;
            this.txtbDataPath.Text = ".\\Data\\Characters\\CharsData.txt";
            // 
            // groupTraining
            // 
            this.groupTraining.Controls.Add(this.btnStopTrain);
            this.groupTraining.Controls.Add(this.btnStartTrain);
            this.groupTraining.Controls.Add(this.chartErrors);
            this.groupTraining.Controls.Add(this.listEvolutionErrors);
            this.groupTraining.Location = new System.Drawing.Point(469, 17);
            this.groupTraining.Name = "groupTraining";
            this.groupTraining.Size = new System.Drawing.Size(601, 653);
            this.groupTraining.TabIndex = 31;
            this.groupTraining.TabStop = false;
            this.groupTraining.Text = "Training";
            // 
            // btnStopTrain
            // 
            this.btnStopTrain.Enabled = false;
            this.btnStopTrain.Location = new System.Drawing.Point(493, 601);
            this.btnStopTrain.Name = "btnStopTrain";
            this.btnStopTrain.Size = new System.Drawing.Size(88, 36);
            this.btnStopTrain.TabIndex = 25;
            this.btnStopTrain.Text = "Stop";
            this.btnStopTrain.UseVisualStyleBackColor = true;
            this.btnStopTrain.Click += new System.EventHandler(this.btnStopTrain_Click);
            // 
            // btnStartTrain
            // 
            this.btnStartTrain.Enabled = false;
            this.btnStartTrain.Location = new System.Drawing.Point(385, 601);
            this.btnStartTrain.Name = "btnStartTrain";
            this.btnStartTrain.Size = new System.Drawing.Size(86, 36);
            this.btnStartTrain.TabIndex = 24;
            this.btnStartTrain.Text = "Start Train";
            this.btnStartTrain.UseVisualStyleBackColor = true;
            this.btnStartTrain.Click += new System.EventHandler(this.btnStartTrain_Click);
            // 
            // chartErrors
            // 
            chartArea1.AxisX.MajorGrid.LineWidth = 0;
            chartArea1.AxisX.MinorGrid.LineWidth = 0;
            chartArea1.AxisX.Title = "Epoch";
            chartArea1.AxisY.MajorGrid.LineWidth = 0;
            chartArea1.AxisY.Title = "Error";
            chartArea1.BorderColor = System.Drawing.Color.Transparent;
            chartArea1.Name = "ChartArea1";
            this.chartErrors.ChartAreas.Add(chartArea1);
            this.chartErrors.Location = new System.Drawing.Point(9, 21);
            this.chartErrors.Name = "chartErrors";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.LabelBorderWidth = 0;
            series1.LegendText = "Error";
            series1.MarkerColor = System.Drawing.Color.Red;
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Diamond;
            series1.Name = "Series1";
            this.chartErrors.Series.Add(series1);
            this.chartErrors.Size = new System.Drawing.Size(575, 251);
            this.chartErrors.TabIndex = 20;
            this.chartErrors.Text = "chart1";
            title1.Name = "Title1";
            title1.Text = "Minimum error per generation";
            this.chartErrors.Titles.Add(title1);
            // 
            // listEvolutionErrors
            // 
            this.listEvolutionErrors.FormattingEnabled = true;
            this.listEvolutionErrors.ItemHeight = 16;
            this.listEvolutionErrors.Location = new System.Drawing.Point(9, 313);
            this.listEvolutionErrors.Name = "listEvolutionErrors";
            this.listEvolutionErrors.Size = new System.Drawing.Size(575, 260);
            this.listEvolutionErrors.TabIndex = 21;
            // 
            // groupTesting
            // 
            this.groupTesting.Controls.Add(this.btnDetect);
            this.groupTesting.Controls.Add(this.btnTest);
            this.groupTesting.Controls.Add(this.listTestingErrors);
            this.groupTesting.Location = new System.Drawing.Point(12, 510);
            this.groupTesting.Name = "groupTesting";
            this.groupTesting.Size = new System.Drawing.Size(426, 371);
            this.groupTesting.TabIndex = 32;
            this.groupTesting.TabStop = false;
            this.groupTesting.Text = "Testing";
            // 
            // btnDetect
            // 
            this.btnDetect.Enabled = false;
            this.btnDetect.Location = new System.Drawing.Point(310, 313);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(100, 36);
            this.btnDetect.TabIndex = 27;
            this.btnDetect.Text = "Detect";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // btnTest
            // 
            this.btnTest.Enabled = false;
            this.btnTest.Location = new System.Drawing.Point(190, 313);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(100, 36);
            this.btnTest.TabIndex = 26;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // listTestingErrors
            // 
            this.listTestingErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listTestingErrors.HideSelection = false;
            this.listTestingErrors.Location = new System.Drawing.Point(14, 38);
            this.listTestingErrors.MultiSelect = false;
            this.listTestingErrors.Name = "listTestingErrors";
            this.listTestingErrors.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.listTestingErrors.Size = new System.Drawing.Size(396, 269);
            this.listTestingErrors.TabIndex = 32;
            this.listTestingErrors.UseCompatibleStateImageBehavior = false;
            this.listTestingErrors.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Correct";
            this.columnHeader1.Width = 156;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Guessed";
            this.columnHeader2.Width = 287;
            // 
            // groupMain
            // 
            this.groupMain.Controls.Add(this.imgOutput5);
            this.groupMain.Controls.Add(this.imgOutput4);
            this.groupMain.Controls.Add(this.imgOutput3);
            this.groupMain.Controls.Add(this.imgOutput2);
            this.groupMain.Controls.Add(this.tmpNext);
            this.groupMain.Controls.Add(this.tmpValue);
            this.groupMain.Controls.Add(this.tmpTest);
            this.groupMain.Controls.Add(this.imgOutput);
            this.groupMain.Controls.Add(this.imgInput);
            this.groupMain.Controls.Add(this.btnChangeDataPath);
            this.groupMain.Controls.Add(this.btnSaveBestNetwork);
            this.groupMain.Controls.Add(this.txtbDataPath);
            this.groupMain.Controls.Add(this.label9);
            this.groupMain.Controls.Add(this.lblCurrStatus);
            this.groupMain.Controls.Add(this.label10);
            this.groupMain.Enabled = false;
            this.groupMain.Location = new System.Drawing.Point(12, 17);
            this.groupMain.Name = "groupMain";
            this.groupMain.Size = new System.Drawing.Size(435, 472);
            this.groupMain.TabIndex = 33;
            this.groupMain.TabStop = false;
            this.groupMain.Text = "Main";
            // 
            // imgOutput5
            // 
            this.imgOutput5.Location = new System.Drawing.Point(239, 364);
            this.imgOutput5.Name = "imgOutput5";
            this.imgOutput5.Size = new System.Drawing.Size(169, 95);
            this.imgOutput5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgOutput5.TabIndex = 31;
            this.imgOutput5.TabStop = false;
            // 
            // imgOutput4
            // 
            this.imgOutput4.Location = new System.Drawing.Point(14, 364);
            this.imgOutput4.Name = "imgOutput4";
            this.imgOutput4.Size = new System.Drawing.Size(169, 95);
            this.imgOutput4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgOutput4.TabIndex = 30;
            this.imgOutput4.TabStop = false;
            // 
            // imgOutput3
            // 
            this.imgOutput3.Location = new System.Drawing.Point(239, 263);
            this.imgOutput3.Name = "imgOutput3";
            this.imgOutput3.Size = new System.Drawing.Size(169, 95);
            this.imgOutput3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgOutput3.TabIndex = 29;
            this.imgOutput3.TabStop = false;
            // 
            // imgOutput2
            // 
            this.imgOutput2.Location = new System.Drawing.Point(15, 263);
            this.imgOutput2.Name = "imgOutput2";
            this.imgOutput2.Size = new System.Drawing.Size(169, 95);
            this.imgOutput2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgOutput2.TabIndex = 28;
            this.imgOutput2.TabStop = false;
            // 
            // tmpNext
            // 
            this.tmpNext.Enabled = false;
            this.tmpNext.Location = new System.Drawing.Point(286, 125);
            this.tmpNext.Name = "tmpNext";
            this.tmpNext.Size = new System.Drawing.Size(143, 41);
            this.tmpNext.TabIndex = 27;
            this.tmpNext.Text = "TMP Next";
            this.tmpNext.UseVisualStyleBackColor = true;
            this.tmpNext.Click += new System.EventHandler(this.tmpNext_Click);
            // 
            // tmpValue
            // 
            this.tmpValue.DecimalPlaces = 2;
            this.tmpValue.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.tmpValue.Location = new System.Drawing.Point(208, 91);
            this.tmpValue.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tmpValue.Name = "tmpValue";
            this.tmpValue.Size = new System.Drawing.Size(72, 22);
            this.tmpValue.TabIndex = 26;
            this.tmpValue.Value = new decimal(new int[] {
            4,
            0,
            0,
            65536});
            this.tmpValue.ValueChanged += new System.EventHandler(this.tmpValue_ValueChanged);
            // 
            // tmpTest
            // 
            this.tmpTest.Enabled = false;
            this.tmpTest.Location = new System.Drawing.Point(286, 83);
            this.tmpTest.Name = "tmpTest";
            this.tmpTest.Size = new System.Drawing.Size(143, 36);
            this.tmpTest.TabIndex = 25;
            this.tmpTest.Text = "TMP Testing";
            this.tmpTest.UseVisualStyleBackColor = true;
            this.tmpTest.Click += new System.EventHandler(this.tmpTest_Click);
            // 
            // imgOutput
            // 
            this.imgOutput.Location = new System.Drawing.Point(239, 178);
            this.imgOutput.Name = "imgOutput";
            this.imgOutput.Size = new System.Drawing.Size(169, 79);
            this.imgOutput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgOutput.TabIndex = 24;
            this.imgOutput.TabStop = false;
            // 
            // imgInput
            // 
            this.imgInput.Location = new System.Drawing.Point(15, 178);
            this.imgInput.Name = "imgInput";
            this.imgInput.Size = new System.Drawing.Size(169, 79);
            this.imgInput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgInput.TabIndex = 23;
            this.imgInput.TabStop = false;
            // 
            // btnChangeDataPath
            // 
            this.btnChangeDataPath.Location = new System.Drawing.Point(239, 35);
            this.btnChangeDataPath.Name = "btnChangeDataPath";
            this.btnChangeDataPath.Size = new System.Drawing.Size(86, 36);
            this.btnChangeDataPath.TabIndex = 22;
            this.btnChangeDataPath.Text = "Change";
            this.btnChangeDataPath.UseVisualStyleBackColor = true;
            this.btnChangeDataPath.Click += new System.EventHandler(this.btnChangeDataPath_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1097, 925);
            this.Controls.Add(this.groupMain);
            this.Controls.Add(this.groupTesting);
            this.Controls.Add(this.groupTraining);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Neuroevolution - Application";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.groupTraining.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartErrors)).EndInit();
            this.groupTesting.ResumeLayout(false);
            this.groupMain.ResumeLayout(false);
            this.groupMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tmpValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgInput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSaveBestNetwork;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblCurrStatus;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtbDataPath;
        private System.Windows.Forms.GroupBox groupTraining;
        private System.Windows.Forms.GroupBox groupTesting;
        private System.Windows.Forms.GroupBox groupMain;
        private System.Windows.Forms.Button btnChangeDataPath;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartErrors;
        private System.Windows.Forms.ListBox listEvolutionErrors;
        private System.Windows.Forms.ListView listTestingErrors;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button btnDetect;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnStopTrain;
        private System.Windows.Forms.Button btnStartTrain;
        private System.Windows.Forms.PictureBox imgOutput;
        private System.Windows.Forms.PictureBox imgInput;
        private System.Windows.Forms.Button tmpTest;
        private System.Windows.Forms.NumericUpDown tmpValue;
        private System.Windows.Forms.Button tmpNext;
        private System.Windows.Forms.PictureBox imgOutput3;
        private System.Windows.Forms.PictureBox imgOutput2;
        private System.Windows.Forms.PictureBox imgOutput5;
        private System.Windows.Forms.PictureBox imgOutput4;
    }
}

