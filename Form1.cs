using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SHA256VeriSifreleme
{

    public partial class Form1 : Form
    {
        SqlConnection baglanti = new SqlConnection(@"Data Source=LAPTOP-FB9086OP;Initial Catalog=SHA256;Integrated Security=True");
        public Form1()
        {
            InitializeComponent();
            textBoxSifre.PasswordChar = '*';
        }


        private string sha256KoduOlustur(string s)
        {
            var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private void btnKaydol_Click(object sender, EventArgs e)
        {
            if (textBoxKullaniciAdi.Text.ToString().Length == 0
                    || textBoxSifre.Text.ToString().Length == 0)
            {
                MessageBox.Show("Alanlar boş bırakılmaz!");
                return;
            }

            string sorgu = "SELECT KullaniciAdi FROM Kullanici WHERE KullaniciAdi = @P1";

            try
            {
                baglanti.Open();
                SqlCommand sqlCommand = new SqlCommand(sorgu, baglanti);
                sqlCommand.Parameters.AddWithValue("@P1", textBoxKullaniciAdi.Text);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                bool yeniKullaniciEkle = false;

                if (reader.HasRows)
                {
                    // Bu isimde bir kullanıcı varsa veritabanına ekleme yapılmaz
                    MessageBox.Show(textBoxKullaniciAdi.Text + " isminde bir kullanıcı zaten mevcut, ekleme yapılamadı!");
                }
                else
                {
                    // Veritabına yeni kullnaıcıyı ekleyebiliriz
                    yeniKullaniciEkle = true;
                }
                reader.Close();

                if (yeniKullaniciEkle)
                {
                    sqlCommand = new SqlCommand("INSERT INTO Kullanici VALUES (@P1, @P2)", baglanti);
                    sqlCommand.Parameters.AddWithValue("@P1", textBoxKullaniciAdi.Text);
                    sqlCommand.Parameters.AddWithValue("@P2", sha256KoduOlustur(textBoxSifre.Text));
                    sqlCommand.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("VT baglantisinda dorun oluştu, hata kodu: H001\n" + ex.Message);
            }
            finally
            {
                if (baglanti != null)
                    baglanti.Close();
            }
        }

        private void btnGiris_Click(object sender, EventArgs e)
        {
            if (textBoxKullaniciAdi.Text.ToString().Length == 0
                    || textBoxSifre.Text.ToString().Length == 0)
            {
                MessageBox.Show("Alanlar boş bırakılmaz!");
                return;
            }

            try
            {
                baglanti.Open();
                string sorgu = "SELECT KullaniciAdi, Sifre FROM Kullanici WHERE KullaniciAdi = @P1 " +
                               "AND Sifre = @P2";
                SqlCommand sqlCommand = new SqlCommand(sorgu, baglanti);
                sqlCommand.Parameters.AddWithValue("@P1", textBoxKullaniciAdi.Text);
                sqlCommand.Parameters.AddWithValue("@P2", sha256KoduOlustur(textBoxSifre.Text));
                SqlDataReader reader = sqlCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    MessageBox.Show("Kullanıcı adı ve şifre Doğru! Sisteme Hoş Geldiniz!!");
                }
                else
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("VT baglantisinda dorun oluştu, hata kodu: H002\n" + ex.Message);
            }
            finally
            {
                if (baglanti != null)
                    baglanti.Close();
            }
        }
    }
}
