/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Net;

namespace TreeViewer
{
    public class AboutWindow : ChildWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            if (Modules.IsMac)
            {
                this.PlatformImpl.GetType().InvokeMember("SetTitleBarColor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, this.PlatformImpl, new object[] { Avalonia.Media.Color.FromRgb(243, 243, 243) });
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<TextBlock>("VersionTextBlock").Text = "Version " + Program.Version;

            this.FindControl<TextBlock>("GitHubTextBlock").PointerPressed += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "https://treeviewer.org",
                    UseShellExecute = true
                });
            };

            this.FindControl<Button>("CloseButton").Click += (s, e) =>
            {
                this.Close();
            };

            this.FindControl<Button>("CheckUpdatesButton").Click += async (s, e) =>
            {
                string releaseJson;

                ProgressWindow win = new ProgressWindow() { ProgressText = "Checking for updates..." };
                _ = win.ShowDialog2(this);

                try
                {


                    releaseJson = await Modules.HttpClient.DownloadStringTaskAsync("https://api.github.com/repos/" + GlobalSettings.ProgramRepository + "/releases");

                    ReleaseHeader[] releases = System.Text.Json.JsonSerializer.Deserialize<ReleaseHeader[]>(releaseJson);

                    win.Close();

                    Version currVers = new Version(Program.Version);

                    bool found = false;

                    for (int i = 0; i < releases.Length; i++)
                    {
                        try
                        {
                            if (!releases[i].prerelease)
                            {
                                Version version = new Version(releases[i].tag_name.Substring(1));

                                if (version > currVers)
                                {
                                    found = true;

                                    UpdateWindow box = new UpdateWindow(releases[i].name, releases[i].html_url);
                                    await box.ShowDialog2(this);
                                    break;
                                }
                            }
                        }
                        catch { }
                    }

                    if (!found)
                    {
                        MessageBox box = new MessageBox("Check for updates", "The program is up to date!", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Tick);
                        await box.ShowDialog2(this);
                    }
                }
                catch (Exception ex)
                {
                    win.Close();

                    MessageBox box = new MessageBox("Attention", "An error occurred while checking for updates!\n" + ex.Message);
                    await box.ShowDialog2(this);
                }
            };
        }
    }
    internal class ReleaseHeader
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public bool prerelease { get; set; }
        public string html_url { get; set; }
    }
}
