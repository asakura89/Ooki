using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aku;
using Newtonsoft.Json;

namespace Ooki
{
    public class MainForm : AkuForm
    {
        readonly String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constant.Filename);
        IList<Size> profiles = new List<Size> { new Size(1265, 695), new Size(850, 340), new Size(235, 26) };
        static Size currentSize;

        public MainForm()
        {
            InitializeComponent();

            btnResize.Click += btnResizeOnClick;
            btnAddProfile.Click += btnAddProfileOnClick;
            btnProfiles.Click += btnProfilesOnClick;

            LoadProfiles();
            SetCurrentSize(profiles.First());
        }

        void LoadProfiles()
        {
            if (File.Exists(filePath))
            {
                String content = File.ReadAllText(filePath);
                var savedProfiles = JsonConvert.DeserializeObject<List<String>>(content);
                profiles = savedProfiles.Select(p => { String[] splitted = p.Replace(" ", String.Empty).Split(','); return new Size(Convert.ToInt32(splitted[0]), Convert.ToInt32(splitted[1])); }).ToList();
            }
        }

        void SetCurrentSize(Size size)
        {
            currentSize = size;
            btnProfiles.Text = size.Width + Constant.XString + size.Height;
        }

        void btnResizeOnClick(object sender, EventArgs e)
        {
            try
            {
                IList<ActiveWindow> allWindows = ActiveWindowCollector.GetActiveWindows();
                IList<ActiveWindow> selectedWindowList = allWindows
                    .Where(wnd => wnd.Name.ToLowerInvariant() != "start" && wnd.Name.ToLowerInvariant() != Constant.AppName.ToLowerInvariant())
                    .ToList();

                foreach (ActiveWindow item in selectedWindowList)
                    Sizer.Set(item.Id, currentSize.Width, currentSize.Height);
            }
            catch (Exception ex)
            {
                new Alert(ex.Message).ShowDialog(this);
            }
        }

        void btnAddProfileOnClick(object sender, EventArgs e)
        {
            try
            {
                String newSize = new Prompt("Enter new Size in format width:height").ShowDialog(this);
                if (!String.IsNullOrEmpty(newSize) && newSize.Contains(":"))
                {
                    String[] splittedSize = newSize.Split(':');
                    var size = new Size(Convert.ToInt32(splittedSize[0]), Convert.ToInt32(splittedSize[1]));
                    profiles.Add(size);

                    String profilesJson = JsonConvert.SerializeObject(profiles);
                    File.WriteAllText(filePath, profilesJson);
                }
            }
            catch (Exception ex)
            {
                new Alert(ex.Message).ShowDialog();
            }
        }

        void btnProfilesOnClick(object sender, EventArgs e)
        {
            try
            {
                new ProfileForm(profiles, SelectedChangedAction).ShowDialog(this);
            }
            catch (Exception ex)
            {
                new Alert(ex.Message).ShowDialog();
            }
        }

        void SelectedChangedAction(String s)
        {
            String[] splittedSize = s.Split(Constant.XChar);
            var size = new Size(Convert.ToInt32(splittedSize[0]), Convert.ToInt32(splittedSize[1]));
            SetCurrentSize(size);
        }

        #region Windows Form Designer generated code

        private IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private AkuButton btnClose;
        private AkuButton btnResize;
        private AkuButton btnAddProfile;
        private AkuButton btnProfiles;

        private void InitializeComponent()
        {
            btnClose = new AkuButton();
            btnResize = new AkuButton();
            btnAddProfile = new AkuButton();
            btnProfiles = new AkuButton();
            SuspendLayout();
            ClientSize = new System.Drawing.Size(235, 26);

            Int32 btnCloseSize = ClientSize.Height - 4;
            Int32 rightBound = ClientSize.Width - btnCloseSize - 4;
            btnClose.Location = new Point(rightBound, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(btnCloseSize, btnCloseSize);
            btnClose.Text = "×";
            btnClose.Click += (sender, args) => { Close(); };

            rightBound = btnClose.Location.X - btnCloseSize - 4;
            btnResize.Location = new Point(rightBound, 4);
            btnResize.Name = "btnResize";
            btnResize.Size = new Size(btnCloseSize, btnCloseSize);
            btnResize.Text = "R";

            rightBound = btnResize.Location.X - btnCloseSize - 4;
            btnAddProfile.Location = new Point(rightBound, 4);
            btnAddProfile.Name = "btnAddProfile";
            btnAddProfile.Size = new Size(btnCloseSize, btnCloseSize);
            btnAddProfile.Text = "A";

            Int32 btnProfSize = btnCloseSize * 3;
            rightBound = btnAddProfile.Location.X - btnProfSize - 4;
            btnProfiles.Location = new Point(rightBound, 4);
            btnProfiles.Name = "btnProfiles";
            btnProfiles.Size = new Size(btnProfSize, btnCloseSize);
            btnProfiles.Text = "P";

            Controls.Add(btnClose);
            Controls.Add(btnResize);
            Controls.Add(btnAddProfile);
            Controls.Add(btnProfiles);
            Name = "MainForm";
            Text = Constant.AppName;
            ResumeLayout(false);
        }

        #endregion
    }

    public class ActiveWindow
    {
        public IntPtr Id { get; set; }
        public String Name { get; set; }
    }

    public static class Sizer
    {
        public static void Set(IntPtr hWnd, Int32 w, Int32 h)
        {
            var rect = new Rect();
            if (GetWindowRect(hWnd, ref rect))
                MoveWindow(hWnd, rect.X, rect.Y, w, h, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public Int32 X;
            public Int32 Y;
            public Int32 Width;
            public Int32 Height;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern Boolean GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern Boolean MoveWindow(IntPtr hWnd, Int32 x, Int32 y, Int32 width, Int32 height, Boolean repaint);
    }

    public static class ActiveWindowCollector
    {
        public static IList<ActiveWindow> GetActiveWindows()
        {
            IntPtr lShellWindow = GetShellWindow();
            var lWindows = new List<ActiveWindow>();

            EnumWindows(delegate(IntPtr hWnd, Int32 lParam)
            {
                if (hWnd == lShellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                Int32 lLength = GetWindowTextLength(hWnd);
                if (lLength == 0) return true;

                var lBuilder = new StringBuilder(lLength);
                GetWindowText(hWnd, lBuilder, lLength + 1);

                lWindows.Add(new ActiveWindow { Id = hWnd, Name = lBuilder.ToString() });
                return true;

            }, 0);

            return lWindows;
        }

        delegate Boolean EnumWindowsProc(IntPtr hWnd, Int32 lParam);

        [DllImport("USER32.DLL")]
        static extern Boolean EnumWindows(EnumWindowsProc enumFunc, Int32 lParam);

        [DllImport("USER32.DLL")]
        static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder lpString, Int32 nMaxCount);

        [DllImport("USER32.DLL")]
        static extern Int32 GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        static extern Boolean IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        static extern IntPtr GetShellWindow();
    }
}
