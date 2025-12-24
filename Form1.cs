using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fetchy
{
    public partial class Form1 : Form
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36";
        private const int MAX_TURNSTILE_WAIT_SECONDS = 15;
        private const int MAX_TURNSTILE_RETRY = 3;
        private const int TURNSTILE_CHECK_INTERVAL_MS = 1000;

        private readonly string _appPath;
        private readonly string _cachePath;
        private readonly string _downloadPath;

        private WebView2 _webView;
        private string _zipPath;

        public Form1()
        {
            InitializeComponent();

            _appPath = Application.StartupPath;
            _cachePath = Path.Combine(_appPath, "runtimes", "wvd");
            _downloadPath = Path.Combine(_appPath, "download");

            InitializeDirectories();

            _webView = new WebView2();
        }

        private void InitializeDirectories()
        {
            if (Directory.Exists(_cachePath))
            {
                Directory.Delete(_cachePath, true);
            }

            Directory.CreateDirectory(_cachePath);
            Directory.CreateDirectory(_downloadPath);
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            await RunUpdaterAsync();
        }

        private async Task RunUpdaterAsync()
        {
            try
            {
                await InitializeWebViewAsync();

                var (version, pageUrl) = await ScrapeUpdateDataAsync();

                string directUrl = await GetMediaFireDirectLinkAsync(pageUrl);
                await DownloadAndExtractAsync(version, directUrl);

                UpdateUI(() => lblLog.Text = "Launching new version...");
                LaunchExecutable(Path.Combine(_downloadPath, $"{version}.exe"));
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private async Task InitializeWebViewAsync()
        {
            UpdateUI(() => lblLog.Text = "Checking for updates...");

            var env = await CoreWebView2Environment.CreateAsync(null, _cachePath);
            await _webView.EnsureCoreWebView2Async(env);
            _webView.Source = new Uri("https://file.unlocktool.net/");

            bool bypassed = await WaitForTurnstileBypassAsync();

            if (!bypassed)
            {
                throw new Exception("Blocked by Cloudflare Turnstile.");
            }
        }

        private async Task<bool> WaitForTurnstileBypassAsync()
        {
            for (int attempt = 0; attempt != MAX_TURNSTILE_RETRY; attempt++)
            {
                for (int i = 0; i < MAX_TURNSTILE_WAIT_SECONDS; i++)
                {
                    await Task.Delay(TURNSTILE_CHECK_INTERVAL_MS);

                    if (!await HasTurnstileAsync())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> HasTurnstileAsync()
        {
            string js = "!!document.querySelector('.cf-turnstile') || !!window.turnstile";
            string result = await _webView.ExecuteScriptAsync(js);
            return result == "true";
        }

        private async Task<(string Version, string Url)> ScrapeUpdateDataAsync()
        {
            string js = @"
                (function() {
                    const titleElement = document.querySelector('button.nav-link.active');
                    const linkElement = document.querySelector('a[href*=""www.mediafire.com""]');
                    
                    return JSON.stringify({
                        title: titleElement ? titleElement.innerText.trim() : '',
                        link: linkElement ? linkElement.href : ''
                    });
                })();
            ";

            string raw = await _webView.ExecuteScriptAsync(js);
            raw = Regex.Unescape(raw).Trim('"');

            dynamic data = JsonConvert.DeserializeObject(raw);
            string title = data.title.ToString();
            string version = title.Split(' ')[0];
            string link = data.link.ToString();

            return (version, link);
        }

        private bool VersionExists(string version)
        {
            return Directory.GetFiles(_downloadPath, $"{version}.*")
                .Any(file => Path.GetExtension(file).Equals(".zip", StringComparison.OrdinalIgnoreCase));
        }

        private async Task DownloadAndExtractAsync(string version, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("Download URL not found.");
            }

            _zipPath = Path.Combine(_downloadPath, $"{version}.zip");

            if (VersionExists(version))
            {
                UpdateUI(() =>
                {
                    lblLog.Text = "Extracting existing update...";
                    picBox.Image = Properties.Resources.downloading;
                });

                await ExtractZipWithProgressAsync(_zipPath, _downloadPath, OnExtractProgress);
                return;
            }

            UpdateUI(() =>
            {
                lblLog.Text = "Downloading update...";
                picBox.Image = Properties.Resources.downloading;
                pProgress.Style = ProgressBarStyle.Continuous;
                pProgress.Value = 0;
            });

            await DownloadWithProgressAsync(url, _zipPath, OnDownloadProgress);
            await ExtractZipWithProgressAsync(_zipPath, _downloadPath, OnExtractProgress);
        }

        private void OnDownloadProgress(long downloaded, long total)
        {
            if (total <= 0) return;

            int percent = (int)(downloaded * 100 / total);

            UpdateUI(() =>
            {
                pProgress.Value = Math.Min(percent, 100);
                lblLog.Text = $"Downloading ({percent}%)";
            });
        }

        private void OnExtractProgress(int percent)
        {
            UpdateUI(() =>
            {
                pProgress.Value = Math.Min(percent, 100);
                lblLog.Text = $"Extracting ({percent}%)";
            });
        }

        private async Task<string> GetMediaFireDirectLinkAsync(string pageUrl)
        {
            var request = WebRequest.CreateHttp(pageUrl);
            request.Method = "GET";
            request.Timeout = 15000;
            request.AllowAutoRedirect = true;
            request.UserAgent = USER_AGENT;

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string html = await reader.ReadToEndAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var downloadButton = doc.DocumentNode.SelectSingleNode("//a[@id='downloadButton']");

                return downloadButton?.GetAttributeValue("href", null);
            }
        }

        private async Task DownloadWithProgressAsync(string url, string savePath, Action<long, long> progressCallback)
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            request.AllowAutoRedirect = true;
            request.UserAgent = USER_AGENT;

            using (var response = await request.GetResponseAsync())
            using (var input = response.GetResponseStream())
            using (var output = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                byte[] buffer = new byte[8192];
                long total = response.ContentLength;
                long downloaded = 0;

                int bytesRead;
                while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await output.WriteAsync(buffer, 0, bytesRead);
                    downloaded += bytesRead;
                    progressCallback?.Invoke(downloaded, total);
                }
            }
        }

        private async Task ExtractZipWithProgressAsync(string zipPath, string extractDir, Action<int> progressCallback)
        {
            await Task.Run(() =>
            {
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    int total = archive.Entries.Count;
                    int extracted = 0;

                    foreach (var entry in archive.Entries)
                    {
                        string destination = Path.Combine(extractDir, entry.FullName);
                        string directory = Path.GetDirectoryName(destination);

                        if (!string.IsNullOrEmpty(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        entry.ExtractToFile(destination, true);

                        extracted++;
                        int percent = (extracted * 100) / total;
                        progressCallback?.Invoke(percent);
                    }
                }
            });
        }

        private void HandleError(Exception ex)
        {
            UpdateUI(() =>
            {
                picBox.Image = Properties.Resources.error;
                lblLog.Text = ex.Message;
            });

            Task.Delay(3000).ContinueWith(_ =>
            {
                UpdateUI(() => Application.Exit());
            });
        }

        private static void LaunchExecutable(string exePath)
        {
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException($"Executable not found: {exePath}");
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to launch executable: {ex.Message}");
                throw;
            }

            Environment.Exit(0);
        }

        private void UpdateUI(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}