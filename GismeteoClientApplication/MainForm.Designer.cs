
namespace GismeteoClientApplication
{
    partial class MainForm
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
            this.citiesComboBox = new System.Windows.Forms.ComboBox();
            this.citiesLabel = new System.Windows.Forms.Label();
            this.datesLabel = new System.Windows.Forms.Label();
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.getForecastButton = new System.Windows.Forms.Button();
            this.forecastTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // citiesComboBox
            // 
            this.citiesComboBox.FormattingEnabled = true;
            this.citiesComboBox.Location = new System.Drawing.Point(12, 25);
            this.citiesComboBox.Name = "citiesComboBox";
            this.citiesComboBox.Size = new System.Drawing.Size(260, 21);
            this.citiesComboBox.TabIndex = 0;
            this.citiesComboBox.SelectionChangeCommitted += new System.EventHandler(this.citiesComboBox_SelectionChangeCommitted);
            // 
            // citiesLabel
            // 
            this.citiesLabel.AutoSize = true;
            this.citiesLabel.Location = new System.Drawing.Point(12, 9);
            this.citiesLabel.Name = "citiesLabel";
            this.citiesLabel.Size = new System.Drawing.Size(32, 13);
            this.citiesLabel.TabIndex = 1;
            this.citiesLabel.Text = "Cities";
            // 
            // datesLabel
            // 
            this.datesLabel.AutoSize = true;
            this.datesLabel.Location = new System.Drawing.Point(12, 49);
            this.datesLabel.Name = "datesLabel";
            this.datesLabel.Size = new System.Drawing.Size(35, 13);
            this.datesLabel.TabIndex = 2;
            this.datesLabel.Text = "Dates";
            // 
            // dateTimePicker
            // 
            this.dateTimePicker.Location = new System.Drawing.Point(12, 65);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.Size = new System.Drawing.Size(260, 20);
            this.dateTimePicker.TabIndex = 4;
            // 
            // getForecastButton
            // 
            this.getForecastButton.Location = new System.Drawing.Point(144, 91);
            this.getForecastButton.Name = "getForecastButton";
            this.getForecastButton.Size = new System.Drawing.Size(128, 21);
            this.getForecastButton.TabIndex = 5;
            this.getForecastButton.Text = "Get Weather Forecast";
            this.getForecastButton.UseVisualStyleBackColor = true;
            this.getForecastButton.Click += new System.EventHandler(this.getForecastButton_Click);
            // 
            // forecastTextBox
            // 
            this.forecastTextBox.Location = new System.Drawing.Point(279, 25);
            this.forecastTextBox.Multiline = true;
            this.forecastTextBox.Name = "forecastTextBox";
            this.forecastTextBox.ReadOnly = true;
            this.forecastTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.forecastTextBox.Size = new System.Drawing.Size(316, 288);
            this.forecastTextBox.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 325);
            this.Controls.Add(this.forecastTextBox);
            this.Controls.Add(this.getForecastButton);
            this.Controls.Add(this.dateTimePicker);
            this.Controls.Add(this.datesLabel);
            this.Controls.Add(this.citiesLabel);
            this.Controls.Add(this.citiesComboBox);
            this.Name = "MainForm";
            this.Text = "Gismeteo Client Application";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox citiesComboBox;
        private System.Windows.Forms.Label citiesLabel;
        private System.Windows.Forms.Label datesLabel;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.Button getForecastButton;
        private System.Windows.Forms.TextBox forecastTextBox;
    }
}

