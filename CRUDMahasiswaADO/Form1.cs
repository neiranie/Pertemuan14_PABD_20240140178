using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using ExcelDataReader;

namespace CRUDMahasiswaADO
{
    public partial class FormMahasiswa : Form
    {
        private readonly string connectionString =
            "Data Source=LAPTOP-9BPMNG3K\\ANNEIRA;Initial Catalog=DBAkademikADO;User ID=sa;Password=neira291206;";

        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();

        public FormMahasiswa()
        {
            InitializeComponent();
        }

        // =============================================
        // SIMPAN LOG ERROR KE DATABASE
        // =============================================
        private void SimpanLog(string pesan)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO LogError VALUES(GETDATE(), @pesan)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@pesan", pesan);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { /* Jangan crash hanya karena log gagal */ }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // HAPUS bindingNavigator binding agar tidak konflik dengan DataBinding manual
            // bindingNavigator1.BindingSource = bindingSource;

            LoadData();
        }

        // =============================================
        // LOAD DATA - menggunakan sp_GetMahasiswa
        // sp_GetMahasiswa sekarang return kolom:
        // NIM, Nama, JenisKelamin, TanggalLahir, Alamat, Foto, NamaProdi
        // (KodeProdi TIDAK ada di result set karena pakai JOIN)
        // =============================================
        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            dtMahasiswa = new DataTable();
                            da.Fill(dtMahasiswa);

                            // Bind ke DataGridView langsung tanpa BindingSource
                            // agar tidak ada masalah DataBinding kolom
                            dataGridView1.DataSource = dtMahasiswa;

