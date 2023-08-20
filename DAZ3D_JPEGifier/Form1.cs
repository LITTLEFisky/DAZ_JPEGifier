using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Drawing.Imaging;

namespace DAZ3D_JPEGifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach(ImageCodecInfo codec in codecs)
            {
                if(codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static void ResizeJpg(string path, int nWidth, int nHeight)
        {
            using (var result = new Bitmap(nWidth, nHeight))
            {
                using (var input = new Bitmap(path))
                {
                    using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                    {
                        g.DrawImage(input, 0, 0, nWidth, nHeight);
                    }
                }

                var ici = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ie => ie.MimeType == "image/jpeg");
                var eps = new EncoderParameters(1);
                eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                result.Save(path, ici, eps);
            }
        }

        private void DetectStudio4(string path)
        {
            var autodetect = File.OpenRead(path);
            using (var detect = new StreamReader(autodetect, Encoding.UTF8))
            {
                string line;
                while ((line = detect.ReadLine()) != null)
                {
                    if (line.Contains("Directory PATH="))
                    {
                        int i;
                        for (i = line.Length - 1; i >= 0; i--)
                        {
                            if (line[i] == 'A')
                                break;
                        }
                        int newlen = i - 2;
                        line = line.Substring(0, newlen);
                        line = line.Substring(20);
                        line = line + '/';
                        if (listBox1.Items.Contains(line) == false)
                        {
                            listBox1.Items.Add(line);
                        }
                    }
                }
            }
        }
        private void DetectPoser(string path)
        {
            var autodetect = File.OpenRead(path);
            using (var detect = new StreamReader(autodetect, Encoding.UTF8))
            {
                string line;
                while ((line = detect.ReadLine()) != null)
                {
                    if (line.Contains("<ContentFolder "))
                    {
                        int i;
                        for (i = line.Length - 1; i >= 0; i--)
                        {
                            if (line[i] == '\\')
                                break;
                        }
                        int newlen = i - 7;
                        line = line.Substring(0, newlen);
                        line = line.Substring(24);
                        line = line + '/';
                        if (listBox1.Items.Contains(line) == false)
                        {
                            listBox1.Items.Add(line);
                        }
                    }
                }
            }
        }
        private int ProcessSubString(string inp, string direction)
        {
            if (direction == "forward")
            {
                for (int i = 0; i < inp.Length; i++)
                {
                    if (inp[i] == '/')
                    {
                        return i + 1;
                    }
                }
            }
            else
            {
                for (int i = inp.Length - 1; i > 0; i--)
                {
                    if (inp[i] == '\\')
                    {
                        return i + 1;
                    }

                }
            }
            return inp.Length;
        }
        private int ProcessEndString(string inp)
        {
            for (int i = inp.Length - 1; i > 0; i--)
            {
                if (inp[i] == '.')
                {
                    return i + 4;
                }
            }
            return 0;
        }
        private void AnalizeAddDUF(string duf, string fename)
        {
            //check is DUF is compressed
            string duf_new = duf;
            using (var checkstream = System.IO.File.OpenRead(duf))
            using (var checkstreamReader = new StreamReader(checkstream, Encoding.UTF8, true, 1024))
            {
                string check;
                if ((check = checkstreamReader.ReadLine()) != "{")
                {
                    string sfolder = duf.Substring(0, ProcessSubString(duf, "backwards") - 1);
                    using (FileStream originalFile = System.IO.File.OpenRead(duf))
                    {
                        string currentFile = duf;
                        string outFile = duf.Remove(duf.Length - 4);
                        using (FileStream decompressedFile = System.IO.File.Create(outFile))
                        {
                            using (GZipStream decompressionStream = new GZipStream(originalFile, CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(decompressedFile);
                            }
                        }
                        duf_new = outFile;
                    }

                }
            }


            int count = 0;
            using (var filestream = System.IO.File.OpenRead(duf_new))
            using (var streamReader = new StreamReader(filestream, Encoding.UTF8, true, 1024))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (((line.Contains(".png") == true) && (line.Contains("OctaneRender") == false)))
                    {
                        bool present = false;
                        string type;
                        line = line.Replace("%20", " ");  //replacing special symbols
                        line = line.Replace("%28", "(");
                        line = line.Replace("%29", ")");
                        line = line.Replace("%27", "\'");
                        line = line.Substring(0, ProcessEndString(line));
                        line = line.Substring(ProcessSubString(line, "forward"));

                        string infile = "";
                        for (int i = 0; i < listBox1.Items.Count; i++)
                        {

                            infile = listBox1.Items[i].ToString() + line;
                            if (System.IO.File.Exists(infile) == true)
                            {
                                break;
                            }
                        }
                        

                        if (line != "")
                        {
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                if (Convert.ToString(row.Cells[0].Value) == line)
                                {
                                    present = true;
                                }
                            }

                            if (present == false)
                            {
                                dataGridView1.Rows.Add(line, new FileInfo(infile).Length);
                                count++;
                                dataGridView1.Update();
                            }
                        }

                    }
                }
            }
            Program.filepath = duf;
            if (duf_new.Contains(".duf") == false)
            {
                System.IO.File.Delete(duf_new);
            }
            label3.Text = fename;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Are you running low on disk space?\n" +
                "Tired of artists using heavy PNG textures?\n" +
                "This app is for you! It converts  heavy PNG textures\n" +
                "to lightweight JPGs with selected quality! Just \n" +
                "click \"Open *.duf\", select your DUF scene/figure\n" +
                "and press \"JPEGify\"!\n\n" +
                "Code (C)LITTLEFisky, 2023", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            if(dataGridView1.Rows.Count < 1)
            {
                MessageBox.Show("No textures found, or\n" +
                    "you forgot to open\n" +
                    "DUF first", "No", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                int saved = 0;
                Program.selector = new Form2();
                Program.selector.ShowDialog();
                if (Program.ready == true)
                {
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    ///Setting up parameters for diffuse textures
                    System.Drawing.Imaging.Encoder diffuseEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters diffuseParams = new EncoderParameters(1);
                    EncoderParameter diffuseParameter = new EncoderParameter(diffuseEncoder, Program.quality);
                    diffuseParams.Param[0] = diffuseParameter;
                    ///Setting up parameters for other textures 
                    System.Drawing.Imaging.Encoder othersEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters othersParams = new EncoderParameters(1);
                    EncoderParameter othersParameter = new EncoderParameter(othersEncoder, 95L);
                    othersParams.Param[0] = othersParameter;
                    ///Processing files from the grid
                    foreach (DataGridViewRow line in dataGridView1.Rows)
                    {
                        string infile = "";
                        int indexLib = 0;
                        for (int i = 0; i < listBox1.Items.Count; i++)
                        {

                            infile = listBox1.Items[i].ToString() + line.Cells[0].Value.ToString();
                            if (System.IO.File.Exists(infile) == true)
                            {
                                indexLib = i;
                                break;
                            }
                        }
                        string outFile = infile.Replace(".png", ".jpg");
                        
                        Bitmap image = new Bitmap(infile);
                        if ((infile.Contains("Normal") == true) || (infile.Contains("normal") == true) || (infile.Contains("Bump") == true) || (infile.Contains("bump") == true) || (infile.Contains("Height") == true) || (infile.Contains("height") == true) || (infile.Contains("N") == true) || (infile.Contains("nmap") == true))
                        {
                            image.Save(outFile, jpgEncoder, othersParams);
                            if (Program.resize == true)
                            {
                                ResizeJpg(outFile, Program.textureSize, Program.textureSize);
                            }
                        }
                        else
                        {
                            image.Save(outFile, jpgEncoder, diffuseParams);
                            if (Program.resize == true)
                            {
                                ResizeJpg(outFile, Program.textureSize, Program.textureSize);
                            }
                        }
                        line.Cells[2].Value = new FileInfo(outFile).Length;
                        image.Dispose();    

                    }
                    foreach (DataGridViewRow line in dataGridView1.Rows)
                    {
                        string infile = "";
                        for (int i = 0; i < listBox1.Items.Count; i++)
                        {

                            infile = listBox1.Items[i].ToString() + line.Cells[0].Value.ToString();
                            if (System.IO.File.Exists(infile) == true)
                            {
                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();

                                File.Delete(infile);
                            }
                        }
                    }
                    ///Processing PNG mentions in DUF file
                    //check is DUF is compressed
                    string duf_new = Program.filepath;
                    using (var checkstream = System.IO.File.OpenRead(duf_new))
                    using (var checkstreamReader = new StreamReader(checkstream, Encoding.UTF8, true, 1024))
                    {
                        string check;
                        if ((check = checkstreamReader.ReadLine()) != "{")
                        {
                            string sfolder = duf_new.Substring(0, ProcessSubString(duf_new, "backwards") - 1);
                            using (FileStream originalFile = System.IO.File.OpenRead(duf_new))
                            {
                                string currentFile = duf_new;
                                string outFile = duf_new.Remove(duf_new.Length - 4);
                                using (FileStream decompressedFile = System.IO.File.Create(outFile))
                                {
                                    using (GZipStream decompressionStream = new GZipStream(originalFile, CompressionMode.Decompress))
                                    {
                                        decompressionStream.CopyTo(decompressedFile);
                                    }
                                }
                                duf_new = outFile;
                            }

                        }
                    }

                    string tempfile = Program.filepath + ".temp";
                    using (var infile = System.IO.File.OpenRead(duf_new))
                    using (var outfile = System.IO.File.OpenWrite(tempfile))
                    using (var streamReader = new StreamReader(infile, Encoding.UTF8, true, 1024))
                    using (var streamWriter = new StreamWriter(outfile, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if ((line.Contains(".png") == true) && (line.Contains("OctaneRender") != true))
                            {
                                line = line.Replace(".png", ".jpg");
                            }
                            streamWriter.WriteLine(line);
                        }
                    }
                    if (duf_new.Contains(".duf") == false)
                    {
                        System.IO.File.Delete(duf_new);
                    }
                    File.Delete(Program.filepath);
                    File.Copy(tempfile, Program.filepath);
                    File.Delete(tempfile);
                    ///Calculating how much we saved by converting PNGs
                    foreach (DataGridViewRow line in dataGridView1.Rows)
                    {
                        int diff = Convert.ToInt32(line.Cells[1].Value.ToString()) - Convert.ToInt32(line.Cells[2].Value.ToString());
                        saved += diff;
                    }
                    MessageBox.Show($"You saved {saved / 1024} Kbytes by converting it!", "Hooray!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string path = Environment.ExpandEnvironmentVariables("%APPDATA%\\DAZ 3D\\Studio4\\ContentDirectoryManager.dsx");
            if (File.Exists(path))
            {
                DetectStudio4(path);
            }
            path = Environment.ExpandEnvironmentVariables("%APPDATA%\\DAZ 3D\\Studio4 Public Build\\ContentDirectoryManager.dsx");
            if (File.Exists(path))
            {
                DetectStudio4(path);
            }
            path = Environment.ExpandEnvironmentVariables("%APPDATA%\\Poser\\11\\LibraryPrefs.xml");
            if (File.Exists(path))
            {
                DetectPoser(path);
            }
            path = Environment.ExpandEnvironmentVariables("%APPDATA%\\Poser\\12\\LibraryPrefs.xml");
            if (File.Exists(path))
            {
                DetectPoser(path);
            }
            path = Environment.ExpandEnvironmentVariables("%APPDATA%\\Poser\\13\\LibraryPrefs.xml");
            if (File.Exists(path))
            {
                DetectPoser(path);
            }
            path = Environment.ExpandEnvironmentVariables("%APPDATA%\\Poser Pro\\10\\LibraryPrefs.xml");
            if (File.Exists(path))
            {
                DetectPoser(path);
            }
            path = Environment.ExpandEnvironmentVariables("%APPDATA%\\Poser Pro\\10\\LibraryPrefs.xml");
            if (File.Exists(path))
            {
                DetectPoser(path);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            openFileDialog1.ShowDialog();
            string fename = openFileDialog1.FileName.Substring(ProcessSubString(openFileDialog1.FileName, "backward"));
            if (openFileDialog1.FileName != " ")
            {
                dataGridView1.Rows.Clear();
                AnalizeAddDUF(openFileDialog1.FileName, fename);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://boosty.to/littlefisky/single-payment/donation/451573/target?share=target_link");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            string path = folderBrowserDialog1.SelectedPath.Replace('\\', '/') + '/';
            listBox1.Items.Add(path);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Program.info = new Form3();
            Program.info.ShowDialog();
        }
    }
}
