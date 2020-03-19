using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.IO;
using System.Diagnostics;

namespace KillService
{
    public partial class Form1 : Form
    {
        string pathBat1 = "";
        string pathBat2 = "";
        string pathTXT = "";
        string numeroPID = "";
        bool janela = false;

        public Form1()
        {
            InitializeComponent();

            lblVersao.Text = "V_" + Application.ProductVersion.ToString();
            lblVersao.Text = lblVersao.Text.Remove(lblVersao.Text.Length - 2);
        }

        private bool TestarSeServicoExiste(string nome)
        {
            //Testa se o serviço informado existe
            string status = null;
            if (string.IsNullOrEmpty(nome))
            {
                nome = "ServicoVazioTeste";
            }

            ServiceController service = new ServiceController(nome);
            bool retorno = false;

            try
            {
                status = service.Status.ToString();
                retorno = true;
            }
            catch (Exception)
            {
                retorno = false;
            }

            return retorno;
        }

        private void CriarBatIdentificadorPID() 
        {
            pathBat1 = Path.GetDirectoryName(Application.ExecutablePath) + "\\Temp\\" + DateTime.Now.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "__") + "_1.bat";
            pathTXT = pathBat1.Replace(".bat", ".txt");

            using (StreamWriter writer = new StreamWriter(pathBat1))
            {
                writer.WriteLine("sc queryex " + txbNome.Text + ">" + pathTXT);
            }
        }

        private void CriarBatExecutavelKill()
        {
            pathBat2 = Path.GetDirectoryName(Application.ExecutablePath) + "\\Temp\\" + DateTime.Now.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "__") + "_2.bat";

            using (StreamWriter writer = new StreamWriter(pathBat2))
            {
                writer.WriteLine("taskkill /f /pid " + numeroPID + Environment.NewLine + "Net stop " + txbNome.Text);
            }
        }

        private void ExecutarBat(string path) 
        {
            ProcessStartInfo inf = new ProcessStartInfo(path);
            Process proc = new Process();
            inf.CreateNoWindow = true;
            inf.UseShellExecute = false;
            proc.StartInfo = inf;
            proc.Start();
            proc.WaitForExit();
            
        }

        private void CriarPastaTemp() 
        {
            if (!Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\Temp"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Application.ExecutablePath) + "\\Temp");
            }
        }

        private string LocalizarNumeroPID() 
        {
            string retorno = "";

            string[] linhas = File.ReadAllLines(pathTXT);

            foreach (string linha in linhas)
            {
                if (linha.Contains("PID"))
                {
                    retorno = linha.Replace("PID", "").Replace(":", "").Replace(" ", "");
                }
            }

            return retorno;
        }

        private void DeletarArquivos() 
        {
            if (File.Exists(pathBat1))
            {
                File.Delete(pathBat1);
            }

            if (File.Exists(pathBat2))
            {
                File.Delete(pathBat2);
            }

            if (File.Exists(pathTXT))
            {
                File.Delete(pathTXT);
            }
        }

        private void PararServico() 
        {
            if (TestarSeServicoExiste(txbNome.Text) == false)
            {
                janela = true;
                MessageBox.Show("O Serviço informado não existe, ou não pode ser encontrado.", "Serviço Inexistente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                CriarPastaTemp();

                CriarBatIdentificadorPID();

                ExecutarBat(pathBat1);

                numeroPID = LocalizarNumeroPID();

                CriarBatExecutavelKill();

                ExecutarBat(pathBat2);

                DeletarArquivos();

                DeletarPastaTemp();

                janela = true;
                MessageBox.Show("Serviço " + txbNome.Text + " parado com sucesso!", "Concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeletarPastaTemp() 
        {
            if (Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\Temp"))
            {
                Directory.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "\\Temp", true);
            }
        }

        private void btnParar_Click(object sender, EventArgs e)
        {
            try
            {
                PararServico();
            }
            catch(Exception ex)
            {
                janela = true;
                MessageBox.Show("Não foi possível parar o Serviço " + txbNome.Text + Environment.NewLine + Environment.NewLine + "Mensagem:" + Environment.NewLine + Environment.NewLine + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txbNome_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txbNome.Text))
            {
                btnParar.Enabled = true;
            }
            else
            {
                btnParar.Enabled = false;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnParar.Enabled == true && janela == false)
            {
                try
                {
                    PararServico();
                }
                catch (Exception ex)
                {
                    janela = true;
                    MessageBox.Show("Não foi possível parar o Serviço " + txbNome.Text + Environment.NewLine + Environment.NewLine + "Mensagem:" + Environment.NewLine + Environment.NewLine + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                janela = false;
            }
        }
    }
}
