using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using System.Drawing;
using System.Globalization;

namespace Command.Histoday
{
    public class HistodayCommand : IBaseCommand
    {
        public UndefinedAPI CommandApi { get; private set; } = new("Histoday", "histoday");
        public string CommandName { get; private set; } = "histoday";
        public async Task Execute(ArgSchematics args)
        {
            string ImageCachePath = GenHistodayImage();
            await CommandApi.Api.SendGroupMsg(
                            args.GroupId,
                            CommandApi.GetMessageBuilder()
                                .Image(ImageCachePath, ImageSendType.LocalFile, ImageSubType.Normal).Build()
                        );
            SafeDeleteFile(ImageCachePath);
            CommandApi.Logger.Info("Command Completed");
        }
        public async Task Handle(ArgSchematics args)
        {
            if (args.Command.Equals(CommandName))
            {
                CommandApi.Logger.Info("Command Triggered");
                await Execute(args);
                CommandApi.Logger.Info("Command Completed");
            }
        }
        public void Init()
        {
            CommandApi.CommandEvent.OnCommand += Handle;
            CommandApi.Logger.Info("Command Loaded");
        }
        private void SafeDeleteFile(string tPath)
        {
            try
            {
                if (File.Exists(tPath))
                {
                    File.Delete(tPath);
                }
            }
            catch { }
        }
        private readonly Calendar ZhCNCalendar = new CultureInfo("zh-CN").DateTimeFormat.Calendar;

        private readonly Font s_dateFont = new("Simsun", 60);

        private readonly Font s_contentFont = new("Simsun", 40);

        private readonly SolidBrush s_drawBrush = new(Color.Black);
        public string GenHistodayImage()
        {
            string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"\n{ZhCNCalendar.GetDayOfWeek(DateTime.Now)}";
            string Content = CommandApi.Request.Get("https://xiaoapi.cn/API/lssdjt.php").Result;
            Bitmap bg = new(1080, 1500);
            Graphics g = Graphics.FromImage(bg);
            g.Clear(Color.White);
            g.DrawString(CurrentTime, s_dateFont, s_drawBrush, new RectangleF(100, 100, 880, 200));
            g.DrawString(Content, s_contentFont, s_drawBrush, new RectangleF(50, 400, 980, 1100));
            string ImCachePath = Path.Join(CommandApi.CachePath, $"{DateTime.Now:HH-mm-ss}.png");
            bg.Save(ImCachePath);
            g.Dispose();
            bg.Dispose();
            return ImCachePath;
        }
    }
}