                            // Set kolom Foto agar render sebagai gambar
                            if (dataGridView1.Columns["Foto"] != null)
                            {
                                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                                imgCol.Name = "Foto";
                                imgCol.HeaderText = "Foto";
                                imgCol.DataPropertyName = "Foto";
                                imgCol.ImageLayout = DataGridViewImageCellLayout.Stretch;
                                // Ganti kolom Foto yang ada dengan ImageColumn
                                int fotoIndex = dataGridView1.Columns["Foto"].Index;
                                dataGridView1.Columns.Remove("Foto");
                                dataGridView1.Columns.Insert(fotoIndex, imgCol);
                            }
                        }
                    }
                }

                HitungTotal();
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        // =============================================
        // HITUNG TOTAL - sp_CountMahasiswa (OUTPUT)
        // =============================================
        private void HitungTotal()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                        outputParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        lblTotal.Text = "Total Mahasiswa: " + outputParam.Value.ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        // =============================================
        // CELL CLICK - Klik baris di DataGridView
        // FIX: Gunakan index kolom, bukan nama kolom "KodeProdi"
        // karena sp_GetMahasiswa JOIN return NamaProdi bukan KodeProdi
        // Kolom urutan: 0=NIM, 1=Nama, 2=JenisKelamin, 3=TanggalLahir,
        //               4=Alamat, 5=Foto, 6=NamaProdi
        // =============================================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                // Ambil dari DataTable langsung, lebih aman
                DataRow row = ((DataRowView)dataGridView1.Rows[e.RowIndex].DataBoundItem).Row;

                txtNIM.Text = row["NIM"].ToString();
                txtNama.Text = row["Nama"].ToString();
                cmbJK.Text = row["JenisKelamin"].ToString();
                txtAlamat.Text = row["Alamat"].ToString();

                // NamaProdi ditampilkan di txtKodeProdi (atau bisa buat txtNamaProdi terpisah)
                // Kolom KodeProdi tidak ada di sp_GetMahasiswa baru — tampilkan NamaProdi saja
                if (dtMahasiswa.Columns.Contains("NamaProdi"))
                    txtKodeProdi.Text = row["NamaProdi"].ToString();
                else if (dtMahasiswa.Columns.Contains("KodeProdi"))
                    txtKodeProdi.Text = row["KodeProdi"].ToString();

                // TanggalLahir
                if (row["TanggalLahir"] != DBNull.Value)
                    dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);

                // Tampilkan foto jika ada
                if (row["Foto"] != DBNull.Value)
                {
                    byte[] imgBytes = (byte[])row["Foto"];
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                // Tidak perlu MessageBox, cukup log
            }
        }

        // =============================================
        // TOMBOL - Membuka Koneksi
        // =============================================
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi berhasil");
                }
            }
            catch (SqlException ex) { SimpanLog(ex.Message); MessageBox.Show("SQL Error : " + ex.Message); }
            catch (Exception ex) { SimpanLog(ex.Message); MessageBox.Show("General Error : " + ex.Message); }
        }

        // =============================================
        // TOMBOL - Load
        // =============================================
        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // =============================================
        // TOMBOL - Insert - sp_InsertMahasiswa
        // =============================================
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (txtNIM.Text == "") { MessageBox.Show("NIM harus diisi"); txtNIM.Focus(); return; }
            if (txtNama.Text == "") { MessageBox.Show("Nama harus diisi"); txtNama.Focus(); return; }
            if (cmbJK.Text == "") { MessageBox.Show("Jenis Kelamin harus dipilih"); cmbJK.Focus(); return; }
            if (txtKodeProdi.Text == "") { MessageBox.Show("Kode Prodi harus diisi"); txtKodeProdi.Focus(); return; }

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);
                cmd.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);

                byte[] fotoData = ConvertImageToByteArray(pictureBox1.Image);
                cmd.Parameters.AddWithValue("@Foto", fotoData != null ? (object)fotoData : DBNull.Value);

                cmd.ExecuteNonQuery();

                // Log aktivitas
                SqlCommand cmdLog = new SqlCommand(
                    "INSERT INTO LogAktivitasSalah (aktivitas,waktu) VALUES (@aktivitas,GETDATE())", conn, trans);
                cmdLog.Parameters.AddWithValue("@aktivitas", "INSERT MAHASISWA : " + txtNIM.Text);
                cmdLog.ExecuteNonQuery();

                SqlCommand cmdLogInsert = new SqlCommand(
                    "INSERT INTO LogInsertMahasiswa (NIM, Waktu) VALUES (@NIM, GETDATE())", conn, trans);
                cmdLogInsert.Parameters.AddWithValue("@NIM", txtNIM.Text);
                cmdLogInsert.ExecuteNonQuery();

                trans.Commit();
                MessageBox.Show("Data dan Foto berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                trans.Rollback();
                SimpanLog("ROLLBACK INSERT : " + ex.Message);
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                SimpanLog("GENERAL ERROR : " + ex.Message);
                MessageBox.Show(ex.Message);
            }
            finally { conn.Close(); }
        }

        // =============================================
        // TOMBOL - Update - sp_UpdateMahasiswa
        // =============================================
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                        cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                        cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);

                        byte[] fotoData = ConvertImageToByteArray(pictureBox1.Image);
                        cmd.Parameters.AddWithValue("@Foto", fotoData != null ? (object)fotoData : DBNull.Value);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0) { MessageBox.Show("Data berhasil diupdate"); ClearForm(); LoadData(); }
                        else MessageBox.Show("Data tidak ditemukan");
                    }
                }
            }
            catch (SqlException ex) { SimpanLog(ex.Message); MessageBox.Show("SQL Error : " + ex.Message); }
            catch (Exception ex) { SimpanLog(ex.Message); MessageBox.Show("General Error : " + ex.Message); }
        }

        // =============================================
        // TOMBOL - Delete - sp_DeleteMahasiswa
        // =============================================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "Yakin ingin menghapus data NIM: " + txtNIM.Text + "?",
                "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = txtNIM.Text;

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0) { MessageBox.Show("Data berhasil dihapus"); ClearForm(); LoadData(); }
                            else MessageBox.Show("Data tidak ditemukan");
                        }
                    }
                }
                catch (SqlException ex) { SimpanLog(ex.Message); MessageBox.Show("SQL Error : " + ex.Message); }
                catch (Exception ex) { SimpanLog(ex.Message); MessageBox.Show("General Error : " + ex.Message); }
            }
        }

        // =============================================
        // TOMBOL - Reset Data
        // =============================================
        private void btnResetData_Click_1(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        IF OBJECT_ID('dbo.Mahasiswa_Backup1') IS NOT NULL
                        BEGIN
                            DELETE FROM dbo.Mahasiswa;
                            INSERT INTO dbo.Mahasiswa SELECT * FROM dbo.Mahasiswa_Backup1;
                        END";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                        cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Data berhasil direset");
                LoadData();
            }
            catch (SqlException ex) { SimpanLog(ex.Message); MessageBox.Show("SQL Error : " + ex.Message); }
            catch (Exception ex) { SimpanLog(ex.Message); MessageBox.Show("General Error : " + ex.Message); }
        }

        // =============================================
        // TOMBOL - Test SQL Injection
        // =============================================
        private void btnTestInjection_Click_1(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Mahasiswa SET Nama='" + txtNama.Text + "' WHERE NIM='" + txtNIM.Text + "'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update berhasil");
                }
            }
            catch (SqlException ex) { SimpanLog(ex.Message); MessageBox.Show("SQL Error : " + ex.Message); }
            catch (Exception ex) { SimpanLog(ex.Message); MessageBox.Show("General Error : " + ex.Message); }
        }

        // =============================================
        // TOMBOL - Cari Mahasiswa
        // =============================================
        private void btnCari_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNIM.Text))
            {
                MessageBox.Show("Masukkan NIM di textbox terlebih dahulu untuk mencari!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@pNIM", txtNIM.Text);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dtCari = new DataTable();
                            da.Fill(dtCari);

                            if (dtCari.Rows.Count > 0)
                            {
                                dataGridView1.DataSource = dtCari;

                                DataRow row = dtCari.Rows[0];
                                txtNIM.Text = row["NIM"].ToString();
                                txtNama.Text = row["Nama"].ToString();
                                cmbJK.Text = row["JenisKelamin"].ToString();
                                txtAlamat.Text = row["Alamat"].ToString();

                                if (dtCari.Columns.Contains("NamaProdi"))
                                    txtKodeProdi.Text = row["NamaProdi"].ToString();
                                else if (dtCari.Columns.Contains("KodeProdi"))
                                    txtKodeProdi.Text = row["KodeProdi"].ToString();

                                if (row["TanggalLahir"] != DBNull.Value)
                                    dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);

                                if (row["Foto"] != DBNull.Value)
                                {
                                    byte[] imgBytes = (byte[])row["Foto"];
                                    using (MemoryStream ms = new MemoryStream(imgBytes))
                                    {
                                        pictureBox1.Image = Image.FromStream(ms);
                                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                    }
                                }
                                else
                                {
                                    pictureBox1.Image = null;
                                }

                                MessageBox.Show("Data ditemukan!");
                            }
                            else
                            {
                                MessageBox.Show("Data tidak ditemukan.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { SimpanLog(ex.Message); MessageBox.Show("Error Cari: " + ex.Message); }
        }

        // =============================================
        // TOMBOL - Upload Gambar
        // =============================================
        private void btnUploadGambar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(ofd.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        // =============================================
        // TOMBOL - Refresh
        // =============================================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadData();
            MessageBox.Show("Form dan Data grid berhasil diperbarui!");
        }

        // =============================================
        // TOMBOL - Import Excel
        // =============================================
        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openExcel = new OpenFileDialog();
            openExcel.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if (openExcel.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var stream = File.Open(openExcel.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            using (SqlConnection conn = new SqlConnection(connectionString))
                            {
                                conn.Open();
                                int sukses = 0;
                                bool isHeader = true;

                                while (reader.Read())
                                {
                                    if (isHeader) { isHeader = false; continue; }

                                    using (SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        // Kolom Excel: 0=NIM,1=Nama,2=JenisKelamin,3=TanggalLahir,4=Alamat,5=KodeProdi
                                        cmd.Parameters.AddWithValue("@NIM", reader.GetValue(0)?.ToString() ?? "");
                                        cmd.Parameters.AddWithValue("@Nama", reader.GetValue(1)?.ToString() ?? "");
                                        cmd.Parameters.AddWithValue("@JenisKelamin", reader.GetValue(2)?.ToString() ?? "");
                                        cmd.Parameters.AddWithValue("@TanggalLahir", Convert.ToDateTime(reader.GetValue(3)));
                                        cmd.Parameters.AddWithValue("@Alamat", reader.GetValue(4)?.ToString() ?? "");
                                        cmd.Parameters.AddWithValue("@KodeProdi", reader.GetValue(5)?.ToString() ?? "");
                                        cmd.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@Foto", DBNull.Value);
                                        cmd.ExecuteNonQuery();
                                        sukses++;
                                    }
                                }
                                MessageBox.Show($"{sukses} Data dari Excel berhasil diimport ke Database!");
                            }
                        }
                    }
                    LoadData();
                }
                catch (Exception ex) { SimpanLog(ex.Message); MessageBox.Show("Gagal import excel: " + ex.Message); }
            }
        }

        // =============================================
        // TOMBOL - Import Database
        // =============================================
        private void btnImportDatabase_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fitur Import dari Database Eksternal sedang dikembangkan.");
        }

        // =============================================
        // TOMBOL - Rekap Data
        // =============================================
        private void btnRekapData_Click(object sender, EventArgs e)
        {
            RekapMahasiswa fm3 = new RekapMahasiswa();
            fm3.Show();
            this.Hide();
        }

        // =============================================
        // Clear Form
        // =============================================
        private void ClearForm()
        {
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            pictureBox1.Image = null;
            txtNIM.Focus();
        }

        // =============================================
        // Konversi Image ke Byte Array
        // =============================================
        private byte[] ConvertImageToByteArray(Image img)
        {
            if (img == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        // Event kosong (generated by designer)
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void btnTestInjection_BackColorChanged(object sender, EventArgs e) { }
        private void lblTotal_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }

        private void FormMahasiswa_SizeChanged(object sender, EventArgs e)
        {

        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }
    }
}