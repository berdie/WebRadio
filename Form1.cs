using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using NAudio.Wave;

namespace RadioStreamer
{
    public partial class Form1 : MaterialForm
    {
        private readonly MaterialSkin.MaterialSkinManager materialSkinManager;
        private List<RadioStation> radioStations = new List<RadioStation>();
        private IWavePlayer waveOut;
        private MediaFoundationReader mediaReader;
        private bool isPlaying = false;

        public Form1()
        {
            InitializeComponent();
            materialButton1.Click += (s, e) => PlaySelectedStation();
            materialButton2.Click += (s, e) => StopPlayback();
            materialSkinManager = MaterialSkin.MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkin.MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new MaterialSkin.ColorScheme(
                MaterialSkin.Primary.Indigo700, MaterialSkin.Primary.Indigo500,
                MaterialSkin.Primary.Indigo100, MaterialSkin.Accent.Pink700,
                MaterialSkin.TextShade.WHITE
            );
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Carica le stazioni radio dal file CSV
            LoadRadioStations();

            // Popola il ComboBox con le stazioni
            materialComboBox1.DataSource = radioStations;

            // Inizializza il player audio
            waveOut = new WaveOut();

            // Imposta il volume iniziale
            if (materialSlider2 != null)
            {
                waveOut.Volume = materialSlider2.Value / 100f;
            }
        }

        private void LoadRadioStations()
        {
            try
            {
                string filePath = Path.Combine(Application.StartupPath, "InternetRadio.csv");
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);

                    // Salta l'intestazione
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        string[] parts = line.Split(',');
                        if (parts.Length >= 3)
                        {
                            int id = int.Parse(parts[0]);
                            radioStations.Add(new RadioStation(id, parts[1].Trim(), parts[2].Trim()));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File InternetRadio.csv non trovato!", "Errore",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento delle stazioni radio: {ex.Message}",
                    "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Arresta la riproduzione corrente quando l'utente cambia stazione
            StopPlayback();
        }

        private void materialSlider2_Click(object sender, EventArgs e)
        {
            // Usa il materialSlider2 come controllo del volume
            if (waveOut != null)
            {
                waveOut.Volume = materialSlider2.Value / 100f;
            }
        }

        // Aggiungiamo un metodo per avviare la riproduzione
        public void PlaySelectedStation()
        {
            if (materialComboBox1.SelectedItem == null) return;

            try
            {
                StopPlayback();

                RadioStation selectedStation = (RadioStation)materialComboBox1.SelectedItem;
                mediaReader = new MediaFoundationReader(selectedStation.Url);
                waveOut.Init(mediaReader);
                waveOut.Play();
                isPlaying = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la riproduzione: {ex.Message}",
                    "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo per arrestare la riproduzione
        public void StopPlayback()
        {
            if (isPlaying)
            {
                waveOut.Stop();
                mediaReader.Dispose();
                mediaReader = null;
                isPlaying = false;
            }
        }

        // Assicuriamoci di liberare le risorse quando il form viene chiuso
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopPlayback();
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
            base.OnFormClosing(e);
        }
    }

    // Aggiornata la classe RadioStation per includere l'ID
    public class RadioStation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public RadioStation(int id, string name, string url)
        {
            Id = id;
            Name = name;
            Url = url;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
