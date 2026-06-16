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
        DAL dbLogic = new DAL();

        // FIX: 1 bindingSource konsisten dipakai untuk dataGridView & bindingNavigator,
        // sebelumnya bercampur dengan mahasiswaBindingSource (typed dataset) yang tidak pernah diisi.
        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();

        // FIX: simpan NIM baris yang sedang dipilih, dipakai untuk Update/Delete
        // supaya tidak bergantung pada txtNIM yang bisa ReadOnly/kosong.
        private string selectedNIM = null;

        public FormMahasiswa()
        {
            InitializeComponent();

            dataGridView1.CellClick += new DataGridViewCellEventHandler(dataGridView1_CellClick);

            // FIX: pasang event Import Excel & Import ke Database secara manual,
            // karena di Designer sebelumnya kedua tombol ini TIDAK ter-assign Click handler
            // (btnImpDb malah ter-assign ke btnImportDatabase_Click yang isinya cuma placeholder).
            btnImpExcel.Click += new EventHandler(btnImpExcel_Click);
            btnImpDb.Click += new EventHandler(btnImpDb_Click);

            // FIX: pasang Test Injection (sebelumnya tidak pernah di-assign di Designer)
            btnTestInjection.Click += new EventHandler(btnTestInjection_Click);

            // FIX: pasang Reset (sebelumnya tidak pernah di-assign di Designer)
            btnReset.Click += new EventHandler(btnReset_Click);
        }

        // =============================================
        // SIMPAN LOG
        // =============================================
        public void simpanLog(string message)
        {
            try { dbLogic.InsertLog(message); }
            catch { /* jangan sampai logging gagal menimbulkan error baru ke user */ }
        }

        // =============================================
        // FORM LOAD
        // =============================================
        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };
            cmbJK.SelectedIndex = -1;

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            LoadData();
        }

        // =============================================
        // LOAD DATA
        // =============================================
        private void LoadData()
        {
            try
            {
                dtMahasiswa = dbLogic.GetMhs();

                bindingSource.DataSource = dtMahasiswa;
                dataGridView1.DataSource = bindingSource;
                bindingNavigator1.BindingSource = bindingSource;

                // Set kolom Foto sebagai ImageColumn
                if (dataGridView1.Columns["Foto"] != null && !(dataGridView1.Columns["Foto"] is DataGridViewImageColumn))
                {
                    DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                    imgCol.Name = "Foto";
                    imgCol.HeaderText = "Foto";
                    imgCol.DataPropertyName = "Foto";
                    imgCol.ImageLayout = DataGridViewImageCellLayout.Stretch;
                    int fotoIndex = dataGridView1.Columns["Foto"].Index;
                    dataGridView1.Columns.Remove("Foto");
                    dataGridView1.Columns.Insert(fotoIndex, imgCol);
                }

                HitungTotal();

                // FIX: pastikan semua tombol & grid kembali aktif setelah Load
                // (sebelumnya bisa "nyangkut" disabled kalau sebelumnya sempat Import Excel).
                dataGridView1.Enabled = true;
                btnInsert.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnCari.Enabled = true;
                btnLoad.Enabled = true;
                btnReset.Enabled = true;
                btnTestInjection.Enabled = true;
                btnImpDb.Enabled = false; // hanya aktif setelah Import Excel berhasil membaca file

                ClearForm(keepGridSelection: false);
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        // =============================================
        // HITUNG TOTAL
        // =============================================
        private void HitungTotal()
        {
            try
            {
                int total = dbLogic.CountMhs();
                lblTotal.Text = "Total Mahasiswa: " + total;
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        // =============================================
        // CELL CLICK - populate form saat klik baris
        // =============================================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dataGridView1.DataSource != bindingSource) return; // sedang mode preview Excel, bukan data DB

            try
            {
                DataRowView rowView = (DataRowView)bindingSource[e.RowIndex];
                DataRow row = rowView.Row;

                selectedNIM = row["NIM"].ToString();

                txtNIM.Text = row["NIM"].ToString();
                txtNama.Text = row["Nama"].ToString();
                cmbJK.Text = row["JenisKelamin"].ToString().Trim();
                txtAlamat.Text = row["Alamat"].ToString();

                if (row["TanggalLahir"] != DBNull.Value)
                    dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);

                // FIX UTAMA: data hasil sp_GetMahasiswa HANYA punya kolom "NamaProdi" (bukan KodeProdi),
                // sesuai stored procedure di modul. Sebelumnya kode ini salah mengisi txtKodeProdi
                // dengan NamaProdi (misal "Teknik Informatika") karena kolom KodeProdi tidak ada di sini.
                // Akibatnya saat Update, teks "Teknik Informatika" dikirim sebagai KodeProdi ke database,
                // dan ditolak oleh FOREIGN KEY constraint FK_Mahasiswa_Prodi (karena bukan kode yang valid).
                // FIX: ambil KodeProdi yang BENAR dari sp_GetMahasiswaByNIM (yang memang punya kolom KodeProdi),
                // bukan dari baris grid ini.
                DataTable dtDetail = dbLogic.GetMhsByNIM(selectedNIM);
                if (dtDetail.Rows.Count > 0 && dtDetail.Columns.Contains("KodeProdi") && dtDetail.Rows[0]["KodeProdi"] != DBNull.Value)
                {
                    txtKodeProdi.Text = dtDetail.Rows[0]["KodeProdi"].ToString().Trim();
                }
                else if (row.Table.Columns.Contains("NamaProdi") && row["NamaProdi"] != DBNull.Value)
                {
                    // fallback tampilan saja kalau KodeProdi benar2 tidak ketemu (seharusnya tidak terjadi)
                    txtKodeProdi.Text = row["NamaProdi"].ToString();
                }

                // Tampilkan foto
                if (row.Table.Columns.Contains("Foto") && row["Foto"] != DBNull.Value && row["Foto"] != null)
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

                // FIX UTAMA #1:
                // Sebelumnya: txtNIM.Enabled = false; -> field NIM jadi benar2 nonaktif & terkesan "macet",
                // dan ketika sudah false, baris lain yang diklik tidak akan mengubah apapun karena banyak
                // operasi form bergantung pada txtNIM yang disabled (Enabled control tidak ikut binding events
                // dengan baik di beberapa versi WinForms, dan terlihat seperti "tidak bisa diklik lagi").
                // FIX: gunakan ReadOnly (bukan Enabled) supaya:
                //   - User tetap tidak bisa MENGUBAH NIM saat mode edit (sesuai requirement "NIM jangan diubah saat update")
                //   - User TETAP BISA klik baris lain di grid untuk pindah data (karena yang dipakai untuk
                //     update/delete adalah variabel selectedNIM, bukan txtNIM yang Enabled-nya diubah-ubah).
                txtNIM.ReadOnly = true;
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Error select: " + ex.Message);
            }
        }

        // =============================================
        // TOMBOL - Open / Connect
        // =============================================
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DAL.GetConnectionString()))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi Berhasil");
                }
            }
            catch (SqlException ex) { simpanLog(ex.Message); MessageBox.Show("SQL Error : " + ex.Message); }
            catch (Exception ex) { simpanLog(ex.Message); MessageBox.Show("General Error : " + ex.Message); }
        }

        // =============================================
        // TOMBOL - Load
        // =============================================
        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // =============================================
        // TOMBOL - INSERT
        // =============================================
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (txtNIM.Text == "") { MessageBox.Show("NIM harus diisi"); txtNIM.Focus(); return; }
            if (txtNama.Text == "") { MessageBox.Show("Nama harus diisi"); txtNama.Focus(); return; }
            if (cmbJK.SelectedIndex < 0) { MessageBox.Show("Jenis Kelamin harus dipilih"); cmbJK.Focus(); return; }
            if (txtKodeProdi.Text == "") { MessageBox.Show("Kode Prodi harus diisi"); txtKodeProdi.Focus(); return; }

            try
            {
                byte[] imgBytes = ConvertImageToByteArray(pictureBox1.Image);

                dbLogic.InsertMhs(
                    txtNIM.Text.Trim(),
                    txtNama.Text.Trim(),
                    txtAlamat.Text.Trim(),
                    cmbJK.Text.Trim(),
                    dtpTanggalLahir.Value.Date,
                    txtKodeProdi.Text.Trim(),
                    imgBytes
                );

                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog("Rollback Insert :" + ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog("General Error :" + ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        // =============================================
        // TOMBOL - UPDATE
        // =============================================
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // FIX UTAMA #2: gunakan selectedNIM (diisi saat klik baris di grid),
            // bukan txtNIM.Text yang mungkin sudah ReadOnly tapi tetap berisi NIM yang benar.
            // Validasi tambahan: kalau belum pernah pilih baris, tolak Update.
            string nimUntukUpdate = !string.IsNullOrEmpty(selectedNIM) ? selectedNIM : txtNIM.Text.Trim();

            if (string.IsNullOrEmpty(nimUntukUpdate))
            {
                MessageBox.Show("Pilih data terlebih dahulu (klik baris pada tabel)");
                return;
            }
            if (txtNama.Text == "") { MessageBox.Show("Nama harus diisi"); txtNama.Focus(); return; }
            if (cmbJK.SelectedIndex < 0 && string.IsNullOrEmpty(cmbJK.Text)) { MessageBox.Show("Jenis Kelamin harus dipilih"); cmbJK.Focus(); return; }
            if (txtKodeProdi.Text == "") { MessageBox.Show("Kode Prodi harus diisi"); txtKodeProdi.Focus(); return; }

            try
            {
                byte[] imgBytes = ConvertImageToByteArray(pictureBox1.Image);

                dbLogic.UpdateMhs(
                    nimUntukUpdate,
                    txtNama.Text.Trim(),
                    txtAlamat.Text.Trim(),
                    cmbJK.Text.Trim(),
                    dtpTanggalLahir.Value.Date,
                    txtKodeProdi.Text.Trim(),
                    imgBytes
                );

                MessageBox.Show("Data mahasiswa berhasil diubah");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        // =============================================
        // TOMBOL - DELETE
        // =============================================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // FIX UTAMA #3: sebelumnya tombol Delete (di Designer) ter-pasang ke event yang salah,
            // sehingga yang berjalan adalah logic "Cari" -> makanya muncul pesan "Data tidak ditemukan"
            // padahal data sebenarnya masih ada (baru hilang setelah Refresh karena LoadData dipanggil lagi
            // tanpa benar2 menghapus). Sekarang Delete murni memanggil dbLogic.DeleteMhs dengan NIM yang valid.
            string nimUntukDelete = !string.IsNullOrEmpty(selectedNIM) ? selectedNIM : txtNIM.Text.Trim();

            if (string.IsNullOrEmpty(nimUntukDelete))
            {
                MessageBox.Show("Pilih data terlebih dahulu (klik baris pada tabel)");
                return;
            }

            DialogResult dg = MessageBox.Show(
                "Yakin ingin menghapus data NIM: " + nimUntukDelete + "?",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dg == DialogResult.Yes)
            {
                try
                {
                    dbLogic.DeleteMhs(nimUntukDelete);
                    MessageBox.Show("Data mahasiswa berhasil dihapus");
                    ClearForm();
                    LoadData();
                }
                catch (SqlException ex)
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error :" + ex.Message);
                }
                catch (Exception ex)
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("General Error :" + ex.Message);
                }
            }
        }

        // =============================================
        // TOMBOL - CARI (berdasarkan NIM, sesuai stored procedure sp_GetMahasiswaByNIM pada modul)
        // =============================================
        private void btnCari_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNIM.Text))
            {
                MessageBox.Show("Masukkan NIM terlebih dahulu!");
                return;
            }

            try
            {
                DataTable dtCari = dbLogic.GetMhsByNIM(txtNIM.Text.Trim());

                if (dtCari.Rows.Count > 0)
                {
                    DataRow row = dtCari.Rows[0];
                    selectedNIM = row["NIM"].ToString();

                    txtNIM.Text = row["NIM"].ToString();
                    txtNama.Text = row["Nama"].ToString();
                    cmbJK.Text = row["JenisKelamin"].ToString().Trim();
                    txtAlamat.Text = row["Alamat"].ToString();

                    if (dtCari.Columns.Contains("KodeProdi"))
                        txtKodeProdi.Text = row["KodeProdi"].ToString();
                    else if (dtCari.Columns.Contains("NamaProdi"))
                        txtKodeProdi.Text = row["NamaProdi"].ToString();

                    if (row["TanggalLahir"] != DBNull.Value)
                        dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);

                    if (dtCari.Columns.Contains("Foto") && row["Foto"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])row["Foto"];
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else pictureBox1.Image = null;

                    txtNIM.ReadOnly = true;

                    MessageBox.Show("Data ditemukan!");
                }
                else
                {
                    MessageBox.Show("Data tidak ditemukan.");
                }
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Error Cari: " + ex.Message);
            }
        }

        // =============================================
        // TOMBOL - RESET DATA
        // =============================================
        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult dg = MessageBox.Show(
                "Yakin ingin mereset seluruh data mahasiswa ke kondisi backup?",
                "Konfirmasi Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dg != DialogResult.Yes) return;

            try
            {
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex) { simpanLog(ex.Message); MessageBox.Show("SQL Error :" + ex.Message); }
            catch (Exception ex) { simpanLog(ex.Message); MessageBox.Show("General Error :" + ex.Message); }
        }

        // =============================================
        // TOMBOL - TEST INJECTION
        // =============================================
        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNIM.Text))
            {
                MessageBox.Show("Masukkan NIM terlebih dahulu untuk uji coba injection");
                return;
            }

            try
            {
                dbLogic.testInject(txtNIM.Text.Trim());
                MessageBox.Show("Query dijalankan. Cek apakah nama berubah jadi 'HACKED'.");
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error : Unsafe UPDATE operation not allowed");
                }
                else
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error :" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        // =============================================
        // TOMBOL - REKAP DATA
        // =============================================
        private void btnRekapData_Click(object sender, EventArgs e)
        {
            RekapMahasiswa fm3 = new RekapMahasiswa();
            fm3.Show();
            this.Hide();
        }

        // =============================================
        // TOMBOL - UPLOAD GAMBAR
        // =============================================
        private void btnUploadGambar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(ofd.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        // =============================================
        // TOMBOL - REFRESH
        // =============================================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadData();
        }

        // =============================================
        // TOMBOL - IMPORT EXCEL (preview ke grid, belum masuk DB)
        // =============================================
        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Workbook|*.xlsx" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    try
                    {
                        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = true
                                }
                            });

                            DataTable dt = result.Tables[0];

                            // FIX: validasi kolom wajib supaya tidak error samar saat Import ke Database nanti.
                            string[] kolomWajib = { "NIM", "Nama", "JenisKelamin", "Alamat", "NamaProdi", "TanggalLahir" };
                            foreach (string kolom in kolomWajib)
                            {
                                if (!dt.Columns.Contains(kolom))
                                {
                                    MessageBox.Show("Kolom '" + kolom + "' tidak ditemukan pada file Excel. " +
                                        "Pastikan header kolom: NIM, Nama, JenisKelamin, TanggalLahir, Alamat, NamaProdi (dan opsional FotoPath).");
                                    return;
                                }
                            }

                            dataGridView1.DataSource = dt;
                            dataGridView1.Enabled = false;

                            btnImpDb.Enabled = true;
                            btnInsert.Enabled = false;
                            btnUpdate.Enabled = false;
                            btnDelete.Enabled = false;
                            btnCari.Enabled = false;
                            btnLoad.Enabled = false;
                            btnReset.Enabled = false;
                            btnTestInjection.Enabled = false;

                            MessageBox.Show("File Excel berhasil dibaca (" + dt.Rows.Count + " baris). Klik 'Import form Database' untuk menyimpan ke database.");
                        }
                    }
                    catch (Exception ex)
                    {
                        simpanLog(ex.Message);
                        MessageBox.Show("Gagal membaca file Excel: " + ex.Message);
                    }
                }
            }
        }

        // =============================================
        // TOMBOL - IMPORT KE DATABASE dari Excel
        // =============================================
        private void btnImpDb_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = dataGridView1.DataSource as DataTable;

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;
                int gagal = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string kodeProdi = row["NamaProdi"].ToString().Trim();

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama))
                    {
                        gagal++;
                        continue;
                    }

                    // Parsing tanggal yang aman
                    DateTime tglLahir;
                    string tglStr = row["TanggalLahir"].ToString().Trim();

                    if (!DateTime.TryParse(tglStr, out tglLahir))
                    {
                        if (double.TryParse(tglStr, out double oleDate))
                        {
                            try { tglLahir = DateTime.FromOADate(oleDate); }
                            catch { gagal++; continue; }
                        }
                        else { gagal++; continue; }
                    }

                    // Validasi range tanggal SQL Server
                    if (tglLahir < new DateTime(1753, 1, 1) || tglLahir > new DateTime(9999, 12, 31))
                    {
                        gagal++;
                        continue;
                    }

                    byte[] fotoBytes = null;
                    if (dt.Columns.Contains("FotoPath"))
                    {
                        string fotoPath = row["FotoPath"].ToString().Trim();
                        fotoBytes = ConvertImageFromPath(fotoPath);
                    }

                    try
                    {
                        dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);
                        sukses++;
                    }
                    catch (Exception exRow)
                    {
                        simpanLog("Gagal import NIM " + nim + ": " + exRow.Message);
                        gagal++;
                    }
                }

                MessageBox.Show("Import selesai. Berhasil: " + sukses + ", Gagal: " + gagal);
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog("Rollback Insert :" + ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog("General Error :" + ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        // =============================================
        // TOMBOL - IMPORT FROM DATABASE EKSTERNAL (placeholder, sesuai modul belum diminta implementasi penuh)
        // =============================================
        private void btnImportDatabase_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fitur Import dari Database Eksternal sedang dikembangkan.");
        }

        // =============================================
        // CLEAR FORM
        // =============================================
        private void ClearForm(bool keepGridSelection = false)
        {
            txtNIM.ReadOnly = false; // kembalikan NIM agar bisa diisi untuk Insert baru
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            pictureBox1.Image = null;
            selectedNIM = null;
            txtNIM.Focus();
        }

        private void ClearForm()
        {
            ClearForm(false);
        }

        // =============================================
        // HELPER - Konversi Image ke Byte Array
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

        // =============================================
        // HELPER - Konversi path foto ke Byte Array (untuk import Excel)
        // =============================================
        private byte[] ConvertImageFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (!File.Exists(path)) return null;
            return File.ReadAllBytes(path);
        }

        // Event kosong (generated by designer)
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void lblTotal_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void bindingNavigator1_RefreshItems(object sender, EventArgs e) { }
        private void FormMahasiswa_SizeChanged(object sender, EventArgs e) { }
    }
}