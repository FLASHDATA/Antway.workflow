namespace Client.Winforms.Demos
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.tbLog = new System.Windows.Forms.TextBox();
            this.btSiguiente = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbLocalizador = new System.Windows.Forms.TextBox();
            this.btRun = new System.Windows.Forms.Button();
            this.btnFirmarDoc = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btTest = new System.Windows.Forms.Button();
            this.btAnterior = new System.Windows.Forms.Button();
            this.cmbSchemeCodes = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lbLocalizadores = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btSimulateBDChange = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.gbActividades = new System.Windows.Forms.GroupBox();
            this.tbActivityExecutionDetail = new System.Windows.Forms.TextBox();
            this.lbActivities = new System.Windows.Forms.ListBox();
            this.bindingWFSParameterValues = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbActividades.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingWFSParameterValues)).BeginInit();
            this.SuspendLayout();
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.BackColor = System.Drawing.Color.Black;
            this.tbLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLog.ForeColor = System.Drawing.Color.White;
            this.tbLog.Location = new System.Drawing.Point(309, 25);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ReadOnly = true;
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLog.Size = new System.Drawing.Size(442, 647);
            this.tbLog.TabIndex = 1;
            // 
            // btSiguiente
            // 
            this.btSiguiente.BackColor = System.Drawing.Color.White;
            this.btSiguiente.Enabled = false;
            this.btSiguiente.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btSiguiente.Image = global::Client.Winforms.Demos.Properties.Resources.icons8_invitado_masculino_20;
            this.btSiguiente.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btSiguiente.Location = new System.Drawing.Point(7, 86);
            this.btSiguiente.Name = "btSiguiente";
            this.btSiguiente.Size = new System.Drawing.Size(132, 23);
            this.btSiguiente.TabIndex = 2;
            this.btSiguiente.Text = "Siguiente >>";
            this.btSiguiente.UseVisualStyleBackColor = false;
            this.btSiguiente.Click += new System.EventHandler(this.btSiguiente_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Localizador";
            // 
            // tbLocalizador
            // 
            this.tbLocalizador.BackColor = System.Drawing.Color.Black;
            this.tbLocalizador.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLocalizador.ForeColor = System.Drawing.Color.White;
            this.tbLocalizador.Location = new System.Drawing.Point(75, 58);
            this.tbLocalizador.Name = "tbLocalizador";
            this.tbLocalizador.Size = new System.Drawing.Size(64, 23);
            this.tbLocalizador.TabIndex = 4;
            // 
            // btRun
            // 
            this.btRun.BackColor = System.Drawing.Color.White;
            this.btRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btRun.Image = ((System.Drawing.Image)(resources.GetObject("btRun.Image")));
            this.btRun.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btRun.Location = new System.Drawing.Point(171, 58);
            this.btRun.Name = "btRun";
            this.btRun.Size = new System.Drawing.Size(110, 23);
            this.btRun.TabIndex = 5;
            this.btRun.Text = "Resume/Run  ";
            this.btRun.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btRun.UseVisualStyleBackColor = false;
            this.btRun.Click += new System.EventHandler(this.btRun_Click);
            // 
            // btnFirmarDoc
            // 
            this.btnFirmarDoc.BackColor = System.Drawing.Color.White;
            this.btnFirmarDoc.Enabled = false;
            this.btnFirmarDoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFirmarDoc.Image = ((System.Drawing.Image)(resources.GetObject("btnFirmarDoc.Image")));
            this.btnFirmarDoc.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFirmarDoc.Location = new System.Drawing.Point(171, 86);
            this.btnFirmarDoc.Name = "btnFirmarDoc";
            this.btnFirmarDoc.Size = new System.Drawing.Size(110, 23);
            this.btnFirmarDoc.TabIndex = 10;
            this.btnFirmarDoc.Text = "Firmar";
            this.btnFirmarDoc.UseVisualStyleBackColor = false;
            this.btnFirmarDoc.Click += new System.EventHandler(this.btnDocSignat_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.btTest);
            this.groupBox2.Controls.Add(this.btAnterior);
            this.groupBox2.Controls.Add(this.cmbSchemeCodes);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.lbLocalizadores);
            this.groupBox2.Controls.Add(this.tbLocalizador);
            this.groupBox2.Controls.Add(this.btnFirmarDoc);
            this.groupBox2.Controls.Add(this.btSiguiente);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btRun);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(291, 660);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Aplicación cliente que conecta con Workflow";
            // 
            // btTest
            // 
            this.btTest.Location = new System.Drawing.Point(206, 135);
            this.btTest.Name = "btTest";
            this.btTest.Size = new System.Drawing.Size(75, 23);
            this.btTest.TabIndex = 21;
            this.btTest.Text = "Test";
            this.btTest.UseVisualStyleBackColor = true;
            this.btTest.Visible = false;
            this.btTest.Click += new System.EventHandler(this.btTest_Click);
            // 
            // btAnterior
            // 
            this.btAnterior.BackColor = System.Drawing.Color.White;
            this.btAnterior.Enabled = false;
            this.btAnterior.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btAnterior.Image = global::Client.Winforms.Demos.Properties.Resources.icons8_invitado_masculino_20;
            this.btAnterior.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btAnterior.Location = new System.Drawing.Point(7, 115);
            this.btAnterior.Name = "btAnterior";
            this.btAnterior.Size = new System.Drawing.Size(132, 23);
            this.btAnterior.TabIndex = 20;
            this.btAnterior.Text = "<< Anterior";
            this.btAnterior.UseVisualStyleBackColor = false;
            this.btAnterior.Click += new System.EventHandler(this.btAnterior_Click);
            // 
            // cmbSchemeCodes
            // 
            this.cmbSchemeCodes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSchemeCodes.FormattingEnabled = true;
            this.cmbSchemeCodes.Items.AddRange(new object[] {
            "CALL_SERVICE_SAMPLE",
            "CALL_SERVICE_ASYNC",
            "EXPEDIENTES"});
            this.cmbSchemeCodes.Location = new System.Drawing.Point(6, 22);
            this.cmbSchemeCodes.Name = "cmbSchemeCodes";
            this.cmbSchemeCodes.Size = new System.Drawing.Size(275, 21);
            this.cmbSchemeCodes.TabIndex = 19;
            this.cmbSchemeCodes.SelectedValueChanged += new System.EventHandler(this.cmbSchemeCodes_SelectedValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 160);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(173, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Localizadores de Workflows en BD";
            // 
            // lbLocalizadores
            // 
            this.lbLocalizadores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbLocalizadores.BackColor = System.Drawing.Color.Black;
            this.lbLocalizadores.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLocalizadores.ForeColor = System.Drawing.Color.White;
            this.lbLocalizadores.FormattingEnabled = true;
            this.lbLocalizadores.ItemHeight = 20;
            this.lbLocalizadores.Location = new System.Drawing.Point(6, 176);
            this.lbLocalizadores.Name = "lbLocalizadores";
            this.lbLocalizadores.Size = new System.Drawing.Size(275, 464);
            this.lbLocalizadores.Sorted = true;
            this.lbLocalizadores.TabIndex = 6;
            this.lbLocalizadores.SelectedValueChanged += new System.EventHandler(this.lbLocalizadores_SelectedValueChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.btSimulateBDChange);
            this.groupBox3.Location = new System.Drawing.Point(765, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(352, 76);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Expedientes: Simular cambio en Base de datos";
            // 
            // btSimulateBDChange
            // 
            this.btSimulateBDChange.Enabled = false;
            this.btSimulateBDChange.Location = new System.Drawing.Point(6, 34);
            this.btSimulateBDChange.Name = "btSimulateBDChange";
            this.btSimulateBDChange.Size = new System.Drawing.Size(336, 23);
            this.btSimulateBDChange.TabIndex = 21;
            this.btSimulateBDChange.Text = "Marcar documento como FIRMADO";
            this.btSimulateBDChange.UseVisualStyleBackColor = true;
            this.btSimulateBDChange.Click += new System.EventHandler(this.btSimulateBDChange_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(309, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "LOG";
            // 
            // gbActividades
            // 
            this.gbActividades.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbActividades.Controls.Add(this.tbActivityExecutionDetail);
            this.gbActividades.Controls.Add(this.lbActivities);
            this.gbActividades.Location = new System.Drawing.Point(765, 94);
            this.gbActividades.Name = "gbActividades";
            this.gbActividades.Size = new System.Drawing.Size(352, 578);
            this.gbActividades.TabIndex = 21;
            this.gbActividades.TabStop = false;
            this.gbActividades.Text = "Activities";
            // 
            // tbActivityExecutionDetail
            // 
            this.tbActivityExecutionDetail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbActivityExecutionDetail.BackColor = System.Drawing.Color.Black;
            this.tbActivityExecutionDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbActivityExecutionDetail.ForeColor = System.Drawing.Color.White;
            this.tbActivityExecutionDetail.Location = new System.Drawing.Point(10, 224);
            this.tbActivityExecutionDetail.Multiline = true;
            this.tbActivityExecutionDetail.Name = "tbActivityExecutionDetail";
            this.tbActivityExecutionDetail.Size = new System.Drawing.Size(332, 344);
            this.tbActivityExecutionDetail.TabIndex = 22;
            // 
            // lbActivities
            // 
            this.lbActivities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbActivities.BackColor = System.Drawing.Color.Black;
            this.lbActivities.ForeColor = System.Drawing.Color.White;
            this.lbActivities.FormattingEnabled = true;
            this.lbActivities.Location = new System.Drawing.Point(10, 19);
            this.lbActivities.Name = "lbActivities";
            this.lbActivities.Size = new System.Drawing.Size(332, 199);
            this.lbActivities.TabIndex = 21;
            this.lbActivities.SelectedValueChanged += new System.EventHandler(this.lbActivities_SelectedValueChanged);
            // 
            // bindingWFSParameterValues
            // 
            this.bindingWFSParameterValues.DataSource = typeof(AntWay.Persistence.Provider.Model.WorkflowSchemeParameterValuesView);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1129, 686);
            this.Controls.Add(this.gbActividades);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.tbLog);
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Demo Workflows";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.gbActividades.ResumeLayout(false);
            this.gbActividades.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingWFSParameterValues)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button btSiguiente;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbLocalizador;
        private System.Windows.Forms.Button btRun;
        private System.Windows.Forms.Button btnFirmarDoc;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lbLocalizadores;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbSchemeCodes;
        private System.Windows.Forms.BindingSource bindingWFSParameterValues;
        private System.Windows.Forms.Button btAnterior;
        private System.Windows.Forms.GroupBox gbActividades;
        private System.Windows.Forms.TextBox tbActivityExecutionDetail;
        private System.Windows.Forms.ListBox lbActivities;
        private System.Windows.Forms.Button btSimulateBDChange;
        private System.Windows.Forms.Button btTest;
    }
}