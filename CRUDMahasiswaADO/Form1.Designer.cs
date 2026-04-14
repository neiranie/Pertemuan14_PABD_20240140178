namespace CRUDMahasiswaADO
{
    partial class FormMahasiswa
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtNIM = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNama = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbJK = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpTanggalLahir = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAlamat = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtKodeProdi = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();

            // label1 - NIM
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.Text = "NIM";

            // txtNIM
            this.txtNIM.Location = new System.Drawing.Point(130, 17);
            this.txtNIM.Name = "txtNIM";
            this.txtNIM.Size = new System.Drawing.Size(200, 20);

            // label2 - Nama
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 50);
            this.label2.Name = "label2";
            this.label2.Text = "Nama";

            // txtNama
            this.txtNama.Location = new System.Drawing.Point(130, 47);
            this.txtNama.Name = "txtNama";
            this.txtNama.Size = new System.Drawing.Size(200, 20);

            // label4 - Jenis Kelamin
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 80);
            this.label4.Name = "label4";
            this.label4.Text = "Jenis Kelamin";

            // cmbJK
            this.cmbJK.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJK.Location = new System.Drawing.Point(130, 77);
            this.cmbJK.Name = "cmbJK";
            this.cmbJK.Size = new System.Drawing.Size(100, 21);

            // label5 - Tanggal Lahir
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 110);
            this.label5.Name = "label5";
            this.label5.Text = "Tanggal Lahir";

            // dtpTanggalLahir
            this.dtpTanggalLahir.Location = new System.Drawing.Point(130, 107);
            this.dtpTanggalLahir.Name = "dtpTanggalLahir";
            this.dtpTanggalLahir.Size = new System.Drawing.Size(200, 20);

            // label3 - Alamat
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 140);
            this.label3.Name = "label3";
            this.label3.Text = "Alamat";

            // txtAlamat
            this.txtAlamat.Location = new System.Drawing.Point(130, 137);
            this.txtAlamat.Multiline = true;
            this.txtAlamat.Name = "txtAlamat";
            this.txtAlamat.Size = new System.Drawing.Size(200, 50);

            // label6 - Kode Prodi
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 200);
            this.label6.Name = "label6";
            this.label6.Text = "Kode Prodi";

            // txtKodeProdi
            this.txtKodeProdi.Location = new System.Drawing.Point(130, 197);
            this.txtKodeProdi.Name = "txtKodeProdi";
            this.txtKodeProdi.Size = new System.Drawing.Size(100, 20);

            // btnConnect - Membuka Koneksi
            this.btnConnect.Location = new System.Drawing.Point(380, 17);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(130, 28);
            this.btnConnect.Text = "Membuka Koneksi";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

            // btnLoad - Menampilkan Data
            this.btnLoad.Location = new System.Drawing.Point(380, 55);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(130, 28);
            this.btnLoad.Text = "Menampilkan Data";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);

            // btnInsert - Menambah Data
            this.btnInsert.Location = new System.Drawing.Point(380, 93);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(130, 28);
            this.btnInsert.Text = "Menambah Data";
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);

            // btnUpdate - Mengubah Data
            this.btnUpdate.Location = new System.Drawing.Point(380, 131);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(130, 28);
            this.btnUpdate.Text = "Mengubah Data";
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);

            // btnDelete - Menghapus Data
            this.btnDelete.Location = new System.Drawing.Point(380, 169);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(130, 28);
            this.btnDelete.Text = "Menghapus Data";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // dataGridView1
            this.dataGridView1.Location = new System.Drawing.Point(20, 240);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(540, 200);

            // FormMahasiswa
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNIM);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtNama);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbJK);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dtpTanggalLahir);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAlamat);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtKodeProdi);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.dataGridView1);
            this.Name = "FormMahasiswa";
            this.Text = "Form Data Mahasiswa";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // Deklarasi semua komponen
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNIM;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNama;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbJK;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpTanggalLahir;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAlamat;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtKodeProdi;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}