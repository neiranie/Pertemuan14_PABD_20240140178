public void InsertMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
{
    if (conn.State == ConnectionState.Closed)
    {
        conn.Open();
    }
    SqlTransaction trans = conn.BeginTransaction();
    try
    {
        SqlCommand command = new SqlCommand("sp_InsertMahasiswa", conn);
        command.CommandType = CommandType.StoredProcedure;
        command.Transaction = trans;

        command.Parameters.AddWithValue("@pNIM", nim);
        command.Parameters.AddWithValue("@pNama", nama);
        command.Parameters.AddWithValue("@pAlamat", alamat);
        command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
        command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
        command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
        command.Parameters.AddWithValue("@pFoto", (object)foto ?? DBNull.Value);

        command.ExecuteNonQuery();
        trans.Commit();
    }
    catch (Exception ex)
    {
        trans.Rollback();
        throw; // tambah throw agar error kelihatan di form
    }
    finally
    {
        conn.Close();
    }
}

public void UpdateMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
{
    if (conn.State == ConnectionState.Closed)
    {
        conn.Open();
    }

    SqlCommand command = new SqlCommand("sp_UpdateMahasiswa", conn);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.AddWithValue("@pNIM", nim);
    command.Parameters.AddWithValue("@pNama", nama);
    command.Parameters.AddWithValue("@pAlamat", alamat);
    command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
    command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
    command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
    command.Parameters.AddWithValue("@pFoto", (object)foto ?? DBNull.Value);

    command.ExecuteNonQuery();
}