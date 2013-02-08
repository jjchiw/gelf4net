using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using System.Net;

namespace SandboxWindow
{
	public partial class RadomSentenceForm : Form
	{
		protected static readonly ILog log = LogManager.GetLogger(typeof(RadomSentenceForm));

		public RadomSentenceForm()
		{
			log4net.Config.XmlConfigurator.Configure();
			InitializeComponent();
			this.FormBorderStyle =  FormBorderStyle.Fixed3D;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			log.Debug("Starting the randomizer sentence");
			Size = new Size(800, 600);
			LogRandomSentence();
		}
		
		private string GetPath(string path, string file)
		{
			return String.Concat(path, Path.DirectorySeparatorChar, file);
		}

		private void LogRandomSentence()
		{
            var sentence = GetRandomSentence();

            log.Debug(String.Format("Randomizer Sentence: {0}", sentence));
		}

        private string GetRandomSentence()
        {
            var syntaxes = File.ReadAllLines(GetPath("Resources", "syntax.txt"));
            var nouns = File.ReadAllLines(GetPath("Resources", "nouns.txt"));
            var verbs = File.ReadAllLines(GetPath("Resources", "verbs.txt"));
            var adjectives = File.ReadAllLines(GetPath("Resources", "adjectives.txt"));
            var adverbs = File.ReadAllLines(GetPath("Resources", "adverbs.txt"));
            var rand = new Random();

            var sentence = syntaxes[rand.Next(syntaxes.Length)];
            var complete = false;
            while (!complete)
            {
                var index = sentence.IndexOf("[Noun]");
                if (index > 0)
                {
                    sentence = sentence.Remove(index, "[Noun]".Length);
                    sentence = sentence.Insert(index, nouns[rand.Next(nouns.Length)].Trim());
                    continue;
                }
                index = sentence.IndexOf("[Verb]");
                if (index > 0)
                {
                    sentence = sentence.Remove(index, "[Verb]".Length);
                    sentence = sentence.Insert(index, verbs[rand.Next(verbs.Length)].Trim());
                    continue;
                }
                index = sentence.IndexOf("[Adverb]");
                if (index > 0)
                {
                    sentence = sentence.Remove(index, "[Adverb]".Length);
                    sentence = sentence.Insert(index, adverbs[rand.Next(adverbs.Length)].Trim());
                    continue;
                }
                index = sentence.IndexOf("[Adjective]");
                if (index > 0)
                {
                    sentence = sentence.Remove(index, "[Adjective]".Length);
                    sentence = sentence.Insert(index, adjectives[rand.Next(adjectives.Length)].Trim());
                    continue;
                }
                complete = true;
            }
            var cutIndex = 0;
            var readChars = 0;
            const int maxWidth = 32;
            while (sentence.Length > readChars + maxWidth)
            {
                cutIndex = sentence.Substring(cutIndex, maxWidth).LastIndexOf(" ");
                sentence = sentence.Remove(readChars + cutIndex, 1);
                sentence = sentence.Insert(readChars + cutIndex, "\n");
                readChars += cutIndex;
            }

            label1.Text = sentence.ToUpper();
            return label1.Text;
        }

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			log.Debug("Closing the app");
		}

        private void button1_Click(object sender, EventArgs e)
        {
            LogRandomSentence();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                throw new ApplicationException("An exception just got thrown!!!!");
            }
            catch (Exception ex)
            {
                log.Error("An error was thrown", ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            log.Debug(new
            {
                Message = GetRandomSentence(),
                CustomProp = "Another Prop"
            });
        }
	}
}