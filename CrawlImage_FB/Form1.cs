
﻿using RestSharp;
using System.Net;
using System.Text.RegularExpressions;

namespace CrawlImage_FB
{
    public partial class Form1 : Form
    {


        bool run;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnfile_Click(object sender, EventArgs e)
        {
            // Tạo hộp thoại chọn thư mục
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Chọn một thư mục"; // Mô tả của hộp thoại
            // Hiển thị hộp thoại chọn thư mục và xử lý kết quả
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Lấy đường dẫn được chọn
                string selectedPath = folderBrowserDialog.SelectedPath;
                // Xử lý đường dẫn được chọn ở đây
                lblocal.Text = selectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.lbprocess.Text = "Đang khởi chạy";
                });
                run = true;
                if (lblocal.Text == "Chọn đường dẫn..." || txtcookie.Text.Trim().Length == 0 || txtid.Text.Trim().Length == 0)
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin và chọn thư mục cần lưu");
                else
                {

                    Thread thr = new Thread(Doall);
                    thr.IsBackground = true;
                    thr.Start();
                }
            }
            catch (Exception)
            {
                lbprocess.Text = "Blocked by facebook or other";
            }
        }
        void SaveImge(string link, string name)
        {
            string id = string.Empty;
            string path = string.Empty;
            this.Invoke((MethodInvoker)delegate
            {
                id = this.txtid.Text;
                path = this.lblocal.Text;
            });
            string folderPath = path + $@"\{id}";
            if (!Directory.Exists(folderPath))
            {
                // Nếu chưa tồn tại, tạo mới thư mục
                Directory.CreateDirectory(folderPath);
            }
            WebClient client = new WebClient();
            link = link.Replace(@"\/", "/");
            byte[] imageData = client.DownloadData(link);
            if (imageData.Length < 10000)
                return;
            string[] files = Directory.GetFiles(folderPath); // Lấy danh sách các file trong thư mục
            this.Invoke((MethodInvoker)delegate
            {
                this.lbprocess.Text = $"{files.Length} image is collected, collecting.";
            });
            string filePath = Path.Combine(path + @"\" + id, name);
            File.WriteAllBytes(filePath, imageData);
        }
        List<string> crawlpost(int page)
        {
            try
            {
                string cc = "";
                string id = string.Empty;
                TextBox cookietb = new TextBox();
                List<string> _Lpost = new List<string>();
                this.Invoke((MethodInvoker)delegate
                {
                    cookietb = this.txtcookie;
                    id = txtid.Text;

                });
                foreach (var line in cookietb.Lines)
                {
                    cc += line;
                }
                string[] cc_vl = cc.Split(';');
                // Tạo yêu cầu HTTP GET đến URL cần truy cập
                var client = new RestClient($"http://mbasic.facebook.com/{id}/photoset/pb.{id}.-2207520000./?owner_id={id}&offset={page}");
                var request = new RestRequest($"http://mbasic.facebook.com/{id}/photoset/pb.{id}.-2207520000./?owner_id={id}&offset={page}", Method.Get);
                request.AddHeader(@"Host", @"mbasic.facebook.com");
                request.AddHeader(@"Connection", @"Close");
                request.AddHeader(@"accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.AddHeader(@"accept-encoding", @"gzip, deflate, br");
                request.AddHeader(@"sec-ch-ua-mobile", @"?0");
                request.AddHeader(@"sec-fetch-dest", @"document");
                request.AddHeader(@"sec-fetch-mode", @"navigate");
                request.AddHeader(@"sec-fetch-site", @"none");
                request.AddHeader(@"sec-fetch-user", @"?1");
                request.AddHeader(@"upgrade-insecure-requests", @"1");
                request.AddHeader(@"user-agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.50");
                // MessageBox.Show(request.Address.ToString());

                // Thêm cookie vào yêu cầu
                List<Cookie> cookies = new List<Cookie>();
                foreach (var item in cc_vl)
                {
                    cookies.Add(new Cookie(item.Split('=')[0].Trim(), item.Split('=')[1].Trim()));
                }
                foreach (Cookie cookie in cookies)
                {
                    client.AddCookie(cookie.Name, cookie.Value, "/", "mbasic.facebook.com");
                }
                RestResponse response = client.Execute(request);
                // RestResponse response =client.Execute(request);
                string? _content = response.Content;
                Regex regex = new Regex("href=\"/photo.php");
                Regex rg = new Regex("id=\"login_error\"");
                if (_content != null)
                {
                    if (regex.IsMatch(_content))
                    {
                        string[] lan1 = _content.Split("href=\"/photo.php?");
                        for (int i = 1; i < lan1.Length; i++)
                        {
                            _Lpost.Add(lan1[i].Split('"')[0]);
                        }
                        return _Lpost;
                    }
                    else if (rg.IsMatch(_content))
                    {
                        MessageBox.Show("Cookie error", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    return null;

                }
                //done
            }
            catch (Exception)
            {
                MessageBox.Show("Kiểm tra lại định dạng cookie");
                Invoke(new Action(() =>
                {
                    lbprocess.Text = "";
                }));

            }
            return null;
        }
        string Source(string link)
        {
            List<string> _source = new List<string>();
            string cc = "";
            List<string> _Lpost = new();
            TextBox cookietb = new TextBox();
            this.Invoke((MethodInvoker)delegate
            {
                cookietb = this.txtcookie;
            });
            foreach (var line in cookietb.Lines)
            {
                cc += line;
            }
            string[] cc_vl = cc.Split(';');
            // Tạo yêu cầu HTTP GET đến URL cần truy cập

            var client = new RestClient($"https://www.facebook.com/photo.php?{link}");
            var request = new RestRequest($"https://www.facebook.com/photo.php?{link}", Method.Get);
            request.AddHeader(@"accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.AddHeader(@"accept-encoding", @"gzip, deflate, br");
            request.AddHeader(@"accept-language", @"vi,en-US;q=0.9,en;q=0.8");
            request.AddHeader(@"cache-control", @"max-age=0");
            request.AddHeader(@"sec-ch-prefers-color-scheme", @"dark");
            request.AddHeader(@"sec-ch-ua", "\"Chromium\";v=\"110\", \"Not A(Brand\";v=\"24\", \"Microsoft Edge\";v=\"110\"");
            request.AddHeader(@"sec-ch-ua-mobile", @"?0");
            request.AddHeader(@"sec-ch-ua-platform", "\"Windows\"");
            request.AddHeader(@"sec-fetch-dest", @"document");
            request.AddHeader(@"sec-fetch-mode", @"navigate");
            request.AddHeader(@"sec-fetch-site", @"none");
            request.AddHeader(@"sec-fetch-user", @"?1");
            request.AddHeader(@"upgrade-insecure-requests", @"1");
            request.AddHeader(@"user-agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.50");
            // MessageBox.Show(request.Address.ToString());

            // Thêm cookie vào yêu cầu
            List<Cookie> cookies = new List<Cookie>();
            foreach (var item in cc_vl)
            {
                cookies.Add(new Cookie(item.Split('=')[0].Trim(), item.Split('=')[1].Trim()));
            }
            foreach (Cookie cookie in cookies)
            {
                client.AddCookie(cookie.Name, cookie.Value, "/", "facebook.com");
            }
            RestResponse response = client.Execute(request);
            Regex regex = new Regex("https:");
            string[] _content = response.Content.Split("prefetch_uris_v2");
            string[] cat2 = _content[1].Split("label");
            string[] cat3 = cat2[0].Split('"');
            foreach (var tonghop in cat3)
            {
                if (regex.IsMatch(tonghop))
                {
                    return tonghop;
                }
            }
            return null;
        }
        void Doall()
        {
            int i = 0;
            while (crawlpost(i) != null && run == true)
            {
                List<Thread> threads = new List<Thread>();
                foreach (string s in crawlpost(i))
                {
                    Thread thr = new Thread(() =>
                    {
                        SaveImge(Source(s), $"{DateTime.Now.ToString("yyyyMMddHHmmssffffff").Trim()}.jpg");
                    });
                    thr.IsBackground = true;
                    thr.Start();
                    threads.Add(thr);
                }
                foreach (var threat in threads)
                {
                    threat.Join();
                }
                i += 12;
            }

            string id = ""; string path = "";
            this.Invoke((MethodInvoker)delegate
            {
                id = this.txtid.Text;
                path = this.lblocal.Text;
            });
            string folderPath = path + $@"\{id}";
            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                this.Invoke((MethodInvoker)delegate
                {
                    this.lbprocess.Text = $"{files.Length} image has been collected, Thanks for using";
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.lbprocess.Text = $"Crawl thất bại,có thể do bị fb đánh dấu spam or đầu vào cookie bị lỗi :)))";
                });
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (lbprocess.Text != "..." && lbprocess.Text != "Đã chạy đâu mà dừng :)))")
            {
                lock (typeof(Form1))
                {
                    run = false;
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.lbprocess.Text = "Đã dừng";
                    });
                }
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.lbprocess.Text = "Đã chạy đâu mà dừng :)))";
                });
            }

        }

    }
}


