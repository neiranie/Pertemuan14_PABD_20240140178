using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        // =============================================
        // CONNECTION STRING
        // =============================================
        // FIX: dibuat static method (bukan field yang dihitung sekali saat class load),
        // supaya IP lokal selalu terbaru saat dipanggil (sesuai langkah modul Deploy Aplikasi).
        public static string GetLocalIPAddress()
        {
            string localIP = string.Empty;
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting local IP address: " + ex.Message);
            }
            return localIP;
        }

        public static string GetConnectionString()
        {
            // DEVELOPMENT (comment dulu):
            // return @"Data Source=LAPTOP-9BPMNG3K\ANNEIRA;Initial Catalog=DBAkademikADO;User ID=sa;Password=neira291206";

            // DEPLOY (aktifkan ini):
            return $"Data Source={GetLocalIPAddress()};Initial Catalog=DBAkademikADO;User ID=sa;Password=neira291206";

            // =============================================
            // DEPLOY (nanti, sesuai modul langkah 18-19 "Deploy Aplikasi"):
            // Aktifkan baris di bawah ini (hapus komentar) dan beri komentar pada baris di atas,
            // SETELAH TCP/IP diaktifkan di SQL Server Configuration Manager
            // dan SQL Server bisa diakses dari komputer lain di jaringan yang sama.
            // =============================================
            // return $"Data Source={GetLocalIPAddress()};Initial Catalog=DBAkademikADO;User ID=sa;Password=neira291206";
        }

        // Instance method supaya tetap kompatibel dengan pemanggilan dbLogic.GetConnectionString()
        public string GetConnString()
        {
            return GetConnectionString();
        }

        // =============================================
        // COUNT MAHASISWA
        // =============================================
        public int CountMhs()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                outputParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParam);

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(outputParam.Value);
            }
        }

        // =============================================
        // GET ALL MAHASISWA
        // =============================================
        public DataTable GetMhs()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // =============================================
        // GET MAHASISWA BY NIM (dipakai untuk Cari & untuk ambil KodeProdi saat klik row)
        // =============================================
        public DataTable GetMhsByNIM(string nim)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", nim);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // =============================================
        // INSERT MAHASISWA
        // =============================================
        public void InsertMhs(string nim, string nama, string alamat, string jenisKelamin,
            DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    SqlCommand command = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@pNIM", nim);
                    command.Parameters.AddWithValue("@pNama", nama);
                    command.Parameters.AddWithValue("@pAlamat", alamat);
                    command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
                    command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
                    command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);

                    SqlParameter fotoParam = new SqlParameter("@pFoto", SqlDbType.VarBinary, -1);
                    fotoParam.Value = (object)foto ?? DBNull.Value;
                    command.Parameters.Add(fotoParam);

                    command.ExecuteNonQuery();
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw; // lempar kembali agar form bisa tampilkan error
                }
            }
        }

        // =============================================
        // UPDATE MAHASISWA
        // =============================================
        public void UpdateMhs(string nim, string nama, string alamat, string jenisKelamin,
            DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand command = new SqlCommand("sp_UpdateMahasiswa", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@pNIM", nim);
                command.Parameters.AddWithValue("@pNama", nama);
                command.Parameters.AddWithValue("@pAlamat", alamat);
                command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);

                SqlParameter fotoParam = new SqlParameter("@pFoto", SqlDbType.VarBinary, -1);
                fotoParam.Value = (object)foto ?? DBNull.Value;
                command.Parameters.Add(fotoParam);

                int rows = command.ExecuteNonQuery();

                // FIX: beri tahu kalau ternyata tidak ada baris yang ter-update (NIM tidak ketemu),
                // supaya tidak terlihat "berhasil" padahal data tidak berubah.
                if (rows == 0)
                {
                    throw new Exception("Tidak ada data dengan NIM " + nim + " yang berhasil diupdate. Pastikan NIM benar.");
                }
            }
        }

        // =============================================
        // DELETE MAHASISWA
        // =============================================
        public void DeleteMhs(string nim)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
                cmd.Parameters.AddWithValue("@NIM", nim);
                cmd.CommandType = CommandType.StoredProcedure;

                int rows = cmd.ExecuteNonQuery();

                // FIX: ini penyebab Delete selalu muncul "Data tidak ditemukan" lalu hilang setelah refresh:
                // sebelumnya tombol Delete memanggil logic Cari (bug event binding), bukan DeleteMhs.
                // Sekarang DeleteMhs benar2 dipanggil dan kita validasi jumlah baris terhapus.
                if (rows == 0)
                {
                    throw new Exception("Data dengan NIM " + nim + " tidak ditemukan, tidak ada yang dihapus.");
                }
            }
        }

        // =============================================
        // RESET DATA
        // =============================================
        public void resetData()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string deleteQuery = "DELETE FROM mahasiswa;";
                SqlCommand cmdDelete = new SqlCommand(deleteQuery, conn);
                cmdDelete.ExecuteNonQuery();

                string insertQuery = @"INSERT INTO mahasiswa SELECT * FROM mahasiswa_backup;";
                SqlCommand cmdInsert = new SqlCommand(insertQuery, conn);
                cmdInsert.ExecuteNonQuery();
            }
        }

        // =============================================
        // TEST INJECTION (sengaja vulnerable untuk demo materi SQL Injection di modul)
        // Tombol "Test" pada form dimaksudkan menunjukkan bahaya string concatenation.
        // =============================================
        public void testInject(string nim)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "UPDATE mahasiswa SET nama = 'HACKED' WHERE NIM = '" + nim + "'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }

        // =============================================
        // LOG
        // =============================================
        public void InsertLog(string message)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_LogMessage", conn);
                cmd.Parameters.AddWithValue("@psn", message);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
        }

        // =============================================
        // PRODI
        // =============================================
        public DataTable getProdi()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT NamaProdi, KodeProdi FROM ProgramStudi", conn);
                cmd.CommandType = CommandType.Text;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // =============================================
        // REKAP / REPORT
        // =============================================
        public DataTable getDataRekap(string prodi, DateTime tanggalMasuk)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTglMsuk", tanggalMasuk.Year.ToString());

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =============================================
        // CHART / DASHBOARD
        // =============================================
        public DataTable getAllDataChart()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_DashBoard", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable getDataChartByTahun(DateTime thMasuk)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_DashBoardByTahun", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inTglMsuk", thMasuk.Year);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}