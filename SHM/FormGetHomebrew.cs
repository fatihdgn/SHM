﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SHM
{
    public partial class FormGetHomebrew : Form
    {
        List<Item> gamesDbs = new List<Item>();

        public FormGetHomebrew()
        {
            InitializeComponent();
        }

        public string TypeConsole { get; set; }

        public Item ItemNull()
        {
            var ItmValNull = new Item();

            ItmValNull.TitleId = "--Exit--";
            ItmValNull.TitleName = "--Exit--";
            ItmValNull.Author = "--Exit--";
            ItmValNull.Version = "--Exit--";
            ItmValNull.LastDirectLink = "--Exit--";
            ItmValNull.ReadmeLink = "--Exit--";
            return ItmValNull;
        }

        public Item Opgave { get { return lstTitles.SelectedItems.Count == 0 ? ItemNull() : (Item)lstTitles.SelectedItems[0].Tag; } }
        //public Item Opgave { get { return (Item)lstTitles.SelectedItems[0].Tag; } }
        public FormGetHomebrew(List<string> UserOpcoes)
        {
            InitializeComponent();

        }

        private void lstTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstTitles.SelectedItems.Count == 0) return;

            Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    LabelNameHomebrew.Visible = true;
                    LabelNameHomebrew.Text = Opgave.TitleName.ToString();
                    this.LabelNameHomebrew.BackColor = Color.Aqua;

                    labelAuthor.Visible = true;
                    labelAuthor.Text = Opgave.Author.ToString();
                    this.labelAuthor.BackColor = Color.Aqua;

                    linkGitHub.Visible = true;
                    labelAuthorFix.Visible = true;
                    LabelNameHomebrewFix.Visible = true;
                    linkYoutube.Visible = true;
                    linkGitHub.Visible = true;

                    linkGitHub.Tag = Opgave.ReadmeLink;
                    linkYoutube.Tag = Opgave.TitleName;

                }));
            });
        }

        private void SELECT_Click(object sender, EventArgs e)
        {
            if (lstTitles.SelectedItems.Count < 1)
            {
                MessageBox.Show("Select an option");
            }
            else
            {
                this.Close();
            }
        }


        private void FormGetDLC_Load(object sender, EventArgs e)
        {
            gamesDbs.Clear();
            if (!this.TypeConsole.Equals(""))
            {
                LoadDatabase_sync(SetConfigRegistry.ReadRegistry(TypeConsole), (vita) =>
                {
                    gamesDbs.AddRange(vita);
                    RefreshList(gamesDbs);
                });
            }
            

        }


        private void LoadDatabase_sync(string path, Action<List<Item>> result)// bool addDlc = false, bool isDLC = false, bool isPsm = false)
        {
            List<Item> dbs = new List<Item>();
            if (string.IsNullOrEmpty(path))
                result.Invoke(dbs);
            else
            {
                path = new Uri(path).ToString();

                try
                {
                    WebClient wc = new WebClient();
                    string content = wc.DownloadString(new Uri(path));
                    wc.Dispose();
                    content = Encoding.UTF8.GetString(Encoding.Default.GetBytes(content));

                    string[] lines = content.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.None);

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var a = lines[i].Split('\t');

                        if (a.Length < 2)
                        {
                            continue;

                        }

                        var itm = new Item();

                        itm.TitleId = a[0];
                        itm.TitleName = a[1];
                        itm.Author = a[2];
                        itm.Version = a[3];
                        itm.LastDirectLink = a[4];
                        itm.ReadmeLink = a[5];

                        if (itm.LastDirectLink.ToLower().Contains("https://"))
                        {
                            dbs.Add(itm);
                        }
                    }
                }
                catch (Exception err) { }
                result.Invoke(dbs);
            }
        }

        private void RefreshList(List<Item> items)
        {

            List<ListViewItem> list = new List<ListViewItem>();
            string pathHistoric = SetConfigRegistry.ReadRegistry("PathDownload") + "\\HomeBrewDonwload\\" + @"\historic.txt";

            string FindHist = "";

            foreach (var item in items)
            {
                FindHist = "";
                var a = new ListViewItem(item.TitleId);
                FindHist = Utilitary.ReadFileHistoric(pathHistoric, item.TitleId);
                if (FindHist == "Y") a.BackColor = ColorTranslator.FromHtml("#B7FF7C");

                a.SubItems.Add(item.TitleName);
                a.SubItems.Add(item.Author);
                a.SubItems.Add(item.Version);
                a.Tag = item;
                list.Add(a);
            }

            lstTitles.BeginUpdate();
            //lstTitles.Columns[4].Width = 335;
            lstTitles.Items.Clear();
            lstTitles.Items.AddRange(list.ToArray());

            lstTitles.ListViewItemSorter = new ListViewItemComparer(2, false);
            lstTitles.Sort();

            lstTitles.EndUpdate();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            lstTitles.Clear();
            this.Close();
        }

        private void linkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkGitHub.Tag.ToString());
        }

        private void linkYoutube_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //var u = new Uri("https://www.youtube.com/results?search_query=dead or alive");
            var u = new Uri("https://www.youtube.com/results?search_query="+ linkYoutube.Tag);
            System.Diagnostics.Process.Start(u.ToString());
        }
    }
}
